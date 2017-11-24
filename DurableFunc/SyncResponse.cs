using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using DurableFunc.Model;
using DurableFunc.Services;
using DurableFunc.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;


namespace DurableFunc
{
    public static class SyncResponse
    {
        [FunctionName("SyncResponse")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var functionName = await Helper.GetParameterValue(req);
            if (string.IsNullOrEmpty(functionName))
            {
                req.CreateResponse(HttpStatusCode.BadRequest,"Please pass a funcname on the query string or in the request body");
            }
            object result = null;
            string statusUri;
            var orchestrationClientUri = ConfigurationManager.AppSettings["OrchestrationClientUri"];
            using (var httpCleint = new HttpClient())
            {
                var orcestrationClientResponse =
                    await httpCleint.PostAsync(new Uri($"{orchestrationClientUri}{functionName}"), null);
                orcestrationClientResponse.EnsureSuccessStatusCode();
                var clientResponse =
                    await orcestrationClientResponse.Content.ReadAsAsync<OrchestrationClientResponse>();
                statusUri = clientResponse.StatusQueryGetUri;
                var executionDetails = Helper.GetExecutionDetails();
                var durableFunctionSyncResponseService = new DurableFunctionSyncResponseService();
                result = await durableFunctionSyncResponseService.ProvideOutput(clientResponse, executionDetails, log);
            }

            return result == null
                ? req.CreateResponse(HttpStatusCode.OK, $"The operation is taking more than expected. Keep following the progress here {statusUri}") : 
                req.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
