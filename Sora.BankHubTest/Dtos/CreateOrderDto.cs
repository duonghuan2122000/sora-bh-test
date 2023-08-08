using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sora.BankHubTest.Dtos
{
    public class CreateOrderDto : IValidatableObject
    {
        [Required(ErrorMessage = "Số tiền không được để trống")]
        [DisplayName("Số tiền")]
        public long OrderAmount { get; set; } = 10000;

        [Required(ErrorMessage = "Số tài khoản không được để trống")]
        [DisplayName("Số tài khoản")]
        public Guid BankAccountId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OrderAmount <= 0)
            {
                yield return new ValidationResult("Số tiền phải là một số nguyên dương", new[] { nameof(OrderAmount) });
            }

            if (OrderAmount >= long.MaxValue)
            {
                yield return new ValidationResult("Số tiền vượt quá giới hạn cho phép", new[] { nameof(OrderAmount) });
            }
        }
    }
}