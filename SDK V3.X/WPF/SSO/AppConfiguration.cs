using System;
using System.Collections.Generic;
using System.Text;

namespace WpfSSOSamples
{
    static public class AppConfiguration
    {
        internal static string APP_ID = "TO DEFINE";
        internal static string APP_SECRET_KEY = "TO DEFINE";
        internal static string HOST_NAME = "openrainbow.com";

        internal static string URI_SCHEME_FOR_SSO = "myappscheme"; // Specify the correct URI Scheme
        internal static string URI_PATH_FOR_SSO = "callback"; // Specify the correct URI Path
        // Authentication URL will then be: myappscheme://callback/
    }
}
