using System;
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;
using VendeAi.Repositories;

namespace VendeAi.Service;

public class CompraService
{
    private readonly PedidoRepository pedidos;
    private readonly CompraRepository compras;

    public CompraService(PedidoRepository pedidos, CompraRepository compras)
    {
        this.pedidos = pedidos;
        this.compras = compras;
    }

    public (List<Compra> Produtos, int TotalPedidos) Consolidar(DateTime inicio, DateTime fim)
    {
        if (inicio.Date > fim.Date)
            return (new List<Compra>(), 0);

        var elegiveis = pedidos.ListarTodos()
            .Where(p => p.Status.ToString() is "Pago" or "Concluido" or "Concluído")
            .Where(p => p.DataPedido.Date >= inicio.Date && p.DataPedido.Date <= fim.Date)
            .ToList();

        var produtos = elegiveis.SelectMany(p => p.Itens)
            .GroupBy(i => new { Produto = i.Produto.CodigoSku, Variacao = i.ProdutoVariacao?.CodigoSku ?? "" })
            .Select(g =>
            {
                var item = g.First();
                string chave = $"{g.Key.Produto}|{g.Key.Variacao}";
                string variacao = item.ProdutoVariacao == null
                    ? "Produto principal"
                    : string.Join(" - ", new[] { item.ProdutoVariacao.Cor, item.ProdutoVariacao.Tamanho }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
                if (string.IsNullOrWhiteSpace(variacao)) variacao = "Produto principal";
                return new Compra
                {
                    Chave = chave,
                    Produto = item.Produto.Nome,
                    Variacao = variacao,
                    Categoria = item.Produto.Categoria.ToString() switch
                    {
                        "Cosmeticos" => "Cosméticos",
                        "Acessorios" => "Acessórios",
                        "Calcados" => "Calçados",
                        var categoria => categoria
                    },
                    Quantidade = g.Sum(i => i.Quantidade),
                    Status = compras.ObterStatus(chave)
                };
            })
            .OrderBy(c => c.Produto).ThenBy(c => c.Variacao).ToList();

        return (produtos, elegiveis.Count);
    }

    public void AtualizarStatus(string chave, StatusCompra status) => compras.SalvarStatus(chave, status);
}
