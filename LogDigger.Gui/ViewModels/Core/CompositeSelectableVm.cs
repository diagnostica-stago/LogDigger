using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Core
{
    public class CompositeSelectableVm : SelectableVm
    {
        public ReactiveList<SelectableVm> Children { get; }

        public CompositeSelectableVm(string name, bool isActive)
        {
            Children = new ReactiveList<SelectableVm> { ChangeTrackingEnabled = true };
            Name = name;
            IsActive = isActive;
        }

        public CompositeSelectableVm(string name, bool isActive, IEnumerable<SelectableVm> children)
           : this(name, isActive)
        {
            Children = new ReactiveList<SelectableVm>(children) { ChangeTrackingEnabled = true };
            Children.ItemChanged.Where(x => x.PropertyName == nameof(IsActive)).Select(x => x.Sender)
                .Subscribe(x =>
                {
                    this.RaisePropertyChanged(nameof(IsActive));
                });
        }

        public override bool? IsActive
        {
            get
            {
                if (Children.All(x => x.IsActive ?? false))
                {
                    return true;
                }
                if (Children.Any(x => x.IsActive ?? false))
                {
                    return null;
                }
                return false;
            }
            set
            {
                foreach (var child in Children)
                {
                    child.IsActive = value;
                }
            }
        }
    }
}