using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLapTrinhWeb.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int Amount { get; set; }

        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
       
        public int CategoryId { get; set; }
        
        public Category? Category { get; set; }

        public string? UserID { get; set; }

        [NotMapped]
        public string? CategoryNameWithIcon {
            get
            {
                return Category==null? "" : Category.Icon + " " +Category.Name;
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
