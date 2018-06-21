using System;

namespace LogDigger.Business.Models
{
    public class SessionId : IEquatable<SessionId>
    {
        public static SessionId NoSessionId = new SessionId(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        public SessionId(string rootId, string parentId, string id, string moduleName, string context)
        {
            RootId = rootId;
            ParentId = parentId;
            Id = id;
            ModuleName = moduleName;
            Context = context;
        }

        public string RootId { get; }
        public string ParentId { get; }
        public string Id { get; }
        public string Context { get; private set; }
        public string ModuleName { get; private set; }

        public static SessionId Parse(string id, SessionId defaultValue = null)
        {
            var trimmedId = id?.Trim();
            if (trimmedId == "[unidentified]")
            {
                return defaultValue;
            }
            var parts = trimmedId?.Split('|');
            if (parts == null || parts.Length == 1)
            {
                return defaultValue;
            }
            var ctx = parts[1];
            var idParts = parts[0].Split(new[] {"<-"}, StringSplitOptions.None);
            var currentId = idParts[0];
            var parentId = string.Empty;
            if (idParts.Length > 1)
            {
                parentId = idParts[1];
            }
            var rootId = currentId;
            if (idParts.Length > 2)
            {
                rootId = idParts[2];
            }
            else
            {
                rootId = currentId;
            }
            return new SessionId(rootId, parentId, currentId, string.Empty, ctx);
        }

        public bool Equals(SessionId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RootId.Equals(other.RootId) && ParentId.Equals(other.ParentId) && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SessionId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RootId.GetHashCode();
                hashCode = (hashCode * 397) ^ ParentId.GetHashCode();
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                return hashCode;
            }
        }
    }
}