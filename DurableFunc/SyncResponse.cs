using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using DurableFunc.Model;
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
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string funcname = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "funcname", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            funcname = funcname ?? data?.funcname;
            List<string> result = null;
            string statusUri;
            var orchestrationClientUri = ConfigurationManager.AppSettings["OrchestrationClientUri"];
            using (var httpCleint = new HttpClient())
            {
                var orcestrationClientResponse =
                    await httpCleint.PostAsync(new Uri($"{orchestrationClientUri}{funcname}"), null);
                orcestrationClientResponse.EnsureSuccessStatusCode();
                var clientResponse =
                    await orcestrationClientResponse.Content.ReadAsAsync<OrchestrationClientResponse>();
                statusUri = clientResponse.StatusQueryGetUri;
                for (int i = 0; i < 15; i++)
                {
                    Thread.Sleep(300);
                    string statusCheck;
                    try
                    {
                        statusCheck = await httpCleint.GetStringAsync(statusUri);
                    }
                    catch (Exception e)
                    {
                        // log the exception
                        Console.WriteLine(e);
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

            return funcname == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a funcname on the query string or in the request body")
                : result == null
                ? req.CreateResponse(HttpStatusCode.OK, $"The operation is taking more than expected. Keep following the progress here {statusUri}") : 
                req.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
