using System.Collections.Generic;
using VendeAi.Models;

namespace VendeAi.Repositories;

public class CompraRepository
{
    private readonly Dictionary<string, StatusCompra> statusPorChave = new();

    public StatusCompra ObterStatus(string chave) =>
        statusPorChave.TryGetValue(chave, out var status) ? status : StatusCompra.Aberto;

    public void SalvarStatus(string chave, StatusCompra status) => statusPorChave[chave] = status;
}
