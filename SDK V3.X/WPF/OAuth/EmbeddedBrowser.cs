using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media;

namespace WpfWebView
{

    public class BrowserResult
    {
        public Dictionary<String, String> Parameters { get; set; }
        public String Code { get; set; }
        public Boolean CancelByUser { get; set; }
        public Boolean Error { get; set; }

    }

    public class EmbeddedBrowser
    {
        public Window Window { get; private set; }

        public String Title
        {
            get { return Window.Title; }
            set { Window.Title = value; }
        }

        public ImageSource Icon
        {
            get { return Window.Icon; }
            set { Window.Icon = value; }
        }

        private WebView2 webView2;

        private SemaphoreSlim signal;
        private BrowserResult result;
        private String Uri;
        private String RedirectUri;
        private Boolean SSOInprogress = false;

        private Boolean needToClose = false;

        public EmbeddedBrowser()
        {
            WinInetHelper.SupressCookiePersist();

            result = new BrowserResult();
            SetDefaultResult();

            Window = new Window()
            {
                Width = 860,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            Window.Closing += Window_Closing;

            // Create WebBrowser
            webView2 = new WebView2();
            webView2.NavigationStarting += WebView2_NavigationStarting;
            
            //webBrowser = new WebBrowser();
            //webBrowser.Navigating += WebBrowser_Navigating;

            // Add browser as content of the window
            Window.Content = webView2;
        }

        public void Show()
        {
            if( (Window != null) && SSOInprogress)
            {
                if (Window.WindowState == WindowState.Minimized)
                    Window.WindowState = WindowState.Normal;

                Window.Show();
            }
        }

        public Boolean Activate()
        {
            if(Window != null)
                return Window.Activate();
            return false;
        }

        public void Close()
        {
            WinInetHelper.EndBrowserSession();

            if (signal != null)
            {
                signal.Dispose();
                signal = null;
            }

            needToClose = true;
            Window.Close();
        }

        public async Task<BrowserResult> InvokeAsync(String uri, String redirectUri, CancellationToken cancellationToken = default)
        {
            Uri = uri;
            RedirectUri = new Uri(redirectUri).ToString();
            SSOInprogress = true;


            // Note: Unfortunately, WebBrowser is very limited and does not give sufficient information for 
            //   robust error handling. The alternative is to use a system browser or third party embedded
            //   library (which tend to balloon the size of your application and are complicated).

            if (signal != null)
                signal.Dispose();
            signal = new SemaphoreSlim(0, 1);

            SetDefaultResult();

            Window.Show();

            // Navigate to the URL
            webView2.Source = new Uri(Uri);

            await signal.WaitAsync();

            return result;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SSOInprogress = false;
            if (!needToClose)
            {
                webView2.NavigateToString("<html></html>");

                try
                {
                    signal.Release();
                }
                catch { }

                e.Cancel = true;
                Window.Hide();
            }
        }

        private void WebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (BrowserIsNavigatingToRedirectUri(e.Uri))
            {
                // Prevent real navigation
                e.Cancel = true;

                // Parse query string to get token value and query parameters
                string queryString = new System.Uri(e.Uri).Query;
                var collection = HttpUtility.ParseQueryString(queryString);

                result.Parameters = collection.Cast<string>().ToDictionary(k => k, v => collection[v]);
                if(result.Parameters.ContainsKey("code"))
                    result.Code = result.Parameters["code"];
                result.Error = String.IsNullOrEmpty(result.Code);
                result.CancelByUser = false;

                

                try
                {
                    signal.Release();
                }
                catch { }

                Window.Close();
            }
        }

        private void SetDefaultResult()
        {
            // Set default values
            result.CancelByUser = true;
            result.Error = true;
            result.Code = "";
        }

        private bool BrowserIsNavigatingToRedirectUri(String uri)
        {
            return uri.StartsWith(RedirectUri, StringComparison.InvariantCultureIgnoreCase);
        }

    }

    public static class WinInetHelper
    {
        public static bool SupressCookiePersist()
        {
            // 3 = INTERNET_SUPPRESS_COOKIE_PERSIST
            // 81 = INTERNET_OPTION_SUPPRESS_BEHAVIOR
            return SetOption(81, 3);
        }

        public static bool EndBrowserSession()
        {
            // 42 = INTERNET_OPTION_END_BROWSER_SESSION
            return SetOption(42, null);
        }

        static bool SetOption(int settingCode, int? option)
        {
            IntPtr optionPtr = IntPtr.Zero;
            int size = 0;
            if (option.HasValue)
            {
                size = sizeof(int);
                optionPtr = Marshal.AllocCoTaskMem(size);
                Marshal.WriteInt32(optionPtr, option.Value);
            }

            bool success = InternetSetOption(0, settingCode, optionPtr, size);


            if (option.HasValue)
                Marshal.FreeCoTaskMem(optionPtr);

            return success;
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetOption(int hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
    }

}
