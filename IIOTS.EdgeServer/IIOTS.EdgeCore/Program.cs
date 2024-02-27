using IIOTS.EdgeCore;
using IIOTS.EdgeCore.Extension;
using IIOTS.EdgeCore.Manage;
using IIOTS.EdgeCore.Service;
using IIOTS.Models; 

var builder = Host.CreateApplicationBuilder(args);
var edgeID = Environment.GetEnvironmentVariable("EdgeID", EnvironmentVariableTarget.Process);
EdgeLoginInfo edgeLoginInfo = new();
if (args.Length > 0)
{
    edgeLoginInfo.EdgeID = args[0];
}
else if (edgeID != null)
{
    edgeLoginInfo.EdgeID = edgeID;
}
else
{
    edgeLoginInfo.EdgeID = builder.Configuration.GetSection("EdgeID").Get<string>();
} 
builder.Services.AddSingleton(new ProgressManage(edgeLoginInfo));
builder.UseIdHelper();
builder.Services.AddHostedService<CoreService>();
builder.Services.AddHostedService<ProgressService>();
var host = builder.Build();
host.Run();
