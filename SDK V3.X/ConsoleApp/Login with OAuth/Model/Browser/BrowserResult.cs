namespace Rainbow.Console
{
    public class BrowserResult : Result
    {
        /// <summary>
        /// Gets or sets the type of the result.
        /// </summary>
        /// <value>
        /// The type of the result.
        /// </value>
        public BrowserResultType ResultType { get; set; }

        /// <summary>
        /// Gets or sets Body.
        /// </summary>
        /// <value>
        /// Body.
        /// </value>
        public String Body { get; set; }

        /// <summary>
        /// Gets or sets Parameters.
        /// </summary>
        /// <value>
        /// Parameters.
        /// </value>
        public Dictionary<string, string> Parameters { get; set; }
    }
}
