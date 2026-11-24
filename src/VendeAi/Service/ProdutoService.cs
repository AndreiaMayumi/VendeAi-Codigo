using System;
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;
using VendeAi.Repositories;
namespace VendeAi.Services;
public class ProdutoService
{
    private readonly ProdutoRepository produtoRepository;
    private readonly PedidoRepository pedidoRepository;
    public ProdutoService(ProdutoRepository produtoRepository, PedidoRepository? pedidoRepository = null)
    {
        this.produtoRepository = produtoRepository;
        this.pedidoRepository = pedidoRepository ?? AppContexto.Pedidos;
    }
    private static string? ValidarModalidades(List<ProdutoModalidade>? modalidades)
    {
        if (modalidades == null) return null;
        foreach (var m in modalidades)
        {
            if (!Enum.IsDefined(m.Modalidade)) return "A modalidade de venda é inválida.";
            if (m.QuantidadeMinimaVenda < 1) return "A quantidade mínima deve ser maior ou igual a 1.";
            if (m.PrecoVenda <= 0) return "O preço de venda deve ser maior que zero.";
            if (m.PrecoCusto < 0) return "O preço de custo não pode ser negativo.";
        }
        return modalidades.GroupBy(m => m.Modalidade).Any(g => g.Count() > 1) ? "Não é permitido repetir a mesma modalidade." : null;
    }
    private static string? ValidarVariacao(ProdutoVariacao v) => v.Modalidades == null || v.Modalidades.Count == 0 ? "A variação deve possuir pelo menos uma modalidade de venda." : ValidarModalidades(v.Modalidades);
    private static string? ValidarProduto(Produto p)
    {
        if (string.IsNullOrWhiteSpace(p.Nome)) return "O nome do produto é obrigatório.";
        if (string.IsNullOrWhiteSpace(p.CodigoSku)) return "O código SKU é obrigatório.";
        if (!Enum.IsDefined(p.Categoria)) return "A categoria informada é inválida.";
        return null;
    }
    public string Cadastrar(Produto produto)
    {
        var erro = ValidarProduto(produto) ?? ValidarModalidades(produto.Modalidades);
        if (erro != null) return erro;
        if (produtoRepository.ExisteSku(produto.CodigoSku)) return "Já existe um produto ou variação com este SKU.";
        var skus = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { produto.CodigoSku.Trim() };
        foreach (var v in produto.Variacoes)
        {
            if (!string.IsNullOrWhiteSpace(v.CodigoSku) && (!skus.Add(v.CodigoSku.Trim()) || produtoRepository.ExisteSku(v.CodigoSku.Trim()))) return $"O SKU '{v.CodigoSku}' já está em uso.";
            erro = ValidarVariacao(v); if (erro != null) return erro;
        }
        if (produto.Modalidades.Count == 0 && produto.Variacoes.Count == 0) return "O produto deve possuir pelo menos uma modalidade de venda.";
        int id = 1; foreach (var v in produto.Variacoes) v.Id = id++;
        produto.Status = StatusProduto.Ativo; produtoRepository.Adicionar(produto);
        return "Produto cadastrado com sucesso.";
    }
    public List<Produto> ListarProdutos() => produtoRepository.ListarTodos();
    public Produto? BuscarProdutoPorSku(string codigoSku) => produtoRepository.BuscarPorSku(codigoSku);
    private static string Executar(Action acao, string sucesso)
    {
        try { acao(); return sucesso; } catch (InvalidOperationException ex) { return ex.Message; }
    }
    public string AdicionarVariacao(string codigoSkuProduto, ProdutoVariacao variacao)
    {
        var erro = ValidarVariacao(variacao); if (erro != null) return erro;
        return Executar(() => produtoRepository.AdicionarVariacao(codigoSkuProduto, variacao), "Variação adicionada com sucesso.");
    }
    public string AtualizarProduto(Produto produto)
    {
        var erro = ValidarProduto(produto); if (erro != null) return erro;
        if (!Enum.IsDefined(produto.Status)) return "Status do produto inválido.";
        return Executar(() => produtoRepository.Atualizar(produto), "Produto atualizado com sucesso.");
    }
    public string AlterarStatusProduto(string codigoSku, StatusProduto status) => !Enum.IsDefined(status) ? "Status do produto inválido." : Executar(() => produtoRepository.AlterarStatus(codigoSku, status), status == StatusProduto.Ativo ? "Produto ativado com sucesso." : "Produto inativado com sucesso.");
    public string AlterarStatusVariacao(string codigoSkuProduto, int idVariacao, StatusProduto status) => !Enum.IsDefined(status) ? "Status da variação inválido." : Executar(() => produtoRepository.AlterarStatusVariacao(codigoSkuProduto, idVariacao, status), status == StatusProduto.Ativo ? "Variação ativada com sucesso." : "Variação inativada com sucesso.");
    public string AlterarStatusModalidade(string sku, int id, Modalidade modalidade, StatusProduto status) => !Enum.IsDefined(status) ? "Status inválido." : Executar(() => produtoRepository.AlterarStatusModalidade(sku, id, modalidade, status), status == StatusProduto.Ativo ? "Variação ativada com sucesso." : "Variação inativada com sucesso.");
    public string ExcluirModalidade(string sku, int id, Modalidade modalidade) => Executar(() =>
    {
        var m = produtoRepository.ObterModalidade(sku, id, modalidade);
        if (pedidoRepository.ListarTodos().Any(p => p.Itens.Any(i => ReferenceEquals(i.ProdutoModalidade, m)))) throw new InvalidOperationException("Esta modalidade está vinculada a pedidos. Inative-a em vez de excluir.");
        produtoRepository.RemoverModalidade(sku, id, modalidade);
    }, "Variação excluída com sucesso.");
    public string ExcluirVariacao(string codigoSkuProduto, int idVariacao) => Executar(() =>
    {
        var v = produtoRepository.ObterVariacao(codigoSkuProduto, idVariacao);
        if (pedidoRepository.ListarTodos().Any(p => p.Itens.Any(i => ReferenceEquals(i.ProdutoVariacao, v)))) throw new InvalidOperationException("Esta variação está vinculada a pedidos. Inative-a em vez de excluir.");
        produtoRepository.RemoverVariacao(codigoSkuProduto, idVariacao);
    }, "Variação excluída com sucesso.");
    public string AdicionarModalidade(string sku, int id, ProdutoModalidade modalidade) => Executar(() =>
    {
        var p = BuscarProdutoPorSku(sku) ?? throw new InvalidOperationException("Produto não encontrado.");
        if (p.Status == StatusProduto.Inativo) throw new InvalidOperationException("Produto inativo.");
        var v = produtoRepository.ObterVariacao(sku, id);
        var erro = ValidarModalidades(v.Modalidades.Append(modalidade).ToList());
        if (erro != null) throw new InvalidOperationException(erro);
        produtoRepository.AdicionarModalidade(sku, id, modalidade);
    }, "Modalidade adicionada à variação existente.");
    public string EditarVariacao(string sku, int id, Modalidade modalidade, string cor, string tamanho, int quantidade, decimal preco, decimal? custo) => Executar(() =>
    {
        var p = BuscarProdutoPorSku(sku) ?? throw new InvalidOperationException("Produto não encontrado.");
        var v = produtoRepository.ObterVariacao(sku, id);
        if (string.IsNullOrWhiteSpace(cor) && string.IsNullOrWhiteSpace(tamanho)) throw new InvalidOperationException("Informe pelo menos a cor ou o tamanho.");
        if (p.Variacoes.Any(x => x != v && string.Equals(x.Cor ?? "", cor, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Tamanho ?? "", tamanho, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException("Já existe uma variação com esta cor e tamanho.");
        var erro = ValidarModalidades(new() { new ProdutoModalidade { Modalidade = modalidade, QuantidadeMinimaVenda = quantidade, PrecoVenda = preco, PrecoCusto = custo } });
        if (erro != null) throw new InvalidOperationException(erro);
        produtoRepository.EditarVariacao(sku, id, modalidade, cor, tamanho, quantidade, preco, custo);
    }, "Variação atualizada com sucesso.");
}
