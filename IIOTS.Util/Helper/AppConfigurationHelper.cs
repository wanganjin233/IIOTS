using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json; 

namespace IIOTS.Util
{
    public class AppConfigurationHelper
    {
        public static IConfiguration Configuration { get; set; }
        static AppConfigurationHelper()
        {          
            Configuration = new ConfigurationBuilder() 
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            .Build();
        }


    }
}
