using MongoDB.Driver;
using Sora.BankHubTest.Entities;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace Sora.BankHubTest.MongoDB
{
    [ConnectionStringName("MongoDB")]
    public class BankHubTestMongoDbContext : AbpMongoDbContext
    {
        public IMongoCollection<Order> Orders => Collection<Order>();
        public IMongoCollection<BankAccount> BankAccounts => Collection<BankAccount>();

        protected override void CreateModel(IMongoModelBuilder modelBuilder)
        {
            base.CreateModel(modelBuilder);

            modelBuilder.Entity<Order>(b =>
            {
                b.CollectionName = "order";
            });

            modelBuilder.Entity<BankAccount>(b =>
            {
                b.CollectionName = "bank_account";
            });
        }
    }
}