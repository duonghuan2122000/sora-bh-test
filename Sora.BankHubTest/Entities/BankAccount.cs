using MongoDB.Bson.Serialization.Attributes;
using Volo.Abp.Domain.Entities;

namespace Sora.BankHubTest.Entities
{
    public class BankAccount : Entity<Guid>
    {
        [BsonElement("bankCode")]
        public string BankCode { get; set; }

        [BsonElement("bankName")]
        public string BankName { get; set; }

        [BsonElement("accountNo")]
        public string AccountNo { get; set; }

        public BankAccount()
        {
        }

        public BankAccount(Guid id) : base(id)
        {
        }
    }
}