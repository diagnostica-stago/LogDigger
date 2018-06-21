using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using LogDigger.Business.Utils;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.Core
{
    public class HighlightSearchItemVm<TItem> : AViewModel, IHighlightSearchItemVm
    {
        private readonly IItemsHolder<TItem> _parent;
        private readonly List<Func<TItem, string>> _fieldsAccessors;

        public HighlightSearchItemVm(IItemsHolder<TItem> parent, string color, params Func<TItem, string>[] accessors)
        {
            _parent = parent;
            _fieldsAccessors = accessors.ToList();
            Color = (Color) (new ColorConverter().ConvertFrom(color));

            var whenAnyHl = this.WhenAnyValue(x => x.Text).Where(x => _parent.IsLoading == false);
            whenAnyHl.Throttle(TimeSpan.FromMilliseconds(300)).Subscribe(b => OnTextChanged());
        }

        public async Task OnTextChanged()
        {
            await RefreshHighlights();
            if (SelectedIndex == null)
            {
                await CallGoToNext();
            }
            _parent.SetSelectedItem(SelectedPoint);
        }

        public Task RefreshHighlights()
        {
            return Task.Run(() =>
            {
                var items = _parent.Items;
                if (items == null)
                {
                    return;
                }
                var totalFilteredEntries = items.Count;
                var indexes = new ObservableCollection<int>();
                var points = new ObservableCollection<double>();
                for (int i = 0; i < totalFilteredEntries; i++)
                {
                    if (!string.IsNullOrEmpty(Text) && Match(items[i]))
                    {
                        indexes.Add(i);
                        points.Add(i / (double) totalFilteredEntries);
                    }
                }
                HighlightIndex = indexes;
                HighlightPoints = points;
                TotalEntries = totalFilteredEntries;
                if (SelectedIndex > HighlightIndex.Count - 1)
                {
                    SelectedIndex = HighlightIndex.Count - 1;
                }
            });
        }

        public string Text
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public Color Color
        {
            get { return GetProperty<Color>(); }
            set
            {
                if (SetProperty(value))
                {
                    Brush = new SolidColorBrush(Color);
                }
            }
        }

        public Brush Brush
        {
            get { return GetProperty<Brush>(); }
            private set { SetProperty(value); }
        }

        public ObservableCollection<int> HighlightIndex
        {
            get { return GetProperty<ObservableCollection<int>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<double> HighlightPoints
        {
            get { return GetProperty<ObservableCollection<double>>(); }
            set { SetProperty(value); }
        }

        public int? SelectedIndex
        {
            get { return GetProperty<int?>(); }
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(SelectedPoint));
                    this.RaisePropertyChanged(nameof(SelectedIndexForDisplay));
                }
            }
        }

        public int? SelectedIndexForDisplay => SelectedIndex + 1;

        public int SelectedPoint
        {
            get
            {
                return (HighlightIndex?.Any() ?? false)
                       && SelectedIndex.HasValue
                       && SelectedIndex.Value != -1
                       && SelectedIndex < HighlightIndex.Count
                    ? HighlightIndex[SelectedIndex.Value]
                    : 0;
            }
        }

        public ICommand GoToNextCommand => ReactiveCommand.CreateFromTask(x => CallGoToNext());
        public ICommand GoToPreviousCommand => ReactiveCommand.CreateFromTask(x => CallGoToPrevious());

        public int TotalEntries
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        private Task CallGoToPrevious()
        {
            if (!SelectedIndex.HasValue && HighlightIndex?.Count > 0)
            {
                SelectedIndex = 0;
            }
            else if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
            else
            {
                SelectedIndex = HighlightIndex?.Count - 1;
            }
            _parent.SetSelectedItem(SelectedPoint);
            return Task.CompletedTask;
        }

        private Task CallGoToNext()
        {
            if (!SelectedIndex.HasValue && HighlightIndex?.Count > 0)
            {
                SelectedIndex = 0;
            }
            else if (SelectedIndex < HighlightIndex?.Count - 1)
            {
                SelectedIndex++;
            }
            else
            {
                SelectedIndex = 0;
            }
            _parent.SetSelectedItem(SelectedPoint);
            return Task.CompletedTask;
        }

        public bool Match(TItem entry)
        {
            return _fieldsAccessors.Any(x => Match(x(entry)));
        }

        public bool Match(string content)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return true;
            }
            return content?.Contains(Text, StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}