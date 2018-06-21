using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using LogDigger.Business.Models;
using LogDigger.Gui.ViewModels.Core;
using ReactiveUI;

namespace LogDigger.Gui.ViewModels.MainWindow
{
    public class TooBigLogModalVm : AModalVm
    {
        public TooBigLogModalVm(long filesSize, Dictionary<DateTime, List<LogFile>> filesByDates)
            : this(BytesToString(filesSize), filesByDates)
        {
        }

        public TooBigLogModalVm(string filesSize, Dictionary<DateTime, List<LogFile>> filesByDates)
        {
            FilesSize = filesSize;
            FilesByDates = filesByDates;
            AvailableDates = new ObservableCollection<DateTime>(filesByDates.Keys.Where(x => x != DateTime.MinValue).OrderBy(x => x));
            LowerDateTick = MinimumDateTick = AvailableDates.Min(x => x).Ticks;
            UpperDateTick = MaximumDateTick = AvailableDates.Max(x => x).Ticks;
        }

        public ObservableCollection<DateTime> AvailableDates { get; }

        public string FilesSize { get; }

        public string ComputedSize => BytesToString(FilesByDates.Where(x => x.Key > new DateTime((long)LowerDateTick)).Sum(kvp => kvp.Value.Sum(lf => lf.Length)));

        public Dictionary<DateTime, List<LogFile>> FilesByDates { get; }

        public string Message => $"Taille > 100MB ({FilesSize}).{Environment.NewLine}Cela peut prendre plus de temps à charger et de taille en mémoire.{Environment.NewLine}Voulez-vous quand même charger tous les logs ou seulement les plus récents ? (*.log, sans les *.log.#)";

        public double LowerDateTick
        {
            get => GetProperty<double>();
            set
            {
                if (SetProperty(value))
                {
                    this.RaisePropertyChanged(nameof(ComputedSize));
                }
            }
        }

        public double UpperDateTick
        {
            get => GetProperty<double>();
            set => SetProperty(value);
        }

        public double MinimumDateTick
        {
            get => GetProperty<double>();
            set => SetProperty(value);
        }

        public double MaximumDateTick
        {
            get => GetProperty<double>();
            set => SetProperty(value);
        }

        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
            {
                return "0" + suf[0];
            }
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString(CultureInfo.CurrentCulture) + suf[place];
        }
    }
}