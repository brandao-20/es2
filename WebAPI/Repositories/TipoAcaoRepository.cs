using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class TipoAcaoRepository : Repository<TipoAcao>, ITipoAcaoRepository
    {
        public TipoAcaoRepository(AppDbContext context) : base(context)
        {
        }
    }
}
