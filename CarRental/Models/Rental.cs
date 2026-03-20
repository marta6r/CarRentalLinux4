using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    [Table("rental")]
    public class Rental
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("car_id")]
        public int CarId { get; set; }
        
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Display(Name = "Дата начала аренды")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Дата начала должна быть в будущем")]
        [Column("rent_date")]
        public DateTime RentDate { get; set; }
        
        [Display(Name = "Дата возврата")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Дата возврата должна быть в будущем")]
        [DateAfter("RentDate", ErrorMessage = "Дата возврата должна быть после даты начала")]
        [Column("return_date")]
        public DateTime ReturnDate { get; set; }
        
        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Завершена")]

        // Навигационные свойства
        public Car? Car { get; set; }
        public Customer? Customer { get; set; }

        public class FutureDateAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                return value is DateTime date && date >= DateTime.Today;
            }
        }

        public class DateAfterAttribute : ValidationAttribute
        {
            private readonly string _comparisonProperty;

            public DateAfterAttribute(string comparisonProperty)
            {
                _comparisonProperty = comparisonProperty;
            }
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

                if (property == null)
                    return new ValidationResult($"Unknown property: {_comparisonProperty}");

                var comparisonValue = (DateTime)property.GetValue(validationContext.ObjectInstance);

                if (value is DateTime date && date <= comparisonValue)
                    return new ValidationResult(ErrorMessage ?? $"Дата должна быть после {_comparisonProperty}");

                return ValidationResult.Success;
            }
        }
    }
}