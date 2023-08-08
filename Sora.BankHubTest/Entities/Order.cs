using MongoDB.Bson.Serialization.Attributes;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Timing;

namespace Sora.BankHubTest.Entities
{
    //[DisableDateTimeNormalization]
    public class Order : Entity<Guid>
    {
        [BsonElement("orderId")]
        public int OrderId { get; set; }

        [BsonElement("orderAmount")]
        public long OrderAmount { get; set; }

        [BsonElement("orderStatus")]
        public int OrderStatus { get; set; }

        [BsonElement("bank")]
        public BankObject Bank { get; set; }

        [BsonElement("desc")]
        public string Description { get; set; }

        [BsonElement("vietQrUrl")]
        public string VietQrUrl { get; set; }

        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; }

        [BsonElement("updatedDate")]
        public DateTime UpdatedDate { get; set; }

        public Order()
        {
        }

        public Order(Guid id) : base(id)
        {
            OrderStatus = (int)OrderStatusType.PendingPayment;
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

        public string OrderStatusText
        {
            get
            {
                switch (OrderStatus)
                {
                    case (int)OrderStatusType.PaymentSuccess:
                        return "Thanh toán thành công";

                    case (int)OrderStatusType.PaymentFailure:
                        return "Thanh toán thất bại";

                    default:
                        return "Chờ thanh toán";
                }
            }
        }
    }

    public class BankObject
    {
        [BsonElement("bankCode")]
        public string BankCode { get; set; }

        [BsonElement("bankName")]
        public string BankName { get; set; }

        [BsonElement("accountNo")]
        public string AccountNo { get; set; }

        [BsonElement("accountName")]
        public string AccountName { get; set; }
    }

    public enum OrderStatusType
    {
        PendingPayment = 0,
        PaymentSuccess = 1,
        PaymentFailure = 2,
    }
}