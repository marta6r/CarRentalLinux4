// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace CarRental.Models
// {
//     [Table("customer")]
//     public class Customer
//     {
//         [Column("id")]
//         public int Id { get; set; }

//         [Required(ErrorMessage = "Обязательное поле")]
//         [Display(Name = "ФИО")]
//         [Column("full_name")]
//         public string FullName { get; set; }

//         [Required(ErrorMessage = "Обязательное поле")]
//         [EmailAddress(ErrorMessage = "Некорректный email")]
//         [Column("email")]
//         public string Email { get; set; }

//         [Required(ErrorMessage = "Обязательное поле")]
//         [Display(Name = "Телефон")]
//         [RegularExpression(@"^\+375\d{9}$", ErrorMessage = "Номер должен иметь +375 и 9 цифр")]
//         [StringLength(13, MinimumLength = 13, ErrorMessage = "Длина 13 символов")]
//         [Column("phone")]
//         public string Phone { get; set; }

//         [Required(ErrorMessage = "Обязательное поле")]
//         [Display(Name = "Номер паспорта")]
//         [RegularExpression(@"^[A-Za-z]{2}\d{7}$", ErrorMessage = "Номер должен иметь 2 буквы и 7 цифр")]
//         [StringLength(9, MinimumLength = 9, ErrorMessage = "Длина 9 символов")]
//         [Column("passport_number")]
//         public string PassportNumber { get; set; }

//         [Column("image_path")]
//         [Display(Name = "Фото клиента")]
//         public string? ImagePath { get; set; }

//         [NotMapped]
//         [Display(Name = "Фото клиента")]
//         public IFormFile? ImageFile { get; set; }

//         [Column("role")]
//         public string Role { get; set; } = "user";  // "user" или "manager"
//     }
// }






























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

        [Column("image_path")]
        [Display(Name = "Фото клиента")]
        public string? ImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Фото клиента")]
        public IFormFile? ImageFile { get; set; }

        [Column("role")]
        public string Role { get; set; } = "user";  // "user" или "manager"

        // Поле для пароля (не хранится в БД, используется только при регистрации)
        [NotMapped]
        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        // Хеш пароля (хранится в БД)
        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        // Токен для сброса пароля
        [Column("password_reset_token")]
        public string? PasswordResetToken { get; set; }

        // Время действия токена сброса пароля
        [Column("password_reset_token_expires")]
        public DateTime? PasswordResetTokenExpires { get; set; }

        // Навигационное свойство
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}