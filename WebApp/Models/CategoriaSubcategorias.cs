namespace WebApp.Models
{
    public class CategoriaSubcategorias
    {
        public int CategoriaId { get; set; }
        public string NomeExibicao { get; set; } = string.Empty;
        public int? ParentId { get; set; }

        public static List<CategoriaSubcategorias> Achatadas(List<Categoria> categorias, int nivel = 0)
        {
            var resultado = new List<CategoriaSubcategorias>();
            foreach (var cat in categorias.OrderBy(c => c.Nome))
            {
                resultado.Add(new CategoriaSubcategorias
                {
                    CategoriaId = cat.CategoriaId,
                    NomeExibicao = new string(' ', nivel * 2) + "└ " + cat.Nome,
                    ParentId = cat.ParentId
                });
                if (cat.SubCategorias?.Any() == true)
                {
                    resultado.AddRange(Achatadas(cat.SubCategorias, nivel + 1));
                }
            }
            return resultado;
        }
    }
}
