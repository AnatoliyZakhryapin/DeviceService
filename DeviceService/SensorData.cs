using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService
{
    internal class SensorData
    {
        public int SensorId {  get; set; }
        public DateTime Timestamp { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public double Value { get; set; }
    }
}
