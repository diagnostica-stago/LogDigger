using System.Collections.Generic;
using System.Linq;

namespace LogDigger.Gui.ViewModels.Pages.History
{
    public class EntityGroup
    {
        public EntityGroup(string type, IEnumerable<Entity> entities)
        {
            Type = type;
            Entities = entities.ToList();
        }

        public string Type { get; }
        public List<Entity> Entities { get; }
    }
}