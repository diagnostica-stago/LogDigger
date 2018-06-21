using System.Collections.Generic;
using System.Linq;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Core;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Pages.Summaries
{
    public class CustomLogTemplateVm : AViewModel
    {
        public CustomLogTemplateVm(CustomLogTemplate template)
        {
            Template = template;
        }

        public bool IsException
        {
            get { return Template.IsException; }
            set
            {
                Template.IsException = value;
                this.RaisePropertyChanged();
            }
        }

        public string Pattern
        {
            get { return Template.Pattern; }
            set
            {
                Template.Pattern = value;
                this.RaisePropertyChanged();
            }
        }

        public string TypeColor
        {
            get { return Template.TypeColor; }
            set
            {
                Template.TypeColor = value;
                this.RaisePropertyChanged();
            }
        }

        public string TypeName
        {
            get { return Template.TypeName; }
            set
            {
                Template.TypeName = value;
                this.RaisePropertyChanged();
            }
        }

        public string Details
        {
            get { return Template.Details; }
            set
            {
                Template.Details = value;
                this.RaisePropertyChanged();
            }
        }

        public string Childpattern
        {
            get { return Template.Subpatterns?.FirstOrDefault(); }
            set
            {
                Template.Subpatterns = new List<string> { value };
                this.RaisePropertyChanged();
            }
        }

        public CustomLogTemplate Template { get; }
    }
}