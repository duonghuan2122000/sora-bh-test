using Sora.BankHubTest.Entities;
using Volo.Abp.Domain.Repositories;

namespace Sora.BankHubTest.Repositories
{
    public interface IBankAccountRepository : IRepository<BankAccount, Guid>
    {
    }
}