using Open.Nat;
using System;
using System.Threading;

namespace PortOpener
{
    class Program
    {
        static void Main(string[] args)
        {
            OpenPort(args.Length > 0 ? Convert.ToInt32(args[0]) : 25565);
            Console.ReadLine();
        }

        static async void OpenPort(int port)
        {
            Console.WriteLine($"Trying to open port {port} using UPnP...");
            try
            {
                NatDiscoverer discoverer = new NatDiscoverer();
                CancellationTokenSource cts = new CancellationTokenSource(2500);
                NatDevice device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port, ""));

                Console.WriteLine($"Port {port} is open!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Can't open port {port} using UPnP!");
                Console.WriteLine($"Reason: {e.Message}");
            }
        }
    }
}
