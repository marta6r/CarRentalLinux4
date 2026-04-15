namespace CarRental.Models
{
    public class CarDto
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal DailyPrice { get; set; }
        public string ImagePath { get; set; }
        public string CategoryName { get; set; }
    }
}