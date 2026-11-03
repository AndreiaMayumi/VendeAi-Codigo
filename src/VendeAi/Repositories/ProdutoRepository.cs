
using System;
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;

namespace VendeAi.Repositories;

public class ProdutoRepository
{
    private readonly List<Produto> produtos = new();

    public void Adicionar(Produto produto)
    {
        produtos.Add(produto);
    }

    public bool ExisteSku(string codigoSku)
    {
        return produtos.Any(produto =>
            produto.CodigoSku.Equals(
                codigoSku,
                StringComparison.OrdinalIgnoreCase
            )
            ||
            produto.Variacoes.Any(variacao =>
                !string.IsNullOrWhiteSpace(variacao.CodigoSku)
                &&
                variacao.CodigoSku.Equals(
                    codigoSku,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        );
    }

    public List<Produto> ListarTodos()
    {
        return produtos;
    }

    public Produto? BuscarPorSku(string codigoSku)
    {
        return produtos.FirstOrDefault(produto =>
            produto.CodigoSku.Equals(
                codigoSku,
                StringComparison.OrdinalIgnoreCase
            )
        );
    }

    public void AdicionarVariacao(
        string codigoSkuProduto,
        ProdutoVariacao variacao)
    {
        Produto? produto = BuscarPorSku(codigoSkuProduto);

        if (produto == null)
        {
            throw new InvalidOperationException(
                "Produto não encontrado."
            );
        }

        if (!string.IsNullOrWhiteSpace(variacao.CodigoSku)
            && ExisteSku(variacao.CodigoSku))
        {
            throw new InvalidOperationException(
                "Já existe um produto ou variação com este SKU."
            );
        }

        variacao.Id = produto.Variacoes.Count == 0
            ? 1
            : produto.Variacoes.Max(v => v.Id) + 1;

        produto.Variacoes.Add(variacao);
    }

    public void Atualizar(Produto produtoAtualizado)
    {
        Produto? produto = BuscarPorSku(
            produtoAtualizado.CodigoSku
        );

        if (produto == null)
        {
            throw new InvalidOperationException(
                "Produto não encontrado."
            );
        }

        produto.Nome = produtoAtualizado.Nome;
        produto.Descricao = produtoAtualizado.Descricao;
        produto.Categoria = produtoAtualizado.Categoria;
        produto.Marca = produtoAtualizado.Marca;
        produto.Imagem = produtoAtualizado.Imagem;
        produto.Status = produtoAtualizado.Status;
    }

    public void AlterarStatus(
        string codigoSku,
        StatusProduto status)
    {
        Produto? produto = BuscarPorSku(codigoSku);

        if (produto == null)
        {
            throw new InvalidOperationException(
                "Produto não encontrado."
            );
        }

        produto.Status = status;
    }
}
