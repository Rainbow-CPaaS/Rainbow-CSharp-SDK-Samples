namespace Rainbow.Console
{
    /// <summary>
    /// Options for the browser used for login.
    /// </summary>
    public class BrowserOptions
    {
        /// <summary>
        /// Gets the start URL.
        /// </summary>
        /// <value>
        /// The start URL.
        /// </value>
        public string StartUrl { get; }

        /// <summary>
        /// Gets the end URL.
        /// </summary>
        /// <value>
        /// The end URL.
        /// </value>
        public string EndUrl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserOptions"/> class.
        /// </summary>
        /// <param name="startUrl">The start URL.</param>
        /// <param name="endUrl">The end URL.</param>
        public BrowserOptions(string startUrl, string endUrl)
        {
            StartUrl = startUrl;
            EndUrl = endUrl;
        }
    }
}
