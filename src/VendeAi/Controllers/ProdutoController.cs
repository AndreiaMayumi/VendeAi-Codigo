using System.Collections.Generic;
using VendeAi.Models;
using VendeAi.Services;
using VendeAi.Repositories;
namespace VendeAi.Controllers;
public class ProdutoController
{
    private readonly ProdutoService produtoService;
    public ProdutoController(ProdutoRepository produtoRepository, PedidoRepository? pedidoRepository = null) => produtoService = new ProdutoService(produtoRepository, pedidoRepository);
    public string Cadastrar(Produto produto) => produtoService.Cadastrar(produto);
    public List<Produto> ListarProdutos() => produtoService.ListarProdutos();
    public Produto? BuscarProdutoPorSku(string codigoSku) => produtoService.BuscarProdutoPorSku(codigoSku);
    public string AdicionarVariacao(string codigoSkuProduto, ProdutoVariacao variacao) => produtoService.AdicionarVariacao(codigoSkuProduto, variacao);
    public string AtualizarProduto(Produto produto) => produtoService.AtualizarProduto(produto);
    public string AlterarStatusProduto(string codigoSku, StatusProduto status) => produtoService.AlterarStatusProduto(codigoSku, status);
    public string AlterarStatusVariacao(string codigoSkuProduto, int idVariacao, StatusProduto status) => produtoService.AlterarStatusVariacao(codigoSkuProduto, idVariacao, status);
    public string AlterarStatusModalidade(string sku, int id, Modalidade modalidade, StatusProduto status) => produtoService.AlterarStatusModalidade(sku, id, modalidade, status);
    public string ExcluirModalidade(string sku, int id, Modalidade modalidade) => produtoService.ExcluirModalidade(sku, id, modalidade);
    public string ExcluirVariacao(string codigoSkuProduto, int idVariacao) => produtoService.ExcluirVariacao(codigoSkuProduto, idVariacao);
    public string AdicionarModalidade(string sku, int id, ProdutoModalidade modalidade) => produtoService.AdicionarModalidade(sku, id, modalidade);
    public string EditarVariacao(string sku, int id, Modalidade modalidade, string cor, string tamanho, int quantidade, decimal preco, decimal? custo) => produtoService.EditarVariacao(sku, id, modalidade, cor, tamanho, quantidade, preco, custo);
}
