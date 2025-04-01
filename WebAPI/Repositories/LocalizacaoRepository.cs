using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class LocalizacaoRepository : Repository<Localizacao>, ILocalizacaoRepository
    {
        public LocalizacaoRepository(AppDbContext context) : base(context)
        {
        }
    }
}
