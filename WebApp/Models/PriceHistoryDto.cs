namespace WebApp.Models
{
    public class PriceHistoryDto
    {
        public int LojaId { get; set; }
        public string LojaNome { get; set; } = string.Empty;
        public List<PricePointDto> Prices { get; set; } = new();
    }

    public class PricePointDto
    {
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
    }
}
