namespace WebAPI.DTOs
{
    public class PriceHistoryDto
    {
        public string LojaNome { get; set; }
        public List<PricePoint> Prices { get; set; }
    }

    public class PricePoint
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
