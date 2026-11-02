
namespace VendeAi.Models;

public class Cliente
{
    public string Nome { get; set; } = string.Empty;

    // Armazena CPF ou CNPJ, conforme o tipo do cliente.
    public string CPF { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public string Endereco { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string Complemento { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string CEP { get; set; } = string.Empty;

    public string Obs { get; set; } = string.Empty;

    public StatusCliente StatusCliente { get; set; }
        = StatusCliente.Ativo;

    public TipoCliente TipoCliente { get; set; }
        = TipoCliente.PessoaFisica;
}
