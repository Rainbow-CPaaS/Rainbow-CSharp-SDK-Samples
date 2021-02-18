using System.Threading;

namespace SDK.WpfApp
{
    /// <summary>
    /// To prevent multi instance running
    /// </summary>
    public static class SingleApplicationDetector
    {
        private static readonly string GUID = "SDK_WpfApp_767AB30E-74BC-4638-90EB-1428DFA099F7";

        private static Semaphore __semaphore;

        /// <summary>
        /// To know if an insance is already running based on the GUID provided
        /// </summary>
        /// <returns>True is an instance is already running</returns>
        public static bool IsRunning()
        {
            var semaphoreName = @"Global\" + GUID;
            try
            {
                __semaphore = Semaphore.OpenExisting(semaphoreName);

                Close();
                return true;
            }
            catch
            {
                __semaphore = new Semaphore(0, 1, semaphoreName);
                return false;
            }
        }

        /// <summary>
        /// To close the sigle applciation detector
        /// 
        /// Must be called when the application is exiting
        /// </summary>
        public static void Close()
        {
            if (__semaphore != null)
            {
                __semaphore.Close();
                __semaphore = null;
            }
        }


    }
}
