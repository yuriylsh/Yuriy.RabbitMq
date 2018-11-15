namespace Yuriy.RabbitMq.Options
{
    public class Exclusivity
    {
        private readonly bool _representation;

        public static Exclusivity Yes { get; } = new Exclusivity(true);

        public static Exclusivity No { get; } = new Exclusivity(false);

        public static implicit operator bool(Exclusivity autoDeletion) => autoDeletion._representation;

        private Exclusivity(bool representation) => _representation = representation;
    }
}