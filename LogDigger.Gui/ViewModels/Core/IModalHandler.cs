using System.Threading.Tasks;

namespace LogDigger.Gui.ViewModels.Core
{
    public interface IModalHandler
    {
        Task<object> OpenModal(AModalVm modal);
        void CloseCurrent();
    }
}