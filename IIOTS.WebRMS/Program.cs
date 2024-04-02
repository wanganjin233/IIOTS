using AntDesign;
using AntDesign.ProLayout;
using Blazored.LocalStorage;
using IIOTS.WebRMS;
using IIOTS.WebRMS.Services; 
using Microsoft.AspNetCore.Components;
LocaleProvider.DefaultLanguage = "zh-CN";
var builder = WebApplication.CreateBuilder(args);
builder.UseIdHelper().UseCache();
builder.UseNodeRedApi();
builder.Services.AddBlazoredLocalStorage();
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAntDesign();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(sp.GetService<NavigationManager>()!.BaseUri)
});
builder.Services.Configure<ProSettings>(builder.Configuration.GetSection("ProSettings"));
builder.Services.AddTransient<IMqttClientService, MqttClientService>();
var dbOptions = builder.Configuration.GetSection("Database:BaseDb").Get<DatabaseOptions>();
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
        .UseConnectionString(dbOptions.DatabaseType, dbOptions.ConnectionString)
        .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
        .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
        .Build(); 
builder.Services.AddSingleton(fsql);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();