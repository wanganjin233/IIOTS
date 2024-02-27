using IIOTS.CommUtil;

namespace CEE.DefaultRecipe
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SocketClientHelper _sockerClient;

        public Worker(ILogger<Worker> logger, SocketClientHelper sockerClient)
        {
            _logger = logger;
            _sockerClient = sockerClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Tag> tags = CoreCommands.SubTags(_sockerClient, new List<string> { "EQP_MODEL" });
             foreach(Tag tag in tags)
            {
                tag.ValueChangeEvent += Tag_ValueChangeEvent;
            }
            List<Tag> tags1 = CoreCommands.SubMQs(_sockerClient, new List<string> { "MES.IssuedRecipe.Base/tes22" });
            foreach (Tag tag in tags1)
            {
                tag.ValueChangeEvent += Tag_ValueChangeEvent;
            }
        }

        private void Tag_ValueChangeEvent(Tag tag)
        {
            Console.WriteLine("name:"+tag.Value);
        }
    }
}