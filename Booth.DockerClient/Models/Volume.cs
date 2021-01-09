using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerClient.Models
{
    public class Volume
    {
        public string Name { get; set; }
        public string Mountpoint { get; set; }
        public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
    }
}
