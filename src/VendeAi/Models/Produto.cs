
using System.Collections.Generic;

namespace VendeAi.Models;

public enum Modalidade
{
    Varejo,
    Atacado,
    Pacote,
    Lote
}

public enum StatusProduto
{
    Ativo,
    Inativo
}

public enum Categoria
{
    Roupas,
    Cosmeticos,
    Acessorios
}

public class Produto
{
    public required string Nome { get; set; }

    public required string CodigoSku { get; set; }

    public string? Descricao { get; set; }

    public required Categoria Categoria { get; set; }

    public string? Marca { get; set; }

    public string? Imagem { get; set; }

    public StatusProduto Status { get; set; } =
        StatusProduto.Ativo;

    // Mantido para compatibilidade com o cadastro atual.
    public List<ProdutoModalidade> Modalidades { get; set; } = new();

    // Novas variações vinculadas ao produto principal.
    public List<ProdutoVariacao> Variacoes { get; set; } = new();
}
