using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatsdClient.Bufferize;

namespace StatsdClient
{
#pragma warning disable CS1591
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "See ObsoleteAttribute.")]
    [ObsoleteAttribute("This class will become private in a future release.\n" +
        "You can use instead `DogStatsdService` or `DogStatsd` which provides automatic metrics" +
        " buffering with asynchronous calls (metrics are added to a queue and another thread send them).")]
    public class Statsd2
    {
        private const string _entityIdInternalTagKey = "dd.internal.entity_id";
        private static readonly string[] EmptyStringArray = new string[0];
        private readonly string _prefix;
        private readonly string[] _constantTags;
        private readonly Telemetry _optionalTelemetry;
        private List<string> _commands = new List<string>();

        internal Statsd2(
                    StatsBufferize udp,
                    IRandomGenerator randomGenerator,
                    IStopWatchFactory stopwatchFactory,
                    string prefix,
                    string[] constantTags,
                    Telemetry optionalTelemetry)
        {
            StopwatchFactory = stopwatchFactory;
            Udp = udp;
            RandomGenerator = randomGenerator;
            if (prefix != null && prefix.Length > 0)
            {
                _prefix = "." + prefix;
            }
            _optionalTelemetry = optionalTelemetry;

            string entityId = Environment.GetEnvironmentVariable(StatsdConfig.DD_ENTITY_ID_ENV_VAR);

            if (string.IsNullOrEmpty(entityId))
            {
                // copy array to prevent changes, coalesce to empty array
                _constantTags = constantTags?.ToArray() ?? EmptyStringArray;
            }
            else
            {
                var entityIdTags = new[] { $"{_entityIdInternalTagKey}:{entityId}" };
                _constantTags = constantTags == null ? entityIdTags : constantTags.Concat(entityIdTags).ToArray();
            }
        }

        public bool TruncateIfTooLong { get; set; }

        public List<string> Commands
        {
            get { return _commands; }
            private set { _commands = value; }
        }

        private IStopWatchFactory StopwatchFactory { get; set; }

        private StatsBufferize Udp { get; set; }

        private IRandomGenerator RandomGenerator { get; set; }

        public void Send(string title, string text, string alertType = null, string aggregationKey = null, string sourceType = null, int? dateHappened = null, string priority = null, string hostname = null, string[] tags = null, bool truncateIfTooLong = false)
        {
            truncateIfTooLong = truncateIfTooLong || TruncateIfTooLong;
            Send(Event.GetCommand(title, text, alertType, aggregationKey, sourceType, dateHappened, priority, hostname, _constantTags, tags, truncateIfTooLong));
            _optionalTelemetry?.OnEventSent();
        }



        public void Send(string name, int status, int? timestamp = null, string hostname = null, string[] tags = null, string serviceCheckMessage = null, bool truncateIfTooLong = false)
        {
            truncateIfTooLong = truncateIfTooLong || TruncateIfTooLong;
            Send(ServiceCheck.GetCommand(name, status, timestamp, hostname, _constantTags, tags, serviceCheckMessage, truncateIfTooLong));
            _optionalTelemetry?.OnServiceCheckSent();
        }


        public void Send<TCommandType, T>(string name, T value, double sampleRate = 1.0, string[] tags = null)
            where TCommandType : Metric
        {
            if (RandomGenerator.ShouldSend(sampleRate))
            {
                Send(Metric.GetCommand<TCommandType, T>(_prefix, name, value, sampleRate, _constantTags, tags));
                _optionalTelemetry?.OnMetricSent();
            }
        }


        public void Send(Message command)
        {
            try
            {
                // clear buffer (keep existing behavior)
                if (Commands.Count > 0)
                {
                    Commands = new List<string>();
                }
                Udp.Send(command);
              
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }





        public void Send(Action actionToTime, string statName, double sampleRate = 1.0, string[] tags = null)
        {
            var stopwatch = StopwatchFactory.Get();

            try
            {
                stopwatch.Start();
                actionToTime();
            }
            finally
            {
                stopwatch.Stop();
                Send<Timing, int>(statName, stopwatch.ElapsedMilliseconds(), sampleRate, tags);
            }
        }

        private static string EscapeContent(string content)
        {
            return content
                .Replace("\r", string.Empty)
                .Replace("\n", "\\n");
        }

        private static string ConcatTags(string[] constantTags, string[] tags)
        {
            // avoid dealing with null arrays
            constantTags = constantTags ?? EmptyStringArray;
            tags = tags ?? EmptyStringArray;

            if (constantTags.Length == 0 && tags.Length == 0)
            {
                return string.Empty;
            }

            var allTags = constantTags.Concat(tags);
            string concatenatedTags = string.Join(",", allTags);
            return $"|#{concatenatedTags}";
        }

        private static string TruncateOverage(string str, int overage)
        {
            return str.Substring(0, str.Length - overage);
        }

        public class Counting : Metric
        {
        }

        public class Timing : Metric
        {
        }

        public class Gauge : Metric
        {
        }

        public class Histogram : Metric
        {
        }

        public class Distribution : Metric
        {
        }

        public class Meter : Metric
        {
        }

        public class Set : Metric
        {
        }

        public static int nbAlloc = 0;

        //public static readonly ConcurrentQueue<byte[]> Poll = new ConcurrentQueue<byte[]>();
        public static readonly ConcurrentQueue<StringBuilder> Poll = new ConcurrentQueue<StringBuilder>();

        public abstract class Metric : ICommandType
        {
            private static readonly Dictionary<Type, string> _commandToUnit = new Dictionary<Type, string>
                                                                {
                                                                    { typeof(Counting), "c" },
                                                                    { typeof(Timing), "ms" },
                                                                    { typeof(Gauge), "g" },
                                                                    { typeof(Histogram), "h" },
                                                                    { typeof(Distribution), "d" },
                                                                    { typeof(Meter), "m" },
                                                                    { typeof(Set), "s" },
                                                                };

            public static Message GetCommand<TCommandType, T>(string prefix, string name, T value, double sampleRate, string[] tags)
                where TCommandType : Metric
            {
                return GetCommand<TCommandType, T>(prefix, name, value, sampleRate, null, tags);
            }

            static ArraySegment<byte> tmp = new ArraySegment<byte>(Encoding.UTF8.GetBytes("test:1|c|#KEY:VALUE"));


            static int WriteToBuff(byte[] buffer, int offset, int value)
            {
                var start = offset;
                if (value < 0)
                {
                    value = -value;
                    buffer[offset++] = (byte)'-';
                }
                var nbDigit = 0;
                for (var v = value; v != 0; v /= 10)
                {
                    ++nbDigit;
                }

                var index = offset + nbDigit - 1;
                for (var v = value; v != 0; v /= 10)
                {
                    var digit = v % 10;
                    buffer[index--] = (byte)('0' + (char)digit);
                }

                return (nbDigit + offset) - start;
            }

            public static Message GetCommand2<TCommandType, T>(string prefix, string name, T value, double sampleRate, string[] constantTags, string[] tags)
                where TCommandType : Metric
            {
                throw new NotImplementedException();

                // byte[] buffer = null;
                // if (!Poll.TryDequeue(out buffer))
                // {

                //     buffer = new byte[100];
                //     ++nbAlloc;
                // }

                // var enc = Encoding.UTF8;
                // var offset = 0;

                // if (!String.IsNullOrEmpty(prefix))
                // {
                //     offset += enc.GetBytes(prefix, 0, prefix.Length, buffer, offset);
                // }

                // offset += enc.GetBytes(name, 0, name.Length, buffer, offset);
                // buffer[offset++] = (byte)':';

                // //  var valueStr = string.Format(CultureInfo.InvariantCulture, "{0}", value);
                // //  offset += enc.GetBytes(valueStr, 0, valueStr.Length, buffer, offset);
                // //offset += WriteToBuff(buffer, offset, Convert.ToInt32(value));
                // buffer[offset++] = (byte)'1';

                // buffer[offset++] = (byte)'|';

                // string unit = _commandToUnit[typeof(TCommandType)];
                // offset += enc.GetBytes(unit, 0, unit.Length, buffer, offset);

                // if (sampleRate != 1.0)
                // {
                //     var smapleStr = string.Format(CultureInfo.InvariantCulture, "|@{0}", sampleRate);
                //     offset += enc.GetBytes(smapleStr, 0, smapleStr.Length, buffer, offset);
                // }

                // if (constantTags.Length > 0 || (tags != null && tags.Length > 0))
                // {
                //     buffer[offset++] = (byte)'|';
                //     buffer[offset++] = (byte)'#';
                //     bool hasTag = false;

                //     foreach (var tag in constantTags)
                //     {
                //         if (hasTag)
                //         {
                //             buffer[offset++] = (byte)',';
                //         }
                //         hasTag = true;

                //         offset += enc.GetBytes(tag, 0, tag.Length, buffer, offset);
                //     }

                //     if (tags != null)
                //     {
                //         foreach (var tag in tags)
                //         {
                //             if (hasTag)
                //             {
                //                 buffer[offset++] = (byte)',';
                //             }
                //             hasTag = true;

                //             offset += enc.GetBytes(tag, 0, tag.Length, buffer, offset);
                //         }
                //     }
                // }

                // return new Message{ buffer = new ArraySegment<byte>(buffer, 0, offset)};
            }

            public static Message GetCommand<TCommandType, T>(string prefix, string name, T value, double sampleRate, string[] constantTags, string[] tags)
            where TCommandType : Metric
            {
                if (!Poll.TryDequeue(out var builder))
                {
                    builder = new StringBuilder();
                    ++nbAlloc;
                }
                else
                {
                    builder.Clear();
                }

                if (!string.IsNullOrEmpty(prefix))
                {
                    builder.Append(prefix);
                }

                string full_name = name;
                string unit = _commandToUnit[typeof(TCommandType)];
                //   var allTags = ConcatTags(constantTags, tags);

                // builder.AppendFormat(
                //     CultureInfo.InvariantCulture,
                //     "{0}:{1}|{2}",
                //     full_name,
                //     value,
                //     unit);

                builder.Append(full_name);
                builder.Append(':');

                // builder.AppendFormat(
                //     CultureInfo.InvariantCulture,
                //     "{0}|{1}",
                //     value,
                //     unit);


                 builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", value);
                 

                 builder.Append('|');
                 builder.Append(unit);

                if (sampleRate != 1.0)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "|@{0}", sampleRate);
                }

                if (constantTags.Length > 0 || tags.Length > 0)
                {
                    builder.Append("|#");
                    bool hasTag = false;
                    foreach (var t in constantTags)
                    {
                        if (hasTag)
                            builder.Append(',');
                        hasTag = true;
                        builder.Append(t);
                    }

                    foreach (var t in tags)
                    {
                        if (hasTag)
                            builder.Append(',');
                        hasTag = true;
                        builder.Append(t);
                    }
                }

                //    throw new NotImplementedException();
                return new Message { buffer = builder };
            }
        }

        public class Event : ICommandType
        {
            private const int MaxSize = 8 * 1024;

            public static Message GetCommand(string title, string text, string alertType, string aggregationKey, string sourceType, int? dateHappened, string priority, string hostname, string[] tags, bool truncateIfTooLong = false)
            {
                return GetCommand(title, text, alertType, aggregationKey, sourceType, dateHappened, priority, hostname, null, tags, truncateIfTooLong);
            }

            public static Message GetCommand(string title, string text, string alertType, string aggregationKey, string sourceType, int? dateHappened, string priority, string hostname, string[] constantTags, string[] tags, bool truncateIfTooLong = false)
            {
                return new Message();

                // string processedTitle = EscapeContent(title);
                // string processedText = EscapeContent(text);
                // string result = string.Format(CultureInfo.InvariantCulture, "_e{{{0},{1}}}:{2}|{3}", processedTitle.Length.ToString(), processedText.Length.ToString(), processedTitle, processedText);
                // if (dateHappened != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|d:{0}", dateHappened);
                // }

                // if (hostname != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|h:{0}", hostname);
                // }

                // if (aggregationKey != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|k:{0}", aggregationKey);
                // }

                // if (priority != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|p:{0}", priority);
                // }

                // if (sourceType != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|s:{0}", sourceType);
                // }

                // if (alertType != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|t:{0}", alertType);
                // }

                // result += ConcatTags(constantTags, tags);

                // if (result.Length > MaxSize)
                // {
                //     if (truncateIfTooLong)
                //     {
                //         var overage = result.Length - MaxSize;
                //         if (title.Length > text.Length)
                //         {
                //             title = TruncateOverage(title, overage);
                //         }
                //         else
                //         {
                //             text = TruncateOverage(text, overage);
                //         }

                //         return GetCommand(title, text, alertType, aggregationKey, sourceType, dateHappened, priority, hostname, tags, true);
                //     }
                //     else
                //     {
                //         throw new Exception(string.Format("Event {0} payload is too big (more than 8kB)", title));
                //     }
                // }

                //  return result;
            }
        }

        public class ServiceCheck : ICommandType
        {
            private const int MaxSize = 8 * 1024;

            public static Message GetCommand(string name, int status, int? timestamp, string hostname, string[] tags, string serviceCheckMessage, bool truncateIfTooLong = false)
            {
                return GetCommand(name, status, timestamp, hostname, null, tags, serviceCheckMessage, truncateIfTooLong);
            }

            public static Message GetCommand(string name, int status, int? timestamp, string hostname, string[] constantTags, string[] tags, string serviceCheckMessage, bool truncateIfTooLong = false)
            {
                return new Message();
                // string processedName = EscapeName(name);
                // string processedMessage = EscapeMessage(serviceCheckMessage);

                // string result = string.Format(CultureInfo.InvariantCulture, "_sc|{0}|{1}", processedName, status);

                // if (timestamp != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|d:{0}", timestamp);
                // }

                // if (hostname != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|h:{0}", hostname);
                // }

                // result += ConcatTags(constantTags, tags);

                // // Note: this must always be appended to the result last.
                // if (processedMessage != null)
                // {
                //     result += string.Format(CultureInfo.InvariantCulture, "|m:{0}", processedMessage);
                // }

                // if (result.Length > MaxSize)
                // {
                //     if (!truncateIfTooLong)
                //     {
                //         throw new Exception(string.Format("ServiceCheck {0} payload is too big (more than 8kB)", name));
                //     }

                //     var overage = result.Length - MaxSize;

                //     if (processedMessage == null || overage > processedMessage.Length)
                //     {
                //         throw new ArgumentException(string.Format("ServiceCheck name is too long to truncate, payload is too big (more than 8Kb) for {0}", name), "name");
                //     }

                //     var truncMessage = TruncateOverage(processedMessage, overage);
                //     return GetCommand(name, status, timestamp, hostname, tags, truncMessage, true);
                // }

                // return result;
            }

            // Service check name string, shouldn’t contain any |
            private static string EscapeName(string name)
            {
                name = EscapeContent(name);

                if (name.Contains("|"))
                {
                    throw new ArgumentException("Name must not contain any | (pipe) characters", "name");
                }

                return name;
            }

            private static string EscapeMessage(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    return EscapeContent(message).Replace("m:", "m\\:");
                }

                return message;
            }
        }
    }
}
