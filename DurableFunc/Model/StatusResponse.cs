using System.Collections.Generic;


namespace DurableFunc.Model
{
    /// <summary>
    /// Status Response 
    /// </summary>
    public  class StatusResponse
    {
        /// <summary>
        /// Status for the execution
        /// </summary>
        public string RuntimeStatus { get; set; }

        /// <summary>
        /// Input data 
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// Output data
        /// </summary>
        public List<string> Output { get; set; }

        /// <summary>
        /// Creating time
        /// </summary>
        public string CreatedTime { get; set; }

        /// <summary>
        /// Last updated time
        /// </summary>
        public string LastUpdatedTime { get; set; }


    }
}
