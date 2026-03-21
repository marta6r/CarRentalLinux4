using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    [Table("feature")]
    public class Feature
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("name")]
        [Required(ErrorMessage = "Название характеристики обязательно")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Display(Name = "Характеристика")]
        public string Name { get; set; } = string.Empty;
        
        // Навигационные свойства
        public ICollection<FeatureValue> FeatureValues { get; set; }
        public ICollection<CarFeature> CarFeatures { get; set; }
    }
}