namespace Yuriy.RabbitMq.Options
{
    public class ExchangeOptions
    {
        public string Name { get; }

        public string Type { get; }

        public Durability IsDurable { get; }

        public AutoDeletion IsAutoDelete { get; }

        public ExchangeOptions(string name, string type, Durability isDurable, AutoDeletion isAutoDelete)
        {
            Name = name;
            Type = type;
            IsDurable = isDurable;
            IsAutoDelete = isAutoDelete;
        }
    }
}