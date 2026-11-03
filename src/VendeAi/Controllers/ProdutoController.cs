
using System.Collections.Generic;
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

    public List<Produto> ListarProdutos()
    {
        return produtoService.ListarProdutos();
    }

    public Produto? BuscarProdutoPorSku(string codigoSku)
    {
        return produtoService.BuscarProdutoPorSku(codigoSku);
    }

    public string AdicionarVariacao(
        string codigoSkuProduto,
        ProdutoVariacao variacao)
    {
        return produtoService.AdicionarVariacao(
            codigoSkuProduto,
            variacao
        );
    }

    public string AtualizarProduto(Produto produto)
    {
        return produtoService.AtualizarProduto(produto);
    }

    public string AlterarStatusProduto(
        string codigoSku,
        StatusProduto status)
    {
        return produtoService.AlterarStatusProduto(
            codigoSku,
            status
        );
    }
}
