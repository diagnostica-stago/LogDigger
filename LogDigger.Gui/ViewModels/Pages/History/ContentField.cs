namespace LogDigger.Gui.ViewModels.Pages.History
{
    public class ContentField : AField
    {
        public ContentField(string content)
        {
            Content = content;
        }

        public string Content { get; }

        public override bool Match(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }
            return Content.Contains(filter);
        }
    }
}