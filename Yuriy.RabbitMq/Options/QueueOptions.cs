namespace Yuriy.RabbitMq.Options
{
    public class QueueOptions
    {
        public string Name { get; set; }

        public string RoutingKey { get; set; }

        public Durability IsDurable { get; set; }

        public AutoDeletion IsAutoDelete { get; set; }

        public Exclusivity IsExclusive { get; set; }

        public QueueOptions(string name, string routingKey, Durability isDurable, AutoDeletion isAutoDelete, Exclusivity isExclusive)
        {
            Name = name;
            RoutingKey = routingKey;
            IsDurable = isDurable;
            IsAutoDelete = isAutoDelete;
            IsExclusive = isExclusive;
        }
    }

}