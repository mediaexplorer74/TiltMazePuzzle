using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Xamarin.Forms;

using System.ServiceModel;

using static TiltMazePuzzle.MainPage;

using Windows.Devices.Sensors; // !
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

[assembly: Dependency(typeof(TiltMazePuzzle.UWP.DeviceInfo))]
namespace TiltMazePuzzle.UWP
{
    public class DeviceInfo : IDeviceInfo
    {

        private Accelerometer _accelerometer;

        public AccelerometerReading ScenarioOutput;

        public bool GetInfo()
        {
            ScenarioOutput = null;

            _accelerometer = Accelerometer.GetDefault();

            if (_accelerometer != null)
            {
                _accelerometer.ReportInterval = Math.Max(_accelerometer.MinimumReportInterval, 16);
                // навешиваем обработчик изм. значений акселерометра
                _accelerometer.ReadingChanged += ReadingChanged;

                return true; //$"Accelerometer found!";
            }
            else 
            {
                return false; //$"Accelerometer not found";
            }
        }

        public double GetX()
        {
            try
            {
                return ScenarioOutput.AccelerationX;
            }
            catch { return 0; }
            
        }

        public double GetY()
        {
            try
            {
                return ScenarioOutput.AccelerationY;
            }
            catch { return 0; }
        }

        public double GetZ()
        {
            try
            {
                return ScenarioOutput.AccelerationZ;
            }
            catch { return 0; }
        }

        /// <summary>
        /// This is the event handler for ReadingChanged events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            
            
            ScenarioOutput = e.Reading;
            /*
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //MainPage.SetReadingText(ScenarioOutput, e.Reading);
            });
            */
        }
    }
}
