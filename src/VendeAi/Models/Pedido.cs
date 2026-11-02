
using System;
using System.Collections.Generic;
using System.Linq;

namespace VendeAi.Models;

public class Pedido
{
    public int Codigo { get; set; }

    public DateTime DataPedido { get; set; } = DateTime.Now;

    public DateTime? DataPrevistaEntrega { get; set; }

    public required Cliente Cliente { get; set; }

    public List<ItemPedido> Itens { get; set; } = new();

    public StatusPedido Status { get; set; }
        = StatusPedido.EmAndamento;

    public FormaPagamento? FormaPagamento { get; set; }

    public string Observacoes { get; set; } = string.Empty;

    public decimal Desconto { get; set; }

    public decimal Subtotal =>
        Itens.Sum(item => item.Subtotal);

    public decimal ValorTotal =>
        Subtotal - Desconto;
}
