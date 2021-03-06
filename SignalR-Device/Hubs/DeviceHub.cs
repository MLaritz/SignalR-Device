﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using SignalR_Device.Helpers;
using SignalR_Device.Models;

namespace SignalR_Demo.Hubs
{
    public class DeviceHub : Hub
    {

        public static readonly List<Device> _devices;

        public const string NormalDevice = "NormalDevice";
        public const string ExtraPixelRatioDevice = "ExtraPixelRatioDevice";
        public const string DashboardGroup = "DashboardGroup";

        static DeviceHub()
        {
            _devices = new List<Device>();
        }

        public void Send(int deviceWidth, int deviceHeight, decimal aspectRatio)
        {
            var ip = Context.Request.GetHttpContext().Request.ClientIPFromRequest(false);
            var device = new Device
                {
                    Height = deviceHeight,
                    Width = deviceWidth,
                    PixelRatio = aspectRatio,
                    IpAddress = ip,
                    Id = new Guid(Context.ConnectionId)
                };
            _devices.Add(device);

            if (aspectRatio > 1)
            {
                Groups.Add(Context.ConnectionId, ExtraPixelRatioDevice);
                Clients.Group(DashboardGroup).sendRetinaDeviceCount(_devices.Count(x => x.PixelRatio > 1));
            }
            else
            {
                Groups.Add(Context.ConnectionId, NormalDevice);
                Clients.Group(DashboardGroup).sendNormalDeviceCount(_devices.Count(x => x.PixelRatio == 1));
            }

            Clients.Group(DashboardGroup).userConnected(device);
            Clients.All.sendNumberOfDevices(_devices.Count);
        }

        public void DashboardConnected()
        {
            Groups.Add(Context.ConnectionId, DashboardGroup);
            Clients.Caller.dashboardConnected(_devices);
            Clients.Caller.sendNormalDeviceCount(_devices.Count(x => x.PixelRatio == 1));
            Clients.Caller.sendRetinaDeviceCount(_devices.Count(x => x.PixelRatio > 1));
        }

        public void SendMessage(Guid id, string message)
        {
            Clients.Client(id.ToString()).sendMessage(message);
        }

        public void SendRetinaMessage(string message)
        {
            Clients.Group(ExtraPixelRatioDevice).sendMessage(message);
        }

        public void SendNormalMessage(string message)
        {
            Clients.Group(NormalDevice).sendMessage(message);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            Clients.All.sendNumberOfDevices(_devices.Count);
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
            Clients.All.sendNumberOfDevices(_devices.Count);
            Clients.Group(DashboardGroup).userDisconnected(Context.ConnectionId);
            return base.OnReconnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            _devices.Remove(_devices.FirstOrDefault(x => x.Id == new Guid(Context.ConnectionId)));
            Clients.All.sendNumberOfDevices(_devices.Count);
            Clients.Group(DashboardGroup).sendNormalDeviceCount(_devices.Count(x => x.PixelRatio == 1));
            Clients.Group(DashboardGroup).sendRetinaDeviceCount(_devices.Count(x => x.PixelRatio > 1));
            return base.OnDisconnected();
        }
    }

}