using System;
using System.Threading;
using System.Threading.Tasks;

namespace InstantMessaging.Helpers
{
    public class CancelableDelay
    {
        CancellationTokenSource tokenSource;
        CancellationToken token;
        Action action;
        Task task;
        int ms;

        public static CancelableDelay StartAfter(int milliseconds, Action action)
        {
            CancelableDelay result = new CancelableDelay();

            result.StartTask(milliseconds, action);

            return result;
        }

        private void StartTask(int milliseconds, Action action)
        {
            ms = milliseconds;
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            this.action = action;

            task = Task.Factory.StartNew(() =>
            {
                token.WaitHandle.WaitOne(ms);
                token.ThrowIfCancellationRequested();
                this.action.Invoke();
            });
        }

        public void PostPone(int milliseconds = 0)
        {
            if (milliseconds == 0)
                milliseconds = ms;
            Cancel();
            StartTask(milliseconds, action);
        }

        public Boolean IsRunning()
        {
            return task.Status == TaskStatus.Running;
        }

        public void Cancel()
        {
            try
            {
                tokenSource.Cancel();
            }
            catch
            {
                // Nothing to do more
            }
        }

        private CancelableDelay() { }

    }
}
