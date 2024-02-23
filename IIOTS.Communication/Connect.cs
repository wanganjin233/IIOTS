using IIOTS.Interface;
using IIOTS.Util;
using System.Text.RegularExpressions;

namespace IIOTS.Communication
{
    public static partial class Connect
    {
        /// <summary>
        /// COM1,9600,8,无,1
        /// </summary>
        /// <param name="connectionStr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ICommunication ConnectionResolution(string connectionStr)
        {
            if (PortRegex().Matches(connectionStr).Count != 0)
            {
                string[] connectionStrSplit = connectionStr.Split(',');
                return new SerialPort(connectionStrSplit[0],
                                      connectionStrSplit[1].ToInt(),
                                      connectionStrSplit[2].ToInt(),
                                      connectionStrSplit[3],
                                      connectionStrSplit[4]);
            }
            else if (SocketRegex().Matches(connectionStr).Count != 0)
            {
                return new SocketClient(connectionStr);
            }
            throw new Exception("未找到合适的连接");
        }

        [GeneratedRegex("COM[0-9]+,[0-9]+,[0-9],.+,.+")]
        private static partial Regex PortRegex();
        [GeneratedRegex("[0-9]+[.][0-9]+[.][0-9]+[.][0-9]+[:][0-9]+")]
        private static partial Regex SocketRegex();
    }
}
