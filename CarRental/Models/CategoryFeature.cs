using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    [Table("category_feature")]
    public class CategoryFeature
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("category_id")]
        public int CategoryId { get; set; }
        
        [Column("feature_id")]
        public int FeatureId { get; set; }
        
        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;
        
        // Навигационные свойства
        public Category Category { get; set; }
        public Feature Feature { get; set; }
    }
}