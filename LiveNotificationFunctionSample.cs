using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WebMotions.Azure.WebJobs.Extensions.Slack;
using WebMotions.Azure.WebJobs.Extensions.Slack.Models;
using WebMotions.Azure.WebJobs.Extensions.Slack.Models.Blocks;
using WebMotions.Azure.WebJobs.Extensions.Slack.Models.Objects;

namespace LiveNotificationFunction
{
    public static class LiveNotificationFunctionSample
    {
        [FunctionName("LiveNotificationFunction")]
        public static async Task Run(
            [EventHubTrigger("monitoring", Connection = "EventHubConnectionAppSetting")] EventData[] events,
            [Slack(Username = "KeyvaultGuardian", Channel = "#devtest")] IAsyncCollector<SlackMessage> slackMessages,
            ILogger log
        )
        {
            var tenantName = "equisoft";
            var exceptions = new List<Exception>();
            var eventRecords = new List<KeyvaultEvent>();
            var auditRecords = new List<KeyvaultAuditEvent>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");

                    var obj = JToken.Parse(messageBody);
                    if (obj.Type == JTokenType.Object)
                    {
                        if (((JObject)obj).TryGetValue("records", StringComparison.OrdinalIgnoreCase, out var auditEventRecords))
                        {
                            log.LogInformation("Found {EventCount} AuditEvent(s)", auditEventRecords.Children().Count());
                            foreach (var auditEventRecord in auditEventRecords)
                            {
                                var keyvaultAuditEvent = auditEventRecord.ToObject<KeyvaultAuditEvent>();
                                auditRecords.Add(keyvaultAuditEvent);
                            }
                        }
                    }
                    else if (obj.Type == JTokenType.Array)
                    {
                        log.LogInformation("Found {EventCount} Event(s)", obj.Children().Count());
                        foreach (var eventRecord in obj)
                        {
                            var keyvaultEvent = eventRecord.ToObject<KeyvaultEvent>();
                            eventRecords.Add(keyvaultEvent);
                        }
                    }
                    else
                    {
                        log.LogWarning("C# Event Hub trigger function processed a message: {Message} but the type {MessageType} is not handled", messageBody, obj.Type);
                        // Todo add some more logic here
                    }

                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();

            if (auditRecords.Count > 0)
            {
                foreach (var auditRecord in auditRecords)
                {
                    if (auditRecord.OperationName == "SecretGet" && auditRecord.ResultType == "Success")
                    {
                        var vaultName = auditRecord.Properties.Id.Host.Replace(".vault.azure.net", string.Empty);
                        var slackMessage = new SlackMessage();
                        slackMessage.Text = "A new keyvault audit event occured";

                        var sectionBlockHeader = new SlackSectionBlock();
                        sectionBlockHeader.Text = new SlackMarkdownText($"You have a new keyvault audit event activity:\n*<https://portal.azure.com/#@{tenantName}.onmicrosoft.com/resource{auditRecord.ResourceId}|{vaultName}>*");

                        var sectionBlockBody = new SlackSectionBlock();
                        // you should be careful if the identity is a Service Principal. Using a user as demonstration
                        var upnClaim = auditRecord.Identity.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
                        var objectId = auditRecord.Identity.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier");
                        sectionBlockBody.Fields.AddRange(new[]
                        {
                            new SlackMarkdownText($"*Who:*\n{upnClaim?.Value ?? "N/A"}[{objectId?.Value ?? "N/A"}]"),
                            new SlackMarkdownText($"*Time:*\n{auditRecord.Time}"),
                            new SlackMarkdownText($"*Type:*\n{auditRecord.OperationName}"),
                            new SlackMarkdownText($"*Name:*\n{auditRecord.Properties.Id.Segments[^2].Replace("/",string.Empty)}"),
                            new SlackMarkdownText($"*IP:*\n{auditRecord.CallerIpAddress}")
                        });

                        slackMessage.Blocks.AddRange(new [] {sectionBlockHeader,sectionBlockBody});

                        await slackMessages.AddAsync(slackMessage);
                    }
                }
            }

            if (eventRecords.Count > 0)
            {
                foreach (var eventRecord in eventRecords)
                {
                    var slackMessage = new SlackMessage();
                    slackMessage.Text = "A new keyvault event occured";
                    
                    var sectionBlockHeader = new SlackSectionBlock();
                    sectionBlockHeader.Text = new SlackMarkdownText($"You have a new keyvault event activity:\n*<https://{eventRecord.Data.VaultName}.vault.azure.net|{eventRecord.Data.VaultName}>*");
                    
                    var sectionBlockBody = new SlackSectionBlock();
                    sectionBlockBody.Fields.AddRange(new []
                    {
                        new SlackMarkdownText($"*Type:*\n{eventRecord.EventType}"), 
                        new SlackMarkdownText($"*Time:*\n{eventRecord.EventTime}"), 
                        new SlackMarkdownText($"*Type:*\n{eventRecord.Data.ObjectType}"),
                        new SlackMarkdownText($"*Name:*\n<{eventRecord.Data.Id}|{eventRecord.Data.ObjectName}>"),
                        new SlackMarkdownText($"*Version:*\n{eventRecord.Data.Version}")
                    });

                    if (eventRecord.Data.Nbf != null)
                    {
                        sectionBlockBody.Fields.Add(new SlackMarkdownText($"*Not Before:*\n{eventRecord.Data.Nbf}"));
                    }

                    if (eventRecord.Data.Exp != null)
                    {
                        sectionBlockBody.Fields.Add(new SlackMarkdownText($"*Expires:*\n{eventRecord.Data.Exp}"));
                    }

                    slackMessage.Blocks.AddRange(new[] { sectionBlockHeader, sectionBlockBody });

                    await slackMessages.AddAsync(slackMessage);
                }
            }
        }
    }
}
