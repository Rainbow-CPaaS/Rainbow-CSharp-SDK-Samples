// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Rainbow.Console
{
    public class SystemBrowser
    {
        public int Port { get; }
        private readonly string _path;

        public SystemBrowser(List<int> ports, string path = null)
        {
            _path = path;

            if ( (ports is null) || (ports.Count == 0))
                throw new ArgumentException("ports is null/empty");

            Port = PortAvailable(ports);

            if(Port == 0)
                throw new ArgumentException("cannot create HTTP server using one of port specified");
        }


        private int PortAvailable(List<int> ports)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] listeners = properties.GetActiveTcpListeners();
            var portsUsed = listeners.Select(item => item.Port);
            return ports.FirstOrDefault(p => !portsUsed.Contains(p));
        }

        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public async Task<BrowserResult> InvokeAsync(String url, CancellationToken cancellationToken)
        {
            using (var listener = new LoopbackHttpListener(Port, _path))
            {
                OpenBrowser(url);

                try
                {
                    var browserResult = await listener.WaitForCallbackAsync();
                    return browserResult;
                }
                catch (TaskCanceledException ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
                }
                catch (Exception ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
                }
                finally
                {
                    listener.Dispose();
                }
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    public class LoopbackHttpListener : IDisposable
    {
        const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)

        IWebHost _host;
        TaskCompletionSource<BrowserResult> _source = new TaskCompletionSource<BrowserResult>();
        string _url;

        public string Url => _url;

        public LoopbackHttpListener(int port, string? path = null)
        {
            path = path ?? String.Empty;
            if (path.StartsWith("/")) path = path.Substring(1);

            _url = $"http://127.0.0.1:{port}/{path}";

            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(_url)
                .Configure(Configure)
                .Build();
            _host.Start();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500);
                _host.Dispose();
            });
        }

        public static string GetRequestBody(System.IO.Stream body)
        {
            var bodyStream = new StreamReader(body);
            //bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();
            return bodyText;
        }

        void Configure(IApplicationBuilder app)
        {
            app.Run(async ctx =>
            {
                if (ctx.Request.Method == "GET")
                {
                    BrowserResult browserResult = new();

                    // Store Query Parameters
                    browserResult.Parameters = new();
                    foreach (var key in ctx.Request.Query.Keys)
                    {
                        if(String.IsNullOrEmpty(key)) continue;
                        browserResult.Parameters[key] = ctx.Request.Query[key];
                    }
                    // Store Body
                    browserResult.Body = GetRequestBody(ctx.Request.Body);

                    // Do we received "code" as parameters ?
                    if(browserResult.Parameters.ContainsKey("code"))
                    {
                        browserResult.ResultType = BrowserResultType.Success;
                        await SetResultAsync(browserResult, ctx);
                    }
                    else
                    {
                        browserResult.ResultType = BrowserResultType.HttpError;
                        await SetResultAsync(browserResult, ctx);
                    }
                }
                else
                {
                    ctx.Response.StatusCode = 405;
                }
            });
        }

        private async Task SetResultAsync(BrowserResult browserResult, HttpContext ctx)
        {
            try
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                await ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>");
                await ctx.Response.Body.FlushAsync();
            }
            catch
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                await ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
                await ctx.Response.Body.FlushAsync();
            }

            _source.TrySetResult(browserResult);
        }

        public Task<BrowserResult> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeoutInSeconds * 1000);
                _source.TrySetCanceled();
            });

            return _source.Task;
        }
    }
}