using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;


namespace DurableFunc
{
    public static class HelloWorld
    {
        [FunctionName("HelloWorld")]
        public static async Task<List<string>> Run([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var list = new List<string>
            {
                $"{await context.CallActivityAsync<string>("Hello", "Tokyo")}. The temperature is {await context.CallSubOrchestratorAsync<string>("TemperatureService", "Tokyo")}°C",
                $"{await context.CallActivityAsync<string>("Hello", "Seattle")}. The temperature is {await context.CallSubOrchestratorAsync<string>("TemperatureService", "Seattle")}°C",
                $"{await context.CallActivityAsync<string>("Hello", "London")}. The temperature is {await context.CallSubOrchestratorAsync<string>("TemperatureService", "London")}°C"
            };

            return list;
        }
    }
}
