using Microsoft.Azure.WebJobs;

namespace DurableFunc
{
    public static class Hello
    {
        [FunctionName("Hello")]
        public static string Run([ActivityTrigger] DurableActivityContext context)
        {
            var input = context.GetInput<string>();
            return $"Hello, {input}";
        }
    }
}
