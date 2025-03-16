namespace ES2_TP_ComparadorPrecos_WebApp.Models
{
    public class Localizacao
    {
        public int LocalizacaoId { get; set; }
        public string? Cidade { get; set; }
        public string? Pais { get; set; }
        public string? CodigoPostal { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? GoogleMapsUrl { get; set; }
    }
}