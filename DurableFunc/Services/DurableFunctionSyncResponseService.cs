using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DurableFunc.Model;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace DurableFunc.Services
{
    /// <summary>
    /// Service providing logic for sync response from Durable Function
    /// </summary>
    public class DurableFunctionSyncResponseService
    {
        /// <summary>
        /// Functions providing logic for sync response from Durable Function
        /// </summary>
        /// <param name="clientResponse">Instance of <see cref="OrchestrationClientResponse"/></param>
        /// <param name="executionDetails">Instance of <see cref="ExecutionDetails"/></param>
        /// <param name="log">Instance of <see cref="TraceWriter"/></param>
        /// <returns>object instance representing the output of DurableFunction</returns>
        public async Task<object> ProvideOutput(OrchestrationClientResponse clientResponse, ExecutionDetails executionDetails, TraceWriter log)
        {
            object result = null;

            using (var httpClient = new HttpClient())
            {
                for (var i = 0; i < executionDetails.Iterations; i++)
                {
                    Thread.Sleep(executionDetails.IterationPeriod);
                    string statusCheck;
                    try
                    {
                        statusCheck = await httpClient.GetStringAsync(clientResponse.StatusQueryGetUri);
                    }
                    catch (Exception e)
                    {
                        // log the exception
                        log.Error(e.Message);
                        continue;
                    }

                    var status = JsonConvert.DeserializeObject<StatusResponse>(statusCheck);
                    if (status.RuntimeStatus != "Completed") { continue; }
                    result = status.Output;
                    break;
                }
            }

            return result;
        }
    }
}
