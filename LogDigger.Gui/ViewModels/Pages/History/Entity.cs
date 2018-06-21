using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LogDigger.Gui.ViewModels.Pages.History
{
    public class Entity : AField
    {
        public Entity(string type, string id, List<AField> fields)
        {
            Type = type;
            Id = id;
            InternalFields = fields;
        }

        public string Type { get; }
        public string Id { get; }

        private List<AField> InternalFields { get; }

        public IEnumerable<AField> Fields
        {
            get
            {
                foreach (var field in InternalFields)
                {
                    switch (field)
                    {
                        case ContentField _:
                            yield return field;
                            break;
                        case Entity currentEntity:
                            EntitiesLookup.TryGetValue(currentEntity.HashId, out Entity newEntity);
                            yield return newEntity ?? currentEntity;
                            break;
                    }
                }
            }
        }

        public string HashId => Type + Id;

        public ConcurrentDictionary<string, Entity> EntitiesLookup { get; set; }

        public override bool Match(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }
            return Type.Contains(filter) || Id.Contains(filter) || (Fields?.Any(x => x.Match(filter)) ?? false);
        }
    }
}