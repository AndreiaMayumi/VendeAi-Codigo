
using System.Collections.Generic;

namespace VendeAi.Models;

public class ProdutoVariacao
{
    public int Id { get; set; }

    // Identificador próprio da variação.
    public string? CodigoSku { get; set; }

    public string? Cor { get; set; }

    public string? Tamanho { get; set; }

    public string? Descricao { get; set; }

    public string? Imagem { get; set; }

    public StatusProduto Status { get; set; } =
        StatusProduto.Ativo;

    // Cada variação pode ter várias modalidades de venda.
    public List<ProdutoModalidade> Modalidades { get; set; } = new();
}
