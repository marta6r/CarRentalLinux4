using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    [Table("feature_value")]
    public class FeatureValue
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("value")]
        [Required(ErrorMessage = "Значение характеристики обязательно")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Display(Name = "Значение")]
        public string Value { get; set; } = string.Empty;
        
        [Column("feature_id")]
        public int FeatureId { get; set; }
        
        // Навигационные свойства
        public Feature Feature { get; set; }
        public ICollection<CarFeature> CarFeatures { get; set; }
    }
}