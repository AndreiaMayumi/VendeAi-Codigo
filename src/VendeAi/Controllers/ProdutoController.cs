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

        if (produto.QuantidadeMinimaVenda < 1)
        {
            return "A quantidade mínima de venda deve ser maior ou igual a 1.";
        }

        if (produto.PrecoVenda <= 0)
        {
            return "O preço de venda deve ser maior que zero.";
        }

        if (produtoRepository.ExisteSku(produto.CodigoSku))
        {
            return "Já existe um produto cadastrado com este SKU.";
        }

        produto.Status = StatusProduto.Ativo;

        produtoRepository.Adicionar(produto);

        return "Produto cadastrado com sucesso.";
    }
}