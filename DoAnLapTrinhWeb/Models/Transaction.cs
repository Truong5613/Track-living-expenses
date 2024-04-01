using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLapTrinhWeb.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Ko bỏ tiền vô thì truyền vô làm gì má")]
        [Range(1, int.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public int Amount { get; set; }

        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
        [Required(ErrorMessage = "Xin hãy chọn một danh mục.")]
        [Range(1, int.MaxValue, ErrorMessage = "Xin hãy chọn một danh mục")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [NotMapped]
        public string? CategoryNameWithIcon
        {
            get
            {
                return Category == null ? "" : Category.Icon + " " + Category.Name;
            }
        }
        [NotMapped]
        public string? FormatAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? " - " : " + ") + Amount.ToString("C0");
            }
        }
    }
}
