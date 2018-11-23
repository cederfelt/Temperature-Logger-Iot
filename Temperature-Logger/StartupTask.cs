using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using IotDeviceLibrary;
using IotDeviceLibrary.BMP280;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Temperature_Logger
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {

            BackgroundTaskDeferral btd = taskInstance.GetDeferral();

            try
            {
                BMP280 bmp280 = new BMP280();
                await bmp280.Initialize();
                const float sea = 1019.4f;
                var temp =  bmp280.ReadTemperature();
                var pressure = bmp280.ReadPreasure();
                var altitude = bmp280.ReadAltitude(sea);

                //Write the values to your debug console
                Debug.WriteLine("Temperature: " + temp.ToString() + " deg C");
                Debug.WriteLine("Pressure: " + pressure.ToString() + " Pa");
                Debug.WriteLine("Altitude: " + altitude.ToString() + " m");

            }
            catch (Exception e)
            {

            }
            btd.Complete();
        }
    }
}
