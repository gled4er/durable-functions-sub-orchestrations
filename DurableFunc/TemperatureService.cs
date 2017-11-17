using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace DurableFunc
{
    public static class TemperatureService
    {
        [FunctionName("TemperatureService")]
        public static async Task<double> Run([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var input = context.GetInput<string>();
            var valueArray = JsonConvert.DeserializeObject<string[]>(input);
            var temperature = await context.CallActivityAsync<double>("Temperature", valueArray.FirstOrDefault());
            return temperature;
        }
    }
}
