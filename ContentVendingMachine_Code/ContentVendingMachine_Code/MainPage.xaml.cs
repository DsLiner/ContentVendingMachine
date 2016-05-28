using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Networking.Proximity;
using Windows.UI.Core;

// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace ContentVendingMachine_Code
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Windows.Networking.Proximity.ProximityDevice _proximityDevice;

        public MainPage()
        {
            this.InitializeComponent();

            Synopsis.Text += "\n\n";
            try
            {
                _proximityDevice = ProximityDevice.GetDefault();
            }
            catch(Exception e)
            {
                Synopsis.Text += e.ToString() + "\n\n";
            }

            if (_proximityDevice != null)
            {
                _proximityDevice.DeviceArrived += DeviceArrived;
                _proximityDevice.DeviceDeparted += DeviceDeparted;
            }
            else
            {
                Synopsis.Text += "No proximity device found\n";
            }
        }

        void DeviceArrived(ProximityDevice proximityDevice)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Synopsis.Text += "Proximate device arrived\n";
            });
        }

        void DeviceDeparted(ProximityDevice proximityDevice)
        {
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                Synopsis.Text += "Proximate device departed\n";
            });
        }

        private void InputTextBlock1_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
