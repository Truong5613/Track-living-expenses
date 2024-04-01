using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLapTrinhWeb.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Column(TypeName ="nvarchar(50)")]
        [Required(ErrorMessage ="Bắt buộc phải có tên")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(5)")]
        [Required(ErrorMessage = "Bắt buộc phải có Icon")]
        public string Icon { get; set; } = "";

        [Column(TypeName = "nvarchar(10)")]
        public string Type { get; set; } = "Expense";

        [NotMapped]
        public string? TitleWithIcon
        {
            get
            {
                return this.Icon + this.Name;
            }
        }
        
    }
}
