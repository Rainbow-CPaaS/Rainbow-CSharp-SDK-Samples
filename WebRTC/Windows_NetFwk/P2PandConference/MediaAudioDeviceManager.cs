using Rainbow.Medias;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace SDK.UIForm.WebRTC
{
    public sealed class MediaAudioDeviceManager
    {
        private static MediaAudioDeviceManager? _instance = null;

        private SDL2AudioInput? _currentAudioInput = null;
        private SDL2AudioOutput? _currentAudioOutput = null;

#region EVENT(S)

        public event EventHandler? OnAudioInputDeviceChanged;
        public event EventHandler? OnAudioOutputDeviceChanged;

#endregion EVENT(S)


#region PUBLIC API

        public SDL2AudioInput? AudioInputDevice { get => _currentAudioInput; }

        public SDL2AudioOutput? AudioOutputDevice { get => _currentAudioOutput; }

        public void CloseAudioInputDevice(Boolean raiseEvent = true)
        {
            if (_currentAudioInput != null)
            {
                _currentAudioInput.Dispose();
                _currentAudioInput = null;
            }
            if (raiseEvent)
                OnAudioInputDeviceChanged?.Invoke(this, null);
        }

        public void CloseAudioOutputDevice(Boolean raiseEvent = true)
        {
            if (_currentAudioOutput != null)
            {
                _currentAudioOutput.Dispose();
                _currentAudioOutput = null;

                if(raiseEvent)
                    OnAudioOutputDeviceChanged?.Invoke(this, null);
            }
        }

        public SDL2AudioInput? SetAudioInputDevice(String deviceName)
        {
            if (_currentAudioInput?.Id == deviceName)
            {
                OnAudioInputDeviceChanged?.Invoke(this, null);
                return _currentAudioInput;
            }

            SDL2AudioInput audio = new SDL2AudioInput(deviceName, deviceName);
            if (audio.Init())
            {
                audio.Start();

                // Close previous one
                CloseAudioInputDevice(false);

                // Store new one
                _currentAudioInput = audio;

                OnAudioInputDeviceChanged?.Invoke(this, null);
                return _currentAudioInput;
            }
            else
            {
                // Close previous one
                CloseAudioInputDevice();
            }
            return null;
        }

        public SDL2AudioOutput? SetAudioOutputDevice(String deviceName)
        {
            if (_currentAudioOutput?.Id == deviceName)
            {
                OnAudioOutputDeviceChanged?.Invoke(this, null);
                return _currentAudioOutput;
            }

            SDL2AudioOutput audio = new SDL2AudioOutput(deviceName, deviceName);
            if (audio.Init())
            {
                audio.Start();

                // Close previous one
                CloseAudioOutputDevice(false);

                // Store new one
                _currentAudioOutput = audio;

                OnAudioOutputDeviceChanged?.Invoke(this, null);
                return _currentAudioOutput;
            }
            else
            {
                // Close previous one
                CloseAudioOutputDevice();
            }
            return null;
        }

        public static MediaAudioDeviceManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MediaAudioDeviceManager();
                return _instance;
            }
        }

#endregion PUBLIC API


    }
}
