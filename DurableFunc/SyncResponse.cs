using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using DurableFunc.Model;
using DurableFunc.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

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
                for (int i = 0; i < executionDetails.Iterations; i++)
                {
                    Thread.Sleep(executionDetails.IterationPeriod);
                    string statusCheck;
                    try
                    {
                        statusCheck = await httpCleint.GetStringAsync(statusUri);
                    }
                    catch (Exception e)
                    {
                        // log the exception
                        log.Error(e.Message);
                        continue;
                    }
                    
                    var status = JsonConvert.DeserializeObject<StatusResponse>(statusCheck);
                    if (status.RuntimeStatus == "Completed")
                    {
                        result = status.Output;
                        break;
                    }
                }
            }

            return result == null
                ? req.CreateResponse(HttpStatusCode.OK, $"The operation is taking more than expected. Keep following the progress here {statusUri}") : 
                req.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
