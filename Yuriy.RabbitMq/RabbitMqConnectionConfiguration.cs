namespace Yuriy.RabbitMq
{
    public interface IRabbitMqConnectionConfiguration
    {
        string UserName { get; }
        string Password { get; }
        string HostName { get; }
        string VirtualHost { get; }
    }

    public class RabbitMqConnectionConfiguration : IRabbitMqConnectionConfiguration
    {
        public string UserName { get; set; }
        
        public string Password { get; set; }
        
        public string HostName { get; set; }
        
        public string VirtualHost { get; set; }
    }
}