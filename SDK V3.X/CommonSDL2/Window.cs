using FFmpeg.AutoGen;

namespace Rainbow.Example.Common.SDL2
{
    public class Window
    {
#region public properties

        public IntPtr Handle = IntPtr.Zero;
        public uint Id = 0;
        public IntPtr Renderer = IntPtr.Zero;
        public IntPtr Texture = IntPtr.Zero;

        public Boolean FullScreen = false; // True if the window is full screen

        public Boolean NeedRendereUpdate = false; // True if it's necessary to update the renderer

        public Boolean VideoStopped = true;

#endregion public properties

#region public static methods

        public static void ToggleFullScreen(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
            {
                window.FullScreen = !window.FullScreen;
                Rainbow.Medias.SDL2.SDL_SetWindowFullscreen(window.Handle, (uint)(window.FullScreen ? Rainbow.Medias.SDL2.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP : 0));
                Restore(window); // If the windows was minimized, it's no more the case
                Raise(window);  // To have the window on top - like SetForegroundWindow()
            }
        }

        public static void Create(Window window)
        {
            if (window is null || window.Handle != IntPtr.Zero)
                return;

            var flags = Rainbow.Medias.SDL2.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | Rainbow.Medias.SDL2.SDL_WindowFlags.SDL_WINDOW_SHOWN;
            window.Handle = Rainbow.Medias.SDL2.SDL_CreateWindow("Output", Rainbow.Medias.SDL2.SDL_WINDOWPOS_UNDEFINED, Rainbow.Medias.SDL2.SDL_WINDOWPOS_UNDEFINED, 800, 600, flags);

            if (window.Handle != IntPtr.Zero)
            {
                window.Id = Rainbow.Medias.SDL2.SDL_GetWindowID(window.Handle);
                window.Renderer = Rainbow.Medias.SDL2.SDL_CreateRenderer(window.Handle, -1,
                        Rainbow.Medias.SDL2.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | Rainbow.Medias.SDL2.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
                ClearRenderer(window);
            }
        }

        public static void Destroy(Window window)
        {
            if (window is null) return;

            DestroyTexture(window);
            DestroyRenderer(window);

            if (window.Handle != IntPtr.Zero)
            {
                try
                {
                    Rainbow.Medias.SDL2.SDL_DestroyWindow(window.Handle);
                    window.Handle = IntPtr.Zero;
                }
                catch { }
            }
        }

        public static void Hide(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                Rainbow.Medias.SDL2.SDL_HideWindow(window.Handle);
        }

        public static void Show(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                Rainbow.Medias.SDL2.SDL_ShowWindow(window.Handle);
        }

        public static void Raise(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                Rainbow.Medias.SDL2.SDL_RaiseWindow(window.Handle);
        }

        public static void Restore(Window window)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                Rainbow.Medias.SDL2.SDL_RestoreWindow(window.Handle);
        }

        public static void UpdateTitle(Window window, string title)
        {
            if (window is not null && window.Handle != IntPtr.Zero)
                Rainbow.Medias.SDL2.SDL_SetWindowTitle(window.Handle, title);
        }

        public static void CreateTexture(Window window, int w, int h, AVPixelFormat pixelFormat)
        {
            if ((window is not null && window.Renderer != IntPtr.Zero))
            {
                // Destroy previous texture
                DestroyTexture(window);

                try
                {
                    var sdlFormat = Rainbow.Medias.SDL2Helper.GetPixelFormat(pixelFormat);
                    if (sdlFormat == Rainbow.Medias.SDL2.SDL_PIXELFORMAT_UNKNOWN)
                    {
                        Rainbow.Example.Common.Util.WriteRed($"Cannot get SDL pixel format using ffmpeg video foramt:[{pixelFormat}]");
                        return;
                    }

                    // Create texture
                    window.Texture = Rainbow.Medias.SDL2.SDL_CreateTexture(window.Renderer, sdlFormat, (int)Rainbow.Medias.SDL2.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, w, h);
                    if (window.Texture == IntPtr.Zero)
                        Rainbow.Example.Common.Util.WriteRed($"Cannot create texture");
                }
                catch { }
            }
            else
                Rainbow.Example.Common.Util.WriteRed($"Cannot create texture - No window renderer");
        }

        public static void DestroyTexture(Window window)
        {
            if ((window is not null && window.Texture != IntPtr.Zero))
            {
                try
                { 
                    Rainbow.Medias.SDL2.SDL_DestroyTexture(window.Texture);
                    window.Texture = IntPtr.Zero;
                }
                catch { }
            }
        }

        public static void DestroyRenderer(Window window)
        {
            if ((window is not null && window.Renderer != IntPtr.Zero))
            {
                try
                { 
                    Rainbow.Medias.SDL2.SDL_DestroyRenderer(window.Renderer);
                    window.Renderer = IntPtr.Zero;
                }
                catch { }
            }
        }

        public static void UpdateTexture(Window window, int stride, IntPtr data)
        {
            if (window is not null && window.Texture != IntPtr.Zero && data != IntPtr.Zero)
            {
                try
                {
                    var _ = Rainbow.Medias.SDL2.SDL_UpdateTexture(window.Texture, IntPtr.Zero, data, stride);
                }
                catch { }
            }
        }

        public static void ClearRenderer(Window window)
        {
            if (window is not null && window.Renderer != IntPtr.Zero)
            {
                try { 
                    if (Rainbow.Medias.SDL2.SDL_RenderClear(window.Renderer) == 0)
                        Rainbow.Medias.SDL2.SDL_RenderPresent(window.Renderer);
                }
                catch { }
            }
        }

        public static void UpdateRenderer(Window window)
        {
            if (window is not null && window.Renderer != IntPtr.Zero && window.Texture != IntPtr.Zero)
            {
                try
                {
                    window.NeedRendereUpdate = false;
                    if (Rainbow.Medias.SDL2.SDL_RenderCopy(window.Renderer, window.Texture, IntPtr.Zero, IntPtr.Zero) == 0)
                        Rainbow.Medias.SDL2.SDL_RenderPresent(window.Renderer);
                }
                catch { }
            }
        }

        public static void CheckUpdateRenderer(Window window)
        {
            if (window is not null && window.NeedRendereUpdate && !window.VideoStopped)
                UpdateRenderer(window);
        }
#endregion public static methods

    }
}
