// ReSharper disable once CheckNamespace
namespace Yuriy.RabbitMq.Options
{
    public class Durability
    {
        private readonly bool _representation;

        public static Durability Durable { get; } = new Durability(true);

        public static Durability Transient { get; } = new Durability(false);

        public static implicit operator bool(Durability durability) => durability._representation;

        private Durability(bool representation) => _representation = representation;
    }
}