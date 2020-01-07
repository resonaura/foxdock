using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YeelightAPI;
using YeelightAPI.Models.Scene;

namespace FoxDock.API
{
    public enum DeviceType
    {
        Yeelight,
    }
    public class SmartHomeDevice
    {
        public string name;
        public string model;
        public bool state;
        public DeviceType type;
        public Device YeelightDevice;
        public async void TurnOn()
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            state = await YeelightDevice.TurnOn(500);
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
        }
        public async Task<bool> IsTurnedOn()
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            object x = await YeelightDevice.GetProp(YeelightAPI.Models.PROPERTIES.power);
                            state = (string)x == "on";
                            return (string)x == "on";
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
            return state;
        }
        public async void TurnOff()
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            state = !(await YeelightDevice.TurnOff(500));
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
        }
        public async Task<int> GetBright()
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            object x = await YeelightDevice.GetProp(YeelightAPI.Models.PROPERTIES.bright);
                            return Int32.Parse(x.ToString());
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
            return 50;
        }
        public async Task<int> GetTemp()
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            object x = await YeelightDevice.GetProp(YeelightAPI.Models.PROPERTIES.ct);
                            return Int32.Parse(x.ToString());
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
            return 50;
        }
        public async void SetTemp(int value)
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            await YeelightDevice.SetColorTemperature(value, 500);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
        }
        public async void SetBright(int value)
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            await YeelightDevice.SetBrightness(value, 500);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
        }
        public async void NightModeOn()
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            await YeelightDevice.SetBrightness(1, 500);
                            await YeelightDevice.SetColorTemperature(2700, 500);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
        }
        public async void DayModeOn()
        {
            try
            {
                switch (type)
                {
                    case DeviceType.Yeelight:
                        await YeelightDevice.Connect();
                        if (YeelightDevice.IsConnected)
                        {
                            await YeelightDevice.SetBrightness(100, 500);
                            await YeelightDevice.SetColorTemperature(2700, 500);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SHD Error: " + ex.ToString());
            }
        }
    }
    class SmartHome
    {
        public async static Task<List<SmartHomeDevice>> Discover()
        {
            List<SmartHomeDevice> homeDevices = new List<SmartHomeDevice>();
            List<Device> devices = await DeviceLocator.Discover();

            foreach(Device device in devices)
            {
                homeDevices.Add(new SmartHomeDevice
                {
                    name = device.Name,
                    model = StringTools.AddSpacesToSentence(device.Model.ToString()),
                    type = DeviceType.Yeelight,
                    state = false,
                    YeelightDevice = device
                });
            }
            return homeDevices;
        }
    }
}
