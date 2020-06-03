using Newtonsoft.Json;
using System;

namespace LiveNotificationFunction
{
    public class KeyvaultEvent
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The resourceId where the event came from
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// The name of the keyvault object
        /// </summary>
        public string Subject { get; set; }

        public string EventType { get; set; }

        [JsonConverter(typeof(KeyvaultEventDataConverter))]
        public KeyvaultEventData Data { get; set; }

        public DateTimeOffset EventTime { get; set; }
    }

    public class KeyvaultEventData
    {
        /// <summary>
        /// The ID of the object that triggered this event
        /// </summary>
        [JsonProperty("Id")]
        public Uri Id { get; set; }

        /// <summary>
        /// The key vault name of the object that triggered this event
        /// </summary>
        [JsonProperty("VaultName")]
        public string VaultName { get; set; }

        /// <summary>
        /// The type of the object that triggered this event
        /// </summary>
        [JsonProperty("ObjectType")]
        public string ObjectType { get; set; }

        /// <summary>
        /// The name of the object that triggered this event
        /// </summary>
        [JsonProperty("ObjectName")]
        public string ObjectName { get; set; }

        /// <summary>
        /// The version of the object that triggered this event
        /// </summary>
        [JsonProperty("Version")]
        public string Version { get; set; }

        /// <summary>
        /// TThe not-before date in seconds since 1970-01-01T00:00:00Z of the object that triggered this event
        /// </summary>
        [JsonProperty("NBF")]
        public DateTimeOffset? Nbf { get; set; }

        /// <summary>
        /// The expiration date in seconds since 1970-01-01T00:00:00Z of the object that triggered this event
        /// </summary>
        [JsonProperty("EXP")]
        public DateTimeOffset? Exp { get; set; }
    }
}