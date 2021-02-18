using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace SDK.WpfApp.Model
{
    /// <summary>
    /// Model used in History window
    /// </summary>
    public class DevicesModel : ObservableObject
    {
        public ObservableCollection<DeviceModel> AudioInputDevices { get; set; }
        public ObservableCollection<DeviceModel> VideoInputDevices { get; set; }
        public ObservableCollection<DeviceModel> AudioOutputDevices { get; set; }

        public DeviceModel AudioInputDeviceSelected { get; set; }
        public DeviceModel VideoInputDeviceSelected { get; set; }
        public DeviceModel AudioOutputDeviceSelected { get; set; }

        public DevicesModel()
        {
            AudioInputDeviceSelected = new DeviceModel();
            VideoInputDeviceSelected = new DeviceModel();
            AudioOutputDeviceSelected = new DeviceModel();

            AudioInputDevices = new ObservableCollection<DeviceModel>();
            VideoInputDevices = new ObservableCollection<DeviceModel>();
            AudioOutputDevices = new ObservableCollection<DeviceModel>();
        }
    }
    
}