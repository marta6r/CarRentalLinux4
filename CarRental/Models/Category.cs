using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    [Table("category")]
    public class Category
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("name")]
        [Required(ErrorMessage = "Название категории обязательно")]
        [StringLength(50, ErrorMessage = "Максимум 50 символов")]
        [Display(Name = "Категория")]
        public string Name { get; set; }
        
        // Навигационное свойство (связь с автомобилями)
        public ICollection<Car> Cars { get; set; }


        public ICollection<CategoryFeature> CategoryFeatures { get; set; }
    }
}