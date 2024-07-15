using IIOTS.EdgeCore.Extension;
using IIOTS.EdgeCore.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.EnvInfo(args);
builder.UseIdHelper();
builder.Services.AddHostedService<CoreService>();
builder.Services.AddHostedService<ProgressService>();
var host = builder.Build();
host.Run();
