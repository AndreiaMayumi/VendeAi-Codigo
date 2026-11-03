
using System;
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;
using VendeAi.Repositories;

namespace VendeAi.Services;

public class ProdutoService
{
    private readonly ProdutoRepository produtoRepository;

    public ProdutoService(ProdutoRepository produtoRepository)
    {
        this.produtoRepository = produtoRepository;
    }

    // =====================================
    // CADASTRAR PRODUTO
    // =====================================

    public string Cadastrar(Produto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Nome))
            return "O nome do produto é obrigatório.";

        if (string.IsNullOrWhiteSpace(produto.CodigoSku))
            return "O código SKU é obrigatório.";

        if (!Enum.IsDefined(produto.Categoria))
            return "A categoria informada é inválida.";

        if (produtoRepository.ExisteSku(produto.CodigoSku))
            return "Já existe um produto ou variação com este SKU.";

        // Valida as modalidades cadastradas diretamente
        // no produto, preservando o cadastro atual.
        string? erroModalidades =
            ValidarModalidades(produto.Modalidades);

        if (erroModalidades != null)
            return erroModalidades;

        // Valida as variações vinculadas ao produto.
        if (produto.Variacoes != null)
        {
            HashSet<string> skus = new(
                StringComparer.OrdinalIgnoreCase
            );

            skus.Add(produto.CodigoSku.Trim());

            foreach (ProdutoVariacao variacao in produto.Variacoes)
            {
                if (!string.IsNullOrWhiteSpace(variacao.CodigoSku))
                {
                    string sku = variacao.CodigoSku.Trim();

                    if (!skus.Add(sku) ||
                        produtoRepository.ExisteSku(sku))
                    {
                        return $"O SKU '{sku}' já está em uso.";
                    }
                }

                string? erroVariacao =
                    ValidarVariacao(variacao);

                if (erroVariacao != null)
                    return erroVariacao;
            }

            int proximoId = 1;

            foreach (ProdutoVariacao variacao in produto.Variacoes)
            {
                variacao.Id = proximoId++;
            }
        }

        // Deve existir pelo menos uma modalidade de venda,
        // no produto original ou em alguma variação.
        bool possuiModalidadeOriginal =
            produto.Modalidades != null &&
            produto.Modalidades.Count > 0;

        bool possuiModalidadeVariacao =
            produto.Variacoes != null &&
            produto.Variacoes.Any(v =>
                v.Modalidades != null &&
                v.Modalidades.Count > 0
            );

        if (!possuiModalidadeOriginal &&
            !possuiModalidadeVariacao)
        {
            return "O produto deve possuir pelo menos uma modalidade de venda.";
        }

        produto.Status = StatusProduto.Ativo;

        produtoRepository.Adicionar(produto);

        return "Produto cadastrado com sucesso.";
    }

    // =====================================
    // VALIDAR MODALIDADES
    // =====================================

    private string? ValidarModalidades(
        List<ProdutoModalidade>? modalidades)
    {
        if (modalidades == null)
            return null;

        foreach (ProdutoModalidade modalidade in modalidades)
        {
            if (!Enum.IsDefined(modalidade.Modalidade))
                return "A modalidade de venda é inválida.";

            if (modalidade.QuantidadeMinimaVenda < 1)
            {
                return
                    $"A quantidade mínima da modalidade " +
                    $"{modalidade.Modalidade} deve ser maior ou igual a 1.";
            }

            if (modalidade.PrecoVenda <= 0)
            {
                return
                    $"O preço de venda da modalidade " +
                    $"{modalidade.Modalidade} deve ser maior que zero.";
            }

            if (modalidade.PrecoCusto.HasValue &&
                modalidade.PrecoCusto.Value < 0)
            {
                return
                    $"O preço de custo da modalidade " +
                    $"{modalidade.Modalidade} não pode ser negativo.";
            }
        }

        // Impede repetir a mesma modalidade dentro da
        // mesma lista, mas permite a modalidade em
        // variações diferentes.
        bool duplicada = modalidades
            .GroupBy(m => m.Modalidade)
            .Any(grupo => grupo.Count() > 1);

        if (duplicada)
        {
            return "Não é permitido repetir a mesma modalidade dentro da mesma variação.";
        }

        return null;
    }

    // =====================================
    // VALIDAR VARIAÇÃO
    // =====================================

    private string? ValidarVariacao(
        ProdutoVariacao variacao)
    {
        if (variacao.Modalidades == null ||
            variacao.Modalidades.Count == 0)
        {
            return "A variação deve possuir pelo menos uma modalidade de venda.";
        }

        return ValidarModalidades(variacao.Modalidades);
    }

    // =====================================
    // LISTAR PRODUTOS
    // =====================================

    public List<Produto> ListarProdutos()
    {
        return produtoRepository.ListarTodos();
    }

    // =====================================
    // BUSCAR PRODUTO
    // =====================================

    public Produto? BuscarProdutoPorSku(string codigoSku)
    {
        return produtoRepository.BuscarPorSku(codigoSku);
    }

    // =====================================
    // ADICIONAR VARIAÇÃO
    // =====================================

    public string AdicionarVariacao(
        string codigoSkuProduto,
        ProdutoVariacao variacao)
    {
        Produto? produto =
            produtoRepository.BuscarPorSku(codigoSkuProduto);

        if (produto == null)
            return "Produto não encontrado.";

        if (produto.Status == StatusProduto.Inativo)
            return "Não é possível adicionar variações a um produto inativo.";

        string? erro = ValidarVariacao(variacao);

        if (erro != null)
            return erro;

        try
        {
            produtoRepository.AdicionarVariacao(
                codigoSkuProduto,
                variacao
            );

            return "Variação adicionada com sucesso.";
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
    }

    // =====================================
    // ATUALIZAR PRODUTO
    // =====================================

    public string AtualizarProduto(Produto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Nome))
            return "O nome do produto é obrigatório.";

        if (string.IsNullOrWhiteSpace(produto.CodigoSku))
            return "O código SKU é obrigatório.";

        if (!Enum.IsDefined(produto.Categoria))
            return "A categoria informada é inválida.";

        try
        {
            produtoRepository.Atualizar(produto);

            return "Produto atualizado com sucesso.";
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
    }

    // =====================================
    // ALTERAR STATUS
    // =====================================

    public string AlterarStatusProduto(
        string codigoSku,
        StatusProduto status)
    {
        if (!Enum.IsDefined(status))
            return "Status do produto inválido.";

        try
        {
            produtoRepository.AlterarStatus(
                codigoSku,
                status
            );

            return status == StatusProduto.Ativo
                ? "Produto ativado com sucesso."
                : "Produto inativado com sucesso.";
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
    }
}
