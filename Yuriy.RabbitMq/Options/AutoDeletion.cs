namespace Yuriy.RabbitMq.Options
{
    public class AutoDeletion
    {
        private readonly bool _representation;

        public static AutoDeletion Yes { get; } = new AutoDeletion(true);

        public static AutoDeletion No { get; } = new AutoDeletion(false);

        public static implicit operator bool(AutoDeletion autoDeletion) => autoDeletion._representation;

        private AutoDeletion(bool representation) => _representation = representation;
    }
}