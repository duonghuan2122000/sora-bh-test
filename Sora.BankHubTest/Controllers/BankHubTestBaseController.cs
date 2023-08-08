using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Linq;

namespace Sora.BankHubTest.Controllers
{
    public class BankHubTestBaseController : AbpController
    {
        protected IAsyncQueryableExecuter AsyncExecuter => LazyServiceProvider.LazyGetRequiredService<IAsyncQueryableExecuter>();
    }
}