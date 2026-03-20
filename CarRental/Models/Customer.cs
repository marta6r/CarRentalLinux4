using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    [Table("customer")]
    public class Customer
    {
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "ФИО")]
        [Column("full_name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Column("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Телефон")]
        [RegularExpression(@"^\+375\d{9}$", ErrorMessage = "Номер должен иметь +375 и 9 цифр")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "Длина 13 символов")]
        [Column("phone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Номер паспорта")]
        [RegularExpression(@"^[A-Za-z]{2}\d{7}$", ErrorMessage = "Номер должен иметь 2 буквы и 7 цифр")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Длина 9 символов")]
        [Column("passport_number")]
        public string PassportNumber { get; set; }
    }
}