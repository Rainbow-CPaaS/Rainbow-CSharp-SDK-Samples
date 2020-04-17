using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstantMessaging.Helpers
{
    public class CancelableDelay
    {
        Thread delayTh;
        Action action;
        int ms;

        public static CancelableDelay StartAfter(int milliseconds, Action action)
        {
            CancelableDelay result = new CancelableDelay() { ms = milliseconds };
            result.action = action;
            result.delayTh = new Thread(result.Delay);
            result.delayTh.Start();
            return result;
        }

        public Boolean IsRunning()
        {
            return delayTh.IsAlive;
        }

        public void Cancel() => delayTh.Abort();

        private CancelableDelay() { }

        private void Delay()
        {
            try
            {
                Thread.Sleep(ms);

                Task task = new Task(() =>
                {
                    action.Invoke();
                });
                task.Start();
            }
            catch (ThreadAbortException)
            { }
        }


    }
}
