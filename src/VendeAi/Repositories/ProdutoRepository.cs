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
        );
    }

    public List<Produto> ListarTodos()
    {
        return produtos;
    }
}