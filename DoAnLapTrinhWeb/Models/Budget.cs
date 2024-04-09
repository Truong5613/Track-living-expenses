using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLapTrinhWeb.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int TransactionId { get; set; }
        public Transaction? transaction { get; set; }
        
    }
}