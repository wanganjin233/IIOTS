using IIOTS.Models;
using IIOTS.Util;
using NetMQ.Sockets; 

namespace IIOTS.EdgeCore.Command
{
    internal static class Driver
    {  /// <summary>
       /// 类名
       /// </summary>
        private readonly static string className = typeof(Driver).Name;

        public static List<string>? GetDrivers(this PublisherSocket publisher, string clientId)
        {
            return publisher.Reply<List<string>>($"{clientId}/{className}/GetDrivers");
        }

        public static bool WriteTag(this PublisherSocket publisher, string clientId, Operate<Tag> tag)
        {
            return publisher.Reply<bool>($"{clientId}/{className}/WriteTag", tag);

        }
    }
}
