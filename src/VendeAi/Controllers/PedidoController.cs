
using System.Collections.Generic;
using VendeAi.Models;
using VendeAi.Service;

namespace VendeAi.Controllers;

public class PedidoController
{
    private readonly PedidoService pedidoService;

    public PedidoController(PedidoService pedidoService)
    {
        this.pedidoService = pedidoService;
    }

    // CADASTRAR PEDIDO
    public void CadastrarPedido(Pedido pedido)
    {
        pedidoService.Cadastrar(pedido);
    }

    // CONSULTAR TODOS OS PEDIDOS
    public List<Pedido> ListarPedidos()
    {
        return pedidoService.ListarTodos();
    }

    // CONSULTAR PEDIDO PELO CÓDIGO
    public Pedido? BuscarPedidoPorCodigo(int codigo)
    {
        return pedidoService.BuscarPorCodigo(codigo);
    }

    // ATUALIZAR PEDIDO
    public void AtualizarPedido(
        int codigo,
        Pedido pedidoAtualizado)
    {
        pedidoService.Atualizar(
            codigo,
            pedidoAtualizado
        );
    }

    // EXCLUIR PEDIDO
    public void ExcluirPedido(int codigo)
    {
        pedidoService.Excluir(codigo);
    }
}
