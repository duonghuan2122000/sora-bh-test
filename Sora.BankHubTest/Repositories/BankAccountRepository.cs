using Sora.BankHubTest.Entities;
using Sora.BankHubTest.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace Sora.BankHubTest.Repositories
{
    public class BankAccountRepository : MongoDbRepository<BankHubTestMongoDbContext, BankAccount, Guid>, IBankAccountRepository
    {
        public BankAccountRepository(IMongoDbContextProvider<BankHubTestMongoDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}