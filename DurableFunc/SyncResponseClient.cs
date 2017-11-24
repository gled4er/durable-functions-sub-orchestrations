using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using  DurableFunc.Model;
using DurableFunc.Services;
using DurableFunc.Utils;

namespace DurableFunc
{
    public static class SyncResponseClient
    {
        /// <summary>
        /// Modified Orchestration Client Function that is providing sync response for the durable function output
        /// </summary>
        /// <param name="req">Request Message</param>
        /// <param name="starter">Durable Orchestration Client</param>
        /// <param name="functionName"> Durable Function Name</param>
        /// <param name="log">Logger</param>
        /// <returns></returns>
        [FunctionName("SyncResponseClient")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "orchestrator/{functionName}")] HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient starter,
            string functionName,
            TraceWriter log)
        {
            // Function input comes from the request content.
            dynamic eventData = await req.Content.ReadAsAsync<object>();
            string instanceId = await starter.StartNewAsync(functionName, eventData);

            log.Info($"Started orchestration with ID = '{instanceId}'.");

            var responseMessage =  starter.CreateCheckStatusResponse(req, instanceId);
            var clientResponse = JsonConvert.DeserializeObject<OrchestrationClientResponse>(await responseMessage.Content.ReadAsStringAsync());
            var executionDetails = Helper.GetExecutionDetails();
            var durableFunctionSyncResponseService = new DurableFunctionSyncResponseService();
            var result = await durableFunctionSyncResponseService.ProvideOutput(clientResponse, executionDetails, log);

            return result == null
                ? req.CreateResponse(HttpStatusCode.OK, $"The operation is taking more than expected. Keep following the progress here {clientResponse.StatusQueryGetUri}") :
                req.CreateResponse(HttpStatusCode.OK, result);
        }

    }
}
