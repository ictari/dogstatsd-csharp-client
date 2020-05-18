using System;
using StatsdClient.Bufferize;

namespace StatsdClient
{
    /// <summary>
    /// DogStatsdService is a <a href="https://docs.datadoghq.com/developers/dogstatsd/?tab=net">DogStatsD client</a>.
    /// Dispose must be called to flush all the metrics.
    /// </summary>
    public class DogStatsdService : IDogStatsd, IDisposable
    {
        private StatsdBuilder _statsdBuilder = new StatsdBuilder(new StatsBufferizeFactory());
        private Statsd2 _statsD;
        private StatsdData _statsdData;
        //private string _prefix;
        private StatsdConfig _config;

        /// <summary>
        /// Gets the telemetry counters
        /// </summary>
        /// <value>The telemetry counters.</value>
        public ITelemetryCounters TelemetryCounters => _statsdData?.Telemetry;

        /// <summary>
        /// Configures the instance.
        /// Must be called before any other methods.
        /// </summary>
        /// <param name="config">The value of the config.</param>
        public void Configure(StatsdConfig config)
        {
            if (_statsdBuilder == null)
            {
                throw new ObjectDisposedException(nameof(DogStatsdService));
            }

            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (_config != null)
            {
                throw new InvalidOperationException("Configuration for DogStatsdService already performed");
            }

            _config = config;
            //_prefix = config.Prefix;

            _statsdData = _statsdBuilder.BuildStatsData(config);
            _statsD = _statsdData.Statsd;
        }

        /// <summary>
        /// Records an event.
        /// </summary>
        /// <param name="title">The title of the event.</param>
        /// <param name="text">The text body of the event.</param>
        /// <param name="alertType">error, warning, success, or info (defaults to info).</param>
        /// <param name="aggregationKey">A key to use for aggregating events.</param>
        /// <param name="sourceType">The source type name.</param>
        /// <param name="dateHappened">The epoch timestamp for the event (defaults to the current time from the DogStatsD server).</param>
        /// <param name="priority">Specifies the priority of the event (normal or low).</param>
        /// <param name="hostname">The name of the host.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public void Event(string title, string text, string alertType = null, string aggregationKey = null, string sourceType = null, int? dateHappened = null, string priority = null, string hostname = null, string[] tags = null)
        {
            _statsD?.Send(title, text, alertType, aggregationKey, sourceType, dateHappened, priority, hostname, tags);
        }

        /// <summary>
        /// Adjusts the specified counter by a given delta.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">A given delta.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public void Counter<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            _statsD?.Send<Statsd2.Counting, T>(statName, value, sampleRate, tags);
        }

        /// <summary>
        /// Increments the specified counter.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The amount of increment.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public void Increment(string statName, int value = 1, double sampleRate = 1.0, string[] tags = null)
        {
            _statsD?.Send<Statsd2.Counting, int>(statName, value, sampleRate, tags);
        }

        /// <summary>
        /// Decrements the specified counter.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The amount of decrement.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public void Decrement(string statName, int value = 1, double sampleRate = 1.0, string[] tags = null)
        {
            _statsD?.Send<Statsd2.Counting, int>(statName, -value, sampleRate, tags);
        }

        /// <summary>
        /// Records the latest fixed value for the specified named gauge.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value of the gauge.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public void Gauge<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            _statsD?.Send<Statsd2.Gauge, T>(statName, value, sampleRate, tags);
        }

        /// <summary>
        /// Records a value for the specified named histogram.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value of the histogram.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public void Histogram<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            _statsD?.Send<Statsd2.Histogram, T>(statName, value, sampleRate, tags);
        }

        /// <summary>
        /// Records a value for the specified named distribution.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value of the distribution.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public void Distribution<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            _statsD?.Send<Statsd2.Distribution, T>(statName, value, sampleRate, tags);
        }

        /// <summary>
        /// Records a value for the specified set.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        public void Set<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            _statsD?.Send<Statsd2.Set, T>(statName, value, sampleRate, tags);
        }

        /// <summary>
        /// Records an execution time in milliseconds.
        /// </summary>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="value">The time in millisecond.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of value parameter.</typeparam>
        public void Timer<T>(string statName, T value, double sampleRate = 1.0, string[] tags = null)
        {
            _statsD?.Send<Statsd2.Timing, T>(statName, value, sampleRate, tags);
        }

        /// <summary>
        /// Creates a timer that records the execution time until Dispose is called on the returned value.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <returns>A disposable object that records the execution time until Dispose is called.</returns>
        public IDisposable StartTimer(string name, double sampleRate = 1.0, string[] tags = null)
        {
            return new MetricsTimer(this, name, sampleRate, tags);
        }

        /// <summary>
        /// Records an execution time for the given action.
        /// </summary>
        /// <param name="action">The given action.</param>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        public void Time(Action action, string statName, double sampleRate = 1.0, string[] tags = null)
        {
            if (_statsD == null)
            {
                action();
            }
            else
            {
                _statsD.Send(action, statName, sampleRate, tags);
            }
        }

        /// <summary>
        /// Records an execution time for the given function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <param name="statName">The name of the metric.</param>
        /// <param name="sampleRate">Percentage of metric to be sent.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <typeparam name="T">The type of the returned value of <paramref name="func"/>.</typeparam>
        /// <returns>The returned value of <paramref name="func"/>.</returns>
        public T Time<T>(Func<T> func, string statName, double sampleRate = 1.0, string[] tags = null)
        {
            if (_statsD == null)
            {
                return func();
            }

            using (StartTimer(statName, sampleRate, tags))
            {
                return func();
            }
        }

        /// <summary>
        /// Records a run status for the specified named service check.
        /// </summary>
        /// <param name="name">The name of the service check.</param>
        /// <param name="status">A constant describing the service status.</param>
        /// <param name="timestamp">The epoch timestamp for the service check (defaults to the current time from the DogStatsD server).</param>
        /// <param name="hostname">The hostname to associate with the service check.</param>
        /// <param name="tags">Array of tags to be added to the data.</param>
        /// <param name="message">Additional information or a description of why the status occurred.</param>
        public void ServiceCheck(string name, Status status, int? timestamp = null, string hostname = null, string[] tags = null, string message = null)
        {
            _statsD?.Send(name, (int)status, timestamp, hostname, tags, message);
        }

        /// <summary>
        /// Disposes an instance of DogStatsdService.
        /// Flushes all metrics.
        /// </summary>
        public void Dispose()
        {            
            _statsdData?.Dispose();
            _statsdData = null;
            Console.WriteLine("Nb Alloc: {0}", Statsd2.nbAlloc);
            Console.WriteLine("Nb Alloc42: {0}", Statsd2.Poll.Count);

        }

        // private string BuildNamespacedStatName(string statName)
        // {
        //     if (string.IsNullOrEmpty(_prefix))
        //     {
        //         return statName;
        //     }

        //     return _prefix + "." + statName;
        // }
    }
}
