using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Core
{
    public class GameSettings
    {
        public bool IsHost { get; set; }
        public string HostIpAddress { get; set; }
        public ushort Port { get; set; }
        public string ListenOn { get; set; }
    }
}
