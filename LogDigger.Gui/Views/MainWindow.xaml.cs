using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dragablz.Dockablz;
using LogDigger.Gui.ViewModels.Core;
using LogDigger.Gui.ViewModels.MainWindow;
using MaterialDesignThemes.Wpf;
using ReactiveUI;

namespace LogDigger.Gui.Views
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IModalHandler
    {
        private BranchResult _branchResult;
        private DialogSession _currentDialogSession;

        public MainWindow()
        {
            AllowDrop = true;
            InitializeComponent();
            MessageBus.Current.Listen<NewTabMessage>().Subscribe(OnNewTab);
            MessageBus.Current.Listen<OpenModalMessage>().Subscribe(OnOpenModal);
        }

        private void OnOpenModal(OpenModalMessage obj)
        {
        }

        private void OnNewTab(NewTabMessage obj)
        {
            if (_branchResult == null || _branchResult.Branch.Parent == null)
            {
                _branchResult = Layout.Branch(xamlTabz, Orientation.Vertical, false, 0.5);
            }

            _branchResult.TabablzControl.AddToSource(obj.NewTab);
            _branchResult.TabablzControl.SelectedItem = obj.NewTab;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                BindingExpression be = xamlTbLogPath.GetBindingExpression(TextBox.TextProperty);
                xamlTbLogPath.SetCurrentValue(TextBox.TextProperty, files[0]);
                be.UpdateSource();
                ((MainWindowVm)DataContext).ResetAndLoadPages();
            }
        }

        public async Task<object> OpenModal(AModalVm modal)
        {
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                var dialogView = new DialogView();
                dialogView.DataContext = modal;
                modal.Handle = DialogHost.Show(dialogView, DialogOpenHandler);
            }));
            var result = await modal.Handle;
            return result;
        }

        private void DialogOpenHandler(object sender, DialogOpenedEventArgs eventargs)
        {
            _currentDialogSession = eventargs.Session;
        }

        public void CloseCurrent()
        {
            _currentDialogSession.Close();
        }
    }
}
