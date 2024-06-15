using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Avia
{
    public static class ServerPinger
    {
        private const string ServerAddress = "den1.mssql7.gear.host";

        public static bool PingServer()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ServerAddress);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while pinging the server: {ex.Message}");
                return false;
            }
        }
    }
}
