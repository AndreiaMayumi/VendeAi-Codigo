using VendeAi.Repositories;

namespace VendeAi;

public static class AppContexto
{
    public static ClienteRepository Clientes { get; } = new();

    public static ProdutoRepository Produtos { get; } = new();

    public static PedidoRepository Pedidos { get; } = new();
}