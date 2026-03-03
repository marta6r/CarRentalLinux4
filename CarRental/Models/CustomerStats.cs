// Models/CustomerStats.cs
namespace CarRental.Models
{
    public class CustomerStats
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public decimal TotalSpent { get; set; }
        public int RentalsCount { get; set; }
    }
}