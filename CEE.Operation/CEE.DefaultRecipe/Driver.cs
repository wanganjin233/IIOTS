using IIOTS.Models;
using IIOTS.Util;
using NetMQ.Sockets;

namespace CEE.DefaultRecipe
{
    internal static class Driver
    {  /// <summary>
       /// 类名
       /// </summary>
        private readonly static string className = typeof(Driver).Name;
        public static bool WriteTag(this PublisherSocket publisher, string clientId, Operate<Tag> tag)
        {
            return publisher.Reply<bool>($"{clientId}/{className}/WriteTag", tag);
        }
    }
}
