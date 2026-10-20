
using VendeAi.Models;
using VendeAi.Services;
using VendeAi.Repositories;

namespace VendeAi.Controllers;

public class ProdutoController
{
    private readonly ProdutoService produtoService;

    public ProdutoController(ProdutoRepository produtoRepository)
    {
        produtoService = new ProdutoService(produtoRepository);
    }

    public string Cadastrar(Produto produto)
    {
        return produtoService.Cadastrar(produto);
    }
}
