using System;
using System.Collections.Generic;
using VendeAi.Models;
using VendeAi.Service;

namespace VendeAi.Controllers;

public class CompraController
{
    private readonly CompraService service;
    public CompraController(CompraService service) => this.service = service;
    public (List<Compra> Produtos, int TotalPedidos) Listar(DateTime inicio, DateTime fim) =>
        service.Consolidar(inicio, fim);
    public void AtualizarStatus(string chave, StatusCompra status) => service.AtualizarStatus(chave, status);
}
