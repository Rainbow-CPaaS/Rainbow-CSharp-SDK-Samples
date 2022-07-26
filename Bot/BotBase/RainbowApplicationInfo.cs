using System;
using System.Collections.Generic;
using System.Text;

namespace RainbowBotBase
{
    internal static class RainbowApplicationInfo
    {
        static internal readonly string DEFAULT_VALUE = "TO DEFINE";  // /!\ DON'T MODIFY THIS

        // NLog configuration file used to log SDK debug information
        static internal readonly String NLOG_CONFIG_FILE_PATH = @"..\..\..\..\..\NLogConfiguration.xml";

        // DEFINE YOUR APP_ID,  APP_SECRET_KEY,  HOST_NAME etc ...
        //static internal readonly string APP_ID = DEFAULT_VALUE;
        //static internal readonly string APP_SECRET_KEY = DEFAULT_VALUE;
        //static internal readonly string HOST_NAME = DEFAULT_VALUE;

        //static internal readonly string LOGIN_MASTER_BOT = DEFAULT_VALUE;

        //static internal readonly string LOGIN_BOT = DEFAULT_VALUE;
        //static internal readonly string PASSWORD_BOT = DEFAULT_VALUE;


        ///////////////////////////////////////////
        //
        // NET ENVIRONMENT - START                                                                                                

        static internal readonly string APP_ID = "6f8c5910725b11e9b55c81be00bebc2c";
        static internal readonly string APP_SECRET_KEY = "qSmr71s7idLiKRmhXGNNIOpPJynliqrS2sHKy3Wzk6ytauRSP13qebuJQmRHGwTN";
        static internal readonly string HOST_NAME = "openrainbow.net";

        static internal readonly string LOGIN_MASTER_BOT = "irlesuser2@sophia.com";

        static internal readonly string LOGIN_BOT = "irlesuser3@sophia.com";
        static internal readonly string PASSWORD_BOT = "Rainbow1!";


    }
}
