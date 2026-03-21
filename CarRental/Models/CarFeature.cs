using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    [Table("car_feature")]
    public class CarFeature
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("car_id")]
        public int CarId { get; set; }
        
        [Column("feature_id")]
        public int FeatureId { get; set; }
        
        [Column("feature_value_id")]
        public int FeatureValueId { get; set; }
        
        // Навигационные свойства
        public Car Car { get; set; }
        public Feature Feature { get; set; }
        public FeatureValue FeatureValue { get; set; }
    }
}