namespace LogDigger.Gui.ViewModels.Pages.History
{
    public class DbEvent
    {
        public DbEvent(Entity entity, Operation operation)
        {
            Entity = entity;
            Operation = operation;
        }

        public Operation Operation { get; }
        public Entity Entity { get; }
    }
}