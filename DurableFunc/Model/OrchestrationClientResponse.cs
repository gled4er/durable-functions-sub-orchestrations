namespace DurableFunc.Model
{
    /// <summary>
    /// Model class for Orchestration Client Response 
    /// </summary>
    public class OrchestrationClientResponse
    {
        /// <summary>
        /// ID of execution
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Status URI
        /// </summary>
        public string StatusQueryGetUri { get; set; }

        /// <summary>
        /// Send Event URI
        /// </summary>
        public string SendEventPostUri { get; set; }

        /// <summary>
        /// Terminate URI
        /// </summary>
        public string TerminatePostUri { get; set; }
    }
}
