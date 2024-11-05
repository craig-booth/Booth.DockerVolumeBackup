using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.Docker.Models
{
    public class Volume
    {
        public string Name { get; set; }
        public string Mountpoint { get; set; }
        public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
        public VolumeUsageData UsageData { get; set; }
    }

    public class VolumeUsageData
    {
        public Int64 Size { get; set; }
        public int RefCount {get; set;}
    }
}
