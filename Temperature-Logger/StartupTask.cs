using IotDeviceLibrary.BMP280;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Temperature_Logger
{
    public sealed class StartupTask : IBackgroundTask
    {
        //static RegistryManager registryManager;
        //static string connectionString = "";

        //private static void Register()
        //{
        //    var deviceId = "Raspberry3";
        //    registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        //    var deviceTask = registryManager.AddDeviceAsync(new Device(deviceId));
        //    deviceTask.Wait();
        //    Debug.WriteLine($"Key {deviceTask.Result.Authentication.SymmetricKey}");
        //}

        private DeviceClient _deviceClient;
        private ThreadPoolTimer timer;
        private BackgroundTaskDeferral btd;

        //only used as startup
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            string iotHubUrl = "nonsens";

            btd = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            timer = ThreadPoolTimer.CreatePeriodicTimer(TemperatureCallback, TimeSpan.FromSeconds(5));
            _deviceClient = DeviceClient.Create(iotHubUrl, new DeviceAuthenticationWithRegistrySymmetricKey("deviceid", "key"), TransportType.Http1);
        }

        private async void TemperatureCallback(ThreadPoolTimer timer)
        {
            try
            {
                BMP280 bmp280 = new BMP280();
                await bmp280.InitializeAsync();
                const float sea = 1019.4f;
                var temp = bmp280.ReadTemperature();
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
        }

        private void ReportMeasurement(double temp, double pressure, double altitude)
        {
            var temperatureMeasurement = new
            {
                deviceId = "deviceId",
                timeStamp = DateTime.UtcNow,
                temperature = temp,
                pressure = pressure,
                altitude = altitude,
            };

            var messageString = JsonConvert.SerializeObject(temperatureMeasurement);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            _deviceClient.SendEventAsync(message).Wait();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            timer.Cancel();
            btd.Complete();
        }
    }
}
