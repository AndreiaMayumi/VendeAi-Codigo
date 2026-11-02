
using System;
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;
using VendeAi.Repositories;

namespace VendeAi.Service;

public class PedidoService
{
    private readonly PedidoRepository pedidoRepository;

    public PedidoService(PedidoRepository pedidoRepository)
    {
        this.pedidoRepository = pedidoRepository;
    }

    // CADASTRAR PEDIDO
    public void Cadastrar(Pedido pedido)
    {
        ValidarPedido(pedido);
        pedidoRepository.Adicionar(pedido);
    }

    // LISTAR PEDIDOS
    public List<Pedido> ListarTodos()
    {
        return pedidoRepository.ListarTodos();
    }

    // BUSCAR PEDIDO PELO CÓDIGO
    public Pedido? BuscarPorCodigo(int codigo)
    {
        return pedidoRepository.BuscarPorCodigo(codigo);
    }

    // ATUALIZAR PEDIDO
    public void Atualizar(int codigo, Pedido pedidoAtualizado)
    {
        if (pedidoRepository.BuscarPorCodigo(codigo) == null)
        {
            throw new InvalidOperationException(
                "Pedido não encontrado."
            );
        }

        ValidarPedido(pedidoAtualizado);

        bool atualizado = pedidoRepository.Atualizar(
            codigo,
            pedidoAtualizado
        );

        if (!atualizado)
        {
            throw new InvalidOperationException(
                "Não foi possível atualizar o pedido."
            );
        }
    }

    // EXCLUIR PEDIDO
    public void Excluir(int codigo)
    {
        bool excluido = pedidoRepository.Excluir(codigo);

        if (!excluido)
        {
            throw new InvalidOperationException(
                "Pedido não encontrado."
            );
        }
    }

    // VALIDAÇÕES DO PEDIDO
    private void ValidarPedido(Pedido pedido)
    {
        if (pedido == null)
        {
            throw new ArgumentException(
                "Informe os dados do pedido."
            );
        }

        if (pedido.Cliente == null)
        {
            throw new ArgumentException(
                "Selecione um cliente para o pedido."
            );
        }

        if (pedido.Itens == null || pedido.Itens.Count == 0)
        {
            throw new ArgumentException(
                "Adicione pelo menos um produto ao pedido."
            );
        }

        if (!Enum.IsDefined(pedido.Status))
        {
            throw new ArgumentException(
                "Status do pedido inválido."
            );
        }

        if (pedido.FormaPagamento.HasValue &&
            !Enum.IsDefined(pedido.FormaPagamento.Value))
        {
            throw new ArgumentException(
                "Forma de pagamento inválida."
            );
        }

        if (pedido.DataPrevistaEntrega.HasValue &&
            pedido.DataPrevistaEntrega.Value.Date <
            pedido.DataPedido.Date)
        {
            throw new ArgumentException(
                "A data prevista de entrega não pode ser anterior à data do pedido."
            );
        }

        if (pedido.Observacoes?.Length > 500)
        {
            throw new ArgumentException(
                "As observações devem ter no máximo 500 caracteres."
            );
        }

        foreach (ItemPedido item in pedido.Itens)
        {
            ValidarItem(item);
        }

        if (pedido.Desconto < 0)
        {
            throw new ArgumentException(
                "O desconto não pode ser negativo."
            );
        }

        if (pedido.Desconto > pedido.Subtotal)
        {
            throw new ArgumentException(
                "O desconto não pode ser maior que o subtotal do pedido."
            );
        }
    }

    // VALIDAÇÕES DOS ITENS
    private void ValidarItem(ItemPedido item)
    {
        if (item == null)
        {
            throw new ArgumentException(
                "O pedido contém um item inválido."
            );
        }

        if (item.Produto == null)
        {
            throw new ArgumentException(
                "Selecione um produto válido."
            );
        }

        if (item.Produto.Status != StatusProduto.Ativo)
        {
            throw new ArgumentException(
                $"O produto '{item.Produto.Nome}' está inativo."
            );
        }

        if (item.ProdutoModalidade == null)
        {
            throw new ArgumentException(
                $"Selecione a modalidade do produto '{item.Produto.Nome}'."
            );
        }

        bool modalidadePertenceAoProduto =
            item.Produto.Modalidades.Any(
                modalidade =>
                    modalidade.Id == item.ProdutoModalidade.Id &&
                    modalidade.Modalidade ==
                        item.ProdutoModalidade.Modalidade
            );

        if (!modalidadePertenceAoProduto)
        {
            throw new ArgumentException(
                $"A modalidade selecionada não pertence ao produto '{item.Produto.Nome}'."
            );
        }

        if (item.Quantidade <= 0)
        {
            throw new ArgumentException(
                $"A quantidade do produto '{item.Produto.Nome}' deve ser maior que zero."
            );
        }

        int quantidadeMinima =
            item.ProdutoModalidade.QuantidadeMinimaVenda;

        if (item.Quantidade < quantidadeMinima)
        {
            throw new ArgumentException(
                $"O produto '{item.Produto.Nome}' exige quantidade mínima de {quantidadeMinima} unidade(s) na modalidade {item.ProdutoModalidade.Modalidade}."
            );
        }

        if (item.PrecoUnitario < 0)
        {
            throw new ArgumentException(
                $"O preço do produto '{item.Produto.Nome}' não pode ser negativo."
            );
        }
    }
}
