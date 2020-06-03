using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace LiveNotificationFunction
{
    public class KeyvaultAuditEvent
    {
        /// <summary>
        /// Date and time in UTC.
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// Type of result. For Key Vault logs, AuditEvent is the single, available value.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Name of the operation, as documented in the next table.
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Result of the REST API request.
        /// </summary>
        public string ResultType { get; set; }

        /// <summary>
        /// Additional description about the result, when available.
        /// </summary>
        public string ResultDescription { get; set; }

        /// <summary>
        /// An optional GUID that the client can pass to correlate client-side logs with service-side (Key Vault) logs.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// IP address of the client that made the request.
        /// </summary>
        public string CallerIpAddress { get; set; }

        /// <summary>
        /// Identity from the token that was presented in the REST API request.
        /// This is usually a "user," a "service principal," or the combination "user+appId," as in the case of a request
        /// that results from an Azure PowerShell cmdlet.
        /// </summary>
        public Identity Identity { get; set; }

        /// <summary>
        /// Information that varies based on the operation (operationName).
        /// In most cases, this field contains client information (the user agent string passed by the client),
        /// the exact REST API request URI, and the HTTP status code. In addition, when an object is returned as a
        /// result of a request (for example, KeyCreate or VaultGet), it also contains the key URI (as "id"), vault URI, or secret URI.
        /// </summary>
        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        /// <summary>
        /// Azure Resource Manager resource ID. For Key Vault logs, this is always the Key Vault resource ID.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// REST API version requested by the client.
        /// </summary>
        public string OperationVersion { get; set; }

        /// <summary>
        /// HTTP status.
        /// </summary>
        public string ResultSignature { get; set; }

        /// <summary>
        /// Time it took to service the REST API request, in milliseconds.
        /// This does not include the network latency, so the time you measure on the client side might not match this time.
        /// </summary>
        public long DurationMs { get; set; }
    }

    public class Identity
    {
        [JsonConverter(typeof(KeyvaultAuditEventIdentityClaimConverter))]
        [JsonProperty("claim")]
        public List<Claim> Claims { get; set; }
    }

    public class Properties
    {
        /// <summary>
        /// When an object is returned as a result of a request (for example, KeyCreate or VaultGet), it also contains the key URI (as "id"), vault URI, or secret URI.
        /// </summary>
        public Uri Id { get; set; }

        /// <summary>
        /// The user agent string passed by the client
        /// </summary>
        public string ClientInfo { get; set; }

        /// <summary>
        /// he exact REST API request response Status Code
        /// </summary>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// The exact REST API request URI
        /// </summary>
        public Uri RequestUri { get; set; }

        /// <summary>
        /// If the type (SecretGet, SecretList, etc) is allowed for the user making the request in the access policies
        /// </summary>
        public bool IsAccessPolicyMatch { get; set; }

        /// <summary>
        /// The keyvault resource properties
        /// </summary>
        [JsonProperty("properties")]
        public JObject ResourceProperties { get; set; }
    }
}