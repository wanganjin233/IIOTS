using IIOTS.CommUtil;
using IIOTS.Models;

namespace CEE.DefaultRecipe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SocketClientHelper sockerClient = new(args[0], new SocketClientInfo
            {
                ClientType = "CEE.DefaultRecipe",
                EQU = args[1],
                ProgressId = Environment.ProcessId
            });
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((Host, services) =>
                { 
                    services.AddHostedService<Worker>();
                    services.AddSingleton(sockerClient);
                    services.AddLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Trace);
                        //logging.AddProvider(new LoggerHelper((logInfo) =>
                        //{
                        //    Commands.Log(sockerClient, logInfo);
                        //}));
                    });
                })
                .Build();
            host.Run();
        }
    }
}