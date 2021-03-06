﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace StatsdClient
{
    /// <summary>
    /// The configuration options for DogStatsdService.
    /// </summary>
    public class StatsdConfig
    {
        /// <summary>
        /// The default port for UDP.
        /// </summary>
        public const int DefaultStatsdPort = 8125;

        /// <summary>
        /// The default UDP maximum packet size.
        /// </summary>
        public const int DefaultStatsdMaxUDPPacketSize = 512;

        /// <summary>
        /// The name of the environment variable defining the global tags to be applied to every metric, event, and service check.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Avoid breaking changes.")]
        [ObsoleteAttribute("This field will be removed in a future release. You should use instead EntityIdEnvVar.")]
        public const string DD_ENTITY_ID_ENV_VAR = "DD_ENTITY_ID";

        /// <summary>
        /// The name of the environment variable defining the global tags to be applied to every metric, event, and service check.
        /// </summary>
        public const string EntityIdEnvVar = "DD_ENTITY_ID";

        /// <summary>
        /// The name of the environment variable defining the port of the targeted StatsD server.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Avoid breaking changes.")]
        [ObsoleteAttribute("This field will be removed in a future release. You should use instead DogStatsdPortEnvVar.")]
        public const string DD_DOGSTATSD_PORT_ENV_VAR = "DD_DOGSTATSD_PORT";

        /// <summary>
        /// The name of the environment variable defining the port of the targeted StatsD server.
        /// </summary>
        public const string DogStatsdPortEnvVar = "DD_DOGSTATSD_PORT";

        /// <summary>
        /// The name of the environment variable defining the host name of the targeted StatsD server.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Avoid breaking changes.")]
        [ObsoleteAttribute("This field will be removed in a future release. You should use instead AgentHostEnvVar.")]
        public const string DD_AGENT_HOST_ENV_VAR = "DD_AGENT_HOST";

        /// <summary>
        /// The name of the environment variable defining the host name of the targeted StatsD server.
        /// </summary>
        public const string AgentHostEnvVar = "DD_AGENT_HOST";

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsdConfig"/> class.
        /// </summary>
        public StatsdConfig()
        {
            StatsdPort = 0;
            StatsdMaxUDPPacketSize = DefaultStatsdMaxUDPPacketSize;
            Advanced = new AdvancedStatsConfig();
        }

        /// <summary>
        /// Gets or sets the host name of the targeted StatsD server.
        /// </summary>
        /// <value>The host name of the targeted StatsD server.</value>
        public string StatsdServerName { get; set; }

        /// <summary>
        /// Gets or sets the port of the targeted StatsD server.
        /// </summary>
        /// <value>The port of the targeted StatsD server.</value>
        public int StatsdPort { get; set; }

        /// <summary>
        /// Gets or sets the maximum UDP packet size.
        /// </summary>
        /// <value>The maximum UDP packet size.</value>
        public int StatsdMaxUDPPacketSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum Unix domain socket packet size.
        /// </summary>
        /// <value>The maximum Unix domain socket packet size.</value>
        public int StatsdMaxUnixDomainSocketPacketSize { get; set; } = 2048;

        /// <summary>
        /// Gets or sets a value indicating whether we truncate the metric if it is too long.
        /// </summary>
        /// <value>The value indicating whether we truncate the metric if it is too long.</value>
        public bool StatsdTruncateIfTooLong { get; set; } = true;

        /// <summary>
        /// Gets or sets the prefix to apply to every metric, event, and service check.
        /// </summary>
        /// <value>The prefix to apply to every metric, event, and service check.</value>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets the advanced configuration.
        /// </summary>
        /// <value>The advanced configuration</value>
        public AdvancedStatsConfig Advanced { get; }

        /// <summary>
        /// Gets or sets the global tags to be applied to every metric, event, and service check.
        /// </summary>
        /// <value>The global tags to be applied to every metric, event, and service check.</value>
        public string[] ConstantTags { get; set; }
    }
}
