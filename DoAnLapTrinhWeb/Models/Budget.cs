using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLapTrinhWeb.Models
{
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        public string? UserId { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

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
    }
}