namespace LogDigger.Gui.ViewModels.Core
{
    public class SelectableVm : AViewModel
    {
        private string _name;

        protected SelectableVm()
        {
        }

        public SelectableVm(string name, bool isActive)
        {
            _name = name;
            IsActive = isActive;
        }

        public string Name
        {
            get { return _name; }
            protected set { _name = value; }
        }

        public virtual bool? IsActive
        {
            get { return GetProperty<bool?>(); }
            set { SetProperty(value); }
        }
    }
}