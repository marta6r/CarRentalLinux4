using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Телефон")]
        [RegularExpression(@"^\+375\d{9}$", ErrorMessage = "Номер должен иметь +375 и 9 цифр")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "Длина 13 символов")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Обязательное поле")]
        [Display(Name = "Номер паспорта")]
        [RegularExpression(@"^[A-Za-z]{2}\d{7}$", ErrorMessage = "Номер должен иметь 2 буквы и 7 цифр")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "Длина 9 символов")]
        public string PassportNumber { get; set; }

    }
}