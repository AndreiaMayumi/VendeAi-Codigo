using System;
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;
namespace VendeAi.Repositories;
public class ProdutoRepository
{
    private readonly List<Produto> produtos = new();
    public void Adicionar(Produto produto) => produtos.Add(produto);
    public bool ExisteSku(string sku) => produtos.Any(p => string.Equals(p.CodigoSku, sku, StringComparison.OrdinalIgnoreCase) || p.Variacoes.Any(v => string.Equals(v.CodigoSku, sku, StringComparison.OrdinalIgnoreCase)));
    public List<Produto> ListarTodos() => produtos;
    public Produto? BuscarPorSku(string sku) => produtos.FirstOrDefault(p => string.Equals(p.CodigoSku, sku, StringComparison.OrdinalIgnoreCase));
    private Produto ObterProduto(string sku) => BuscarPorSku(sku) ?? throw new InvalidOperationException("Produto não encontrado.");
    public ProdutoVariacao ObterVariacao(string sku, int id) => ObterProduto(sku).Variacoes.FirstOrDefault(v => v.Id == id) ?? throw new InvalidOperationException("Variação não encontrada.");
    public void AdicionarVariacao(string codigoSkuProduto, ProdutoVariacao variacao)
    {
        var p = ObterProduto(codigoSkuProduto);
        if (p.Status == StatusProduto.Inativo) throw new InvalidOperationException("Produto inativo.");
        if (!string.IsNullOrWhiteSpace(variacao.CodigoSku) && ExisteSku(variacao.CodigoSku)) throw new InvalidOperationException("Já existe um produto ou variação com este SKU.");
        variacao.Id = p.Variacoes.Count == 0 ? 1 : p.Variacoes.Max(v => v.Id) + 1;
        p.Variacoes.Add(variacao);
    }
    public void Atualizar(Produto produtoAtualizado)
    {
        var p = ObterProduto(produtoAtualizado.CodigoSku);
        p.Nome = produtoAtualizado.Nome; p.Descricao = produtoAtualizado.Descricao;
        p.Categoria = produtoAtualizado.Categoria; p.Marca = produtoAtualizado.Marca; p.Imagem = produtoAtualizado.Imagem;
        if (p.Status != produtoAtualizado.Status) AlterarStatus(p.CodigoSku, produtoAtualizado.Status);
    }
    public void AlterarStatus(string codigoSku, StatusProduto status)
    {
        var p = ObterProduto(codigoSku); p.Status = status;
        foreach (var v in p.Variacoes) { v.Status = status; foreach (var m in v.Modalidades) m.Status = status; }
    }
    public void AlterarStatusVariacao(string codigoSkuProduto, int idVariacao, StatusProduto status)
    {
        var p = ObterProduto(codigoSkuProduto);
        if (status == StatusProduto.Ativo && p.Status == StatusProduto.Inativo) throw new InvalidOperationException("Ative o produto antes de ativar uma variação.");
        ObterVariacao(codigoSkuProduto, idVariacao).Status = status;
    }
    public ProdutoModalidade ObterModalidade(string sku, int id, Modalidade modalidade) => ObterVariacao(sku, id).Modalidades.FirstOrDefault(m => m.Modalidade == modalidade) ?? throw new InvalidOperationException("Modalidade não encontrada.");
    public void AlterarStatusModalidade(string sku, int id, Modalidade modalidade, StatusProduto status)
    {
        var p = ObterProduto(sku);
        var v = ObterVariacao(sku, id);
        if (status == StatusProduto.Ativo && p.Status == StatusProduto.Inativo) throw new InvalidOperationException("Ative o produto primeiro.");
        var m = ObterModalidade(sku, id, modalidade);
        m.Status = status;
        if (status == StatusProduto.Ativo) v.Status = StatusProduto.Ativo;
    }
    public void RemoverModalidade(string sku, int id, Modalidade modalidade)
    {
        var p = ObterProduto(sku);
        var v = ObterVariacao(sku, id);
        v.Modalidades.Remove(ObterModalidade(sku, id, modalidade));
        if (v.Modalidades.Count == 0) p.Variacoes.Remove(v);
    }
    public void RemoverVariacao(string codigoSkuProduto, int idVariacao) => ObterProduto(codigoSkuProduto).Variacoes.Remove(ObterVariacao(codigoSkuProduto, idVariacao));
    public void AdicionarModalidade(string sku, int id, ProdutoModalidade modalidade) => ObterVariacao(sku, id).Modalidades.Add(modalidade);
    public void EditarVariacao(string sku, int id, Modalidade modalidade, string cor, string tamanho, int quantidade, decimal preco, decimal? custo)
    {
        var v = ObterVariacao(sku, id);
        var m = v.Modalidades.FirstOrDefault(x => x.Modalidade == modalidade) ?? throw new InvalidOperationException("Modalidade não encontrada.");
        v.Cor = cor; v.Tamanho = tamanho;
        m.QuantidadeMinimaVenda = quantidade; m.PrecoVenda = preco; m.PrecoCusto = custo;
    }
}
