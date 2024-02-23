using IIOTS.EdgeDriver; 
using IIOTS.Models;
using IIOTS.Util;
using NetMQ.Sockets; 
var builder = Host.CreateApplicationBuilder(args); 
var progressLoginInfo = new ProgressLoginInfo()
{
    ClientType = "IIOTS.EdgeDriver",
    Name = args[2] 
};
Config.Identifier = progressLoginInfo.ClientId;
new IdHelperBootstrapper()
.SetWorkderId(args[3].ToLong())
.Boot();
builder.Services.AddSingleton(progressLoginInfo);
builder.Services.AddHostedService<DriverService>();
builder.Services.AddSingleton(new PublisherSocket($">tcp://{args[0]}"));
builder.Services.AddSingleton(new SubscriberSocket($">tcp://{args[1]}"));
var host = builder.Build();
host.Run();
