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
		[DateGreaterThan("StartDate", ErrorMessage = "Ngày bắt đầu phải luôn bé hơn ngày kết thúc")]
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

		public class DateGreaterThanAttribute : ValidationAttribute
		{
			private readonly string _comparisonProperty;

			public DateGreaterThanAttribute(string comparisonProperty)
			{
				_comparisonProperty = comparisonProperty;
			}

			protected override ValidationResult IsValid(object value, ValidationContext validationContext)
			{
				var propertyInfo = validationContext.ObjectType.GetProperty(_comparisonProperty);
				if (propertyInfo == null)
				{
					return new ValidationResult($"Property {_comparisonProperty} not found.");
				}

				var comparisonValue = (DateTime)propertyInfo.GetValue(validationContext.ObjectInstance);
				var currentValue = (DateTime)value;

				if (currentValue <= comparisonValue)
				{
					return new ValidationResult(ErrorMessage);
				}

				return ValidationResult.Success;
			}
		}

	}
}