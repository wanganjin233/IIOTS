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
var dbOptions = builder.Configuration.GetSection("Database:BaseDb").Get<DatabaseOptions>();
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
        .UseConnectionString(dbOptions.DatabaseType, dbOptions.ConnectionString)
        .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
        .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
        .Build();
builder.Services.AddSingleton(fsql);
builder.Services.AddSingleton(new ProgressManage(edgeLoginInfo));
builder.UseIdHelper();
builder.Services.AddHostedService<CoreService>();
builder.Services.AddHostedService<ProgressService>();
var host = builder.Build();
host.Run();
