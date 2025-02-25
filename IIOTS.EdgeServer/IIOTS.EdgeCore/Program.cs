using IIOTS.EdgeCore.Extension;
using IIOTS.EdgeCore.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.EnvInfo(args);
builder.UseIdHelper();
builder.Services.AddHostedService<CoreService>();
builder.Services.AddHostedService<ProgressService>();
builder.Services.AddInfuxdb(o =>
{
    o.Host = new Uri(builder.Configuration.GetSection("Infuxdb:Host").Get<string>());
    o.Token = builder.Configuration.GetSection("Infuxdb:Token").Get<string>();
    o.DefaultOrg = builder.Configuration.GetSection("Infuxdb:DefaultOrg").Get<string>();
    o.DefaultBucket = builder.Configuration.GetSection("Infuxdb:DefaultBucket").Get<string>();
});
var host = builder.Build();
host.Run();
