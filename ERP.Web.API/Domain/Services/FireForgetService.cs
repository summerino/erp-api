using ERP.Common;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces;
using ERP.Web.API.Domain.Interfaces.Accounting;
using ERP.Web.API.Domain.Services.Accounting;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services
{
    public class FireForgetService : IFireForgetService
    {
        private readonly IServiceScopeFactory _ssf;
        private readonly ILogger<FireForgetService> _logger;

        public FireForgetService(IServiceScopeFactory ssf, ILogger<FireForgetService> logger)
        {
            _ssf = ssf;
            _logger = logger;
        }

        public void Execute(Func<IJournalService, SaveResult> DoWork)
        {
            var t = new Thread(new ThreadStart(() =>
            {
                try
                {
                    using var scope = _ssf.CreateScope();

                    _logger.LogInformation("Inside IFireForgetService Execute.");
                    //scope.ServiceProvider.GetService<TenantContext>();

                    //// Configure tenant context db
                    //var contextOptions = new DbContextOptionsBuilder<TenantContext>()
                    //    .UseSqlServer($"Server={tenant.ServerName};Database={tenant.DatabaseName};User Id={tenant.ServerUserId};Password={tenant.ServerPassword}")
                    //    .Options;

                    //var tenantCtx = new TenantContext(contextOptions, _catalogCtx, _claim);

                    var repo = scope.ServiceProvider.GetRequiredService<IJournalService>();
                    DoWork(repo);
                }
                catch (Exception)
                {
                    throw;
                }
            }));
            t.Start();

            //Task.Run(() =>
            //{
            //    try
            //    {
            //        using var scope = _ssf.CreateScope();
            //        var repo = scope.ServiceProvider.GetRequiredService<IJournalService>();
            //        DoWork(repo);
            //    }
            //    catch (Exception)
            //    {

            //        throw;
            //    }
            //});
        }

        public void Execute(Func<JournalService, Task> DoWork)
        {
            throw new NotImplementedException();
        }
    }
}
