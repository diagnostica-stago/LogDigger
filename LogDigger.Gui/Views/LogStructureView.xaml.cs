using System.Reflection;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace LogDigger.Gui.Views
{
    public partial class LogStructureView
    {
        public LogStructureView()
        {
            InitializeComponent();
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("LogDigger.Gui.logtemplate.xshd"))
            {
                using (var reader = new XmlTextReader(s))
                {
                    xamlTextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
    }
}
