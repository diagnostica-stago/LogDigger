namespace LogDigger.Business.Models
{
    public interface IModuleClassifier
    {
        string GetModuleForFile(string fileName);
    }
}