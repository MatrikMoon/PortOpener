using Open.Nat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PortOpener
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<int, string> portsToOpen = new Dictionary<int, string>();

            var argv = string.Join(" ", args);
            var portsParameter = argv.ParseArgs("ports");

            if (!string.IsNullOrEmpty(portsParameter))
            {
                var ports = portsParameter.Split(",").Select(x => int.Parse(x)).ToArray();
                var types = new string[ports.Length];

                var typesParameter = argv.ParseArgs("types");
                if (!string.IsNullOrEmpty(typesParameter))
                {
                    types = typesParameter.Split(",");
                }
                else ports.Each((port, index) => types[index] = "tcp");

                ports.Each((port, index) => portsToOpen[port] = types[index]);
            }
            else
            {
                portsParameter = argv.ParseArgs("port");
                var port = int.Parse(portsParameter);
                var type = argv.ParseArgs("type");
                type = string.IsNullOrEmpty(type) ? "tcp" : type;

                portsToOpen[port] = type;
            }

            OpenPorts(portsToOpen);
            Console.ReadLine();
        }

        static async void OpenPorts(Dictionary<int, string> ports)
        {
            var portsString = string.Empty;
            ports.ToList().ForEach(x => portsString += $"{{{x.Key},{x.Value}}}, ");
            Console.WriteLine($"Trying to open: {portsString} using UPnP...");
            try
            {
                NatDiscoverer discoverer = new NatDiscoverer();
                CancellationTokenSource cts = new CancellationTokenSource(2500);
                NatDevice device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                ports.ToList().ForEach(async x => await device.CreatePortMapAsync(new Mapping(x.Value == "tcp" ? Protocol.Tcp : Protocol.Udp, x.Key, x.Key, "")));

                Console.WriteLine($"Ports are open!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Can't open ports using UPnP!");
                Console.WriteLine($"Reason: {e.Message}");
            }
        }
    }
}
