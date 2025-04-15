using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryService.Infrastructure.Scanner
{
    public class ScannerSettings
    {
        public HostConnection HostConnection { get; set; } = new();
        public ScannerConfig ScannerConfig { get; set; } = new();
    }

    public class HostConnection
    {
        public string IpAddressHost { get; set; } = string.Empty;
        public int PortHost { get; set; }
    }

    public class ScannerConfig
    {
        public string Code { get; set; } = string.Empty;
        public Connection Connection { get; set; } = new();
    }

    public class Connection
    {
        public string IpAddressReader { get; set; } = string.Empty;
        public int PortReader { get; set; }
    }
}
