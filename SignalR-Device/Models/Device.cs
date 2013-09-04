using System;

namespace SignalR_Device.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal AspectRatio { get; set; } 
    }
}