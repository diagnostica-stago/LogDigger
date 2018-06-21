using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace LogDigger.Gui.ViewModels.Pages.History
{
    public class EntityParser
    {
        private static readonly Parser<string> Type = Parse.LetterOrDigit.Or(Parse.Char('`')).AtLeastOnce().Text().Token();
        private static readonly Parser<string> Id = Parse.LetterOrDigit.AtLeastOnce().Text().Token();

        private static readonly Parser<List<AField>> Fields =
            from lpara in Parse.Char('{').Token()
            from lws in Parse.WhiteSpace.Many()
            from fields in AbstractField.DelimitedBy(Parse.String(", "))
            from rws in Parse.WhiteSpace.Many()
            from rpara in Parse.Char('}').Token()
            select new List<AField>(fields);

        public static readonly Parser<ContentField> ContentField =
            from str in Parse.AnyChar.Except(Parse.Char(',').Or(Parse.Char('}'))).Many()
            select new ContentField(new string(str.ToArray()));

        public static readonly Parser<Entity> Entity =
            from type in Type
            from lbracket in Parse.Char('[').Token()
            from id in Id
            from rbracket in Parse.Char(']').Token()
            from fields in Fields.Optional()
            select new Entity(type, id, fields.IsDefined ? fields.Get() : new List<AField>());

        public static readonly Parser<AField> AbstractField =
            (from entity in Entity
            select (AField) entity)
            .Or(from content in ContentField
            select content);

        public static readonly Parser<Operation> Operation =
            (from str in Parse.String("insert")
            select History.Operation.Create)
            .Or(from str in Parse.String("update")
                select History.Operation.Update)
            .Or(from str in Parse.String("delete")
                select History.Operation.Delete);

        public static readonly Parser<DbEvent> DbEvent =
            from lws in Parse.WhiteSpace.Many()
            from dbstr in Parse.String("db-")
            from op in Operation
            from pts in Parse.Char(':').Token()
            from ws in Parse.WhiteSpace.Many()
            from entity in Entity
            from trws in Parse.WhiteSpace.Many()
            select new DbEvent(entity, op);
    }
}