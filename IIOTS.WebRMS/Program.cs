using AntDesign;
using AntDesign.ProLayout;
using Blazored.LocalStorage;
using IIOTS.WebRMS;
using IIOTS.WebRMS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

LocaleProvider.DefaultLanguage = "zh-CN";
var builder = WebApplication.CreateBuilder(args);
builder.UseIdHelper().UseCache();
builder.UseNodeRedApi();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAntDesign();
builder.Services.AddServerSideBlazor();

JWTTokenOptions tokenOptions = builder.Configuration.GetSection("JWTTokenOptions").Get<JWTTokenOptions>(); 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,//是否验证Issuer
                ValidateAudience = true,//是否验证Audience
                ValidateLifetime = true,//是否验证失效时间
                ValidateIssuerSigningKey = true,//是否验证SecurityKey
                ValidAudience = tokenOptions.Audience,//Audience
                ValidIssuer = tokenOptions.Issuer,//Issuer 
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey))//拿到SecurityKey
            };
        });
builder.Services.AddScoped(sp => new HttpClient
    {
        BaseAddress = new Uri(sp.GetService<NavigationManager>()!.BaseUri)
    })
    .AddBlazoredLocalStorage()
    .AddScoped<AuthenticationStateProvider, AuthProvider>()
    .AddAuthorizationCore(option =>
    {
        option.AddPolicy("Admin", policy => policy.RequireClaim("Admin"));
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
builder.Services.AddInfuxdb(o =>
{
    o.Host = new Uri(builder.Configuration.GetSection("Infuxdb:Host").Get<string>());
    o.Token = builder.Configuration.GetSection("Infuxdb:Token").Get<string>();
    o.DefaultOrg = builder.Configuration.GetSection("Infuxdb:DefaultOrg").Get<string>();
    o.DefaultBucket = builder.Configuration.GetSection("Infuxdb:DefaultBucket").Get<string>();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();
app.Run();