using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainbow.Console
{
    /// <summary>
    /// Possible browser results.
    /// </summary>
    public enum BrowserResultType
    {
        /// <summary>
        /// success
        /// </summary>
        Success,
        /// <summary>
        /// HTTP error
        /// </summary>
        HttpError,
        /// <summary>
        /// user cancel
        /// </summary>
        UserCancel,
        /// <summary>
        /// timeout
        /// </summary>
        Timeout,
        /// <summary>
        /// unknown error
        /// </summary>
        UnknownError
    }
}
