using WebAPI.Repositories;

namespace WebAPI.Factories
{
    public interface IRepositoryFactory
    {
        IUtilizadorRepository CreateUtilizadorRepository();
        IProdutoRepository CreateProdutoRepository();
        ICategoriaRepository CreateCategoriaRepository();
        ILojaRepository CreateLojaRepository();
        ILocalizacaoRepository CreateLocalizacaoRepository();
        IRegistosPrecoRepository CreateRegistosPrecoRepository();
        ITipoAcaoRepository CreateTipoAcaoRepository();
        ITipoUtilizadorRepository CreateTipoUtilizadorRepository();
        IMensagemRepository CreateMensagemRepository();
    }
}
