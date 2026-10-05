namespace WebApplication5.Models
{
    public class DiscountViewModel
    {
        public decimal Price { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public string Message { get; set; }
    }
}