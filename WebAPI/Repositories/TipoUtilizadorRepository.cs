using WebAPI.Context;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class TipoUtilizadorRepository : Repository<TipoUtilizador>, ITipoUtilizadorRepository
    {
        public TipoUtilizadorRepository(AppDbContext context) : base(context)
        {
        }
    }
}
