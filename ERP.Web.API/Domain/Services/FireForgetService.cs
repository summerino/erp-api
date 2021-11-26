using ERP.Web.API.Domain.Interfaces;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Services.Accounting;

namespace ERP.Web.API.Domain.Services
{
    public class FireForgetService : IFireForgetService
    {
        private readonly IServiceScopeFactory _ssf;
        public FireForgetService(IServiceScopeFactory ssf)
        {
            _ssf = ssf;
        }

        public void Execute(Func<IJournalService, Task> DoWork)
        {
            Task.Run(() =>
            {
                try
                {
                    using var scope = _ssf.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IJournalService>();
                    DoWork(repo);
                }
                catch (Exception)
                {

                    throw;
                }
            });
        }

        public void Execute(Func<JournalService, Task> DoWork)
        {
            throw new NotImplementedException();
        }
    }
}
