using System.Threading.Tasks;

namespace LogDigger.Gui.ViewModels.Core
{
    public class AModalVm : AViewModel
    {
        public Task<object> Handle { get; set; }
    }
}