using System;

namespace LogDigger.Business.Services
{
    public interface IClosingAppHandler
    {
        event EventHandler<EventArgs> ClosingApp;
    }
}