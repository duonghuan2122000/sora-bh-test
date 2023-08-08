using Sora.BankHubTest.Entities;
using Sora.BankHubTest.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace Sora.BankHubTest.Repositories
{
    public class OrderRepository : MongoDbRepository<BankHubTestMongoDbContext, Order, Guid>, IOrderRepository
    {
        public OrderRepository(IMongoDbContextProvider<BankHubTestMongoDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}