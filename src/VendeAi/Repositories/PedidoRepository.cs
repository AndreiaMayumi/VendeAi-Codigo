
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;

namespace VendeAi.Repositories;

public class PedidoRepository
{
    private readonly List<Pedido> pedidos = new();

    private int proximoCodigo = 1;

    // CADASTRAR PEDIDO
    public void Adicionar(Pedido pedido)
    {
        pedido.Codigo = proximoCodigo++;
        pedidos.Add(pedido);
    }

    // CONSULTAR TODOS OS PEDIDOS
    public List<Pedido> ListarTodos()
    {
        return new List<Pedido>(pedidos);
    }

    // BUSCAR PEDIDO PELO CÓDIGO
    public Pedido? BuscarPorCodigo(int codigo)
    {
        return pedidos.FirstOrDefault(
            pedido => pedido.Codigo == codigo
        );
    }

    // ATUALIZAR PEDIDO
    public bool Atualizar(int codigo, Pedido pedidoAtualizado)
    {
        Pedido? pedido = BuscarPorCodigo(codigo);

        if (pedido == null)
            return false;

        pedido.DataPedido = pedidoAtualizado.DataPedido;
        pedido.DataPrevistaEntrega =
            pedidoAtualizado.DataPrevistaEntrega;

        pedido.Cliente = pedidoAtualizado.Cliente;
        pedido.Itens = pedidoAtualizado.Itens;
        pedido.Status = pedidoAtualizado.Status;
        pedido.FormaPagamento = pedidoAtualizado.FormaPagamento;
        pedido.Observacoes = pedidoAtualizado.Observacoes;
        pedido.Desconto = pedidoAtualizado.Desconto;

        return true;
    }

    // EXCLUIR PEDIDO
    public bool Excluir(int codigo)
    {
        Pedido? pedido = BuscarPorCodigo(codigo);

        if (pedido == null)
            return false;

        return pedidos.Remove(pedido);
    }
}
