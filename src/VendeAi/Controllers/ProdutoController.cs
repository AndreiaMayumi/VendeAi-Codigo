using VendeAi.Models;
using VendeAi.Repositories;

namespace VendeAi.Controllers;

public class ProdutoController
{
    private readonly ProdutoRepository produtoRepository;

    public ProdutoController(ProdutoRepository produtoRepository)
    {
        this.produtoRepository = produtoRepository;
    }

    public string Cadastrar(Produto produto)
    {
        // ============================
        // VALIDAÇÕES BÁSICAS
        // ============================

        if (string.IsNullOrWhiteSpace(produto.Nome))
        {
            return "O nome do produto é obrigatório.";
        }

        if (string.IsNullOrWhiteSpace(produto.CodigoSku))
        {
            return "O código SKU é obrigatório.";
        }

        if (string.IsNullOrWhiteSpace(produto.Categoria))
        {
            return "A categoria é obrigatória.";
        }

        // ============================
        // VALIDAÇÃO DAS MODALIDADES
        // ============================

        if (produto.Modalidades == null || produto.Modalidades.Count == 0)
        {
            return "O produto deve possuir pelo menos uma modalidade de venda.";
        }

        foreach (ProdutoModalidade modalidade in produto.Modalidades)
        {
            if (modalidade.QuantidadeMinimaVenda < 1)
            {
                return $"A quantidade mínima da modalidade " +
                       $"{modalidade.Modalidade} deve ser maior ou igual a 1.";
            }

            if (modalidade.PrecoVenda <= 0)
            {
                return $"O preço de venda da modalidade " +
                       $"{modalidade.Modalidade} deve ser maior que zero.";
            }

            if (modalidade.PrecoCusto.HasValue &&
                modalidade.PrecoCusto.Value < 0)
            {
                return $"O preço de custo da modalidade " +
                       $"{modalidade.Modalidade} não pode ser negativo.";
            }
        }

        // ============================
        // MODALIDADE DUPLICADA
        // ============================

        bool possuiModalidadeDuplicada =
            produto.Modalidades
                .GroupBy(modalidade => modalidade.Modalidade)
                .Any(grupo => grupo.Count() > 1);

        if (possuiModalidadeDuplicada)
        {
            return "Não é permitido cadastrar a mesma modalidade mais de uma vez.";
        }

        // ============================
        // SKU DUPLICADO
        // ============================

        if (produtoRepository.ExisteSku(produto.CodigoSku))
        {
            return "Já existe um produto cadastrado com este SKU.";
        }

        // ============================
        // CADASTRO
        // ============================

        produto.Status = StatusProduto.Ativo;

        produtoRepository.Adicionar(produto);

        return "Produto cadastrado com sucesso.";
    }
}