using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DurableFunc.Model;
using Microsoft.Azure.WebJobs;

namespace DurableFunc.Utils
{
    public static class Helper
    {
        public static ExecutionDetails GetExecutionDetails()
        {

            int.TryParse(ConfigurationManager.AppSettings["MaxExecutionTime"], out var maxExecutionTimeOut);
            int.TryParse(ConfigurationManager.AppSettings["ExecutionPeriod"], out var executionPeriod);

            return  new ExecutionDetails
            {
                IterationPeriod = executionPeriod,
                Iterations = maxExecutionTimeOut / executionPeriod
            };
        }

        public static async Task<string> GetParameterValue(HttpRequestMessage request)
        {
            var functionName = request.GetQueryNameValuePairs()
                .FirstOrDefault(q => String.Compare(q.Key, "funcname", StringComparison.OrdinalIgnoreCase) == 0)
                .Value;

            // Get request body
            dynamic data = await request.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            functionName = functionName ?? data?.funcname;
            return functionName;
        }
    }
}
