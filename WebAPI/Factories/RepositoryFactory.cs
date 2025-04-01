using WebAPI.Context;
using WebAPI.Repositories;

namespace WebAPI.Factories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly AppDbContext _context;

        public RepositoryFactory(AppDbContext context)
        {
            _context = context;
        }

        public IUtilizadorRepository CreateUtilizadorRepository()
        {
            return new UtilizadorRepository(_context);
        }

        public IProdutoRepository CreateProdutoRepository()
        {
            return new ProdutoRepository(_context);
        }

        public ICategoriaRepository CreateCategoriaRepository()
        {
            return new CategoriaRepository(_context);
        }

        public ILojaRepository CreateLojaRepository()
        {
            return new LojaRepository(_context);
        }

        public ILocalizacaoRepository CreateLocalizacaoRepository()
        {
            return new LocalizacaoRepository(_context);
        }

        public IRegistosPrecoRepository CreateRegistosPrecoRepository()
        {
            return new RegistosPrecoRepository(_context);
        }

        public ITipoAcaoRepository CreateTipoAcaoRepository()
        {
            return new TipoAcaoRepository(_context);
        }

        public ITipoUtilizadorRepository CreateTipoUtilizadorRepository()
        {
            return new TipoUtilizadorRepository(_context);
        }

        public IMensagemRepository CreateMensagemRepository()
        {
            return new MensagemRepository(_context);
        }
    }
}
