
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using VendeAi.Models;
using VendeAi.Repositories;

namespace VendeAi.Service;

public class ClienteService
{
    private readonly ClienteRepository clienteRepository;

    public ClienteService(ClienteRepository clienteRepository)
    {
        this.clienteRepository = clienteRepository;
    }

    // =====================================
    // CADASTRAR CLIENTE
    // =====================================

    public void Cadastrar(Cliente cliente)
    {
        ValidarCliente(cliente);

        if (clienteRepository.BuscarPorCpf(cliente.CPF) != null)
        {
            throw new InvalidOperationException(
                "Já existe um cliente cadastrado com esse CPF/CNPJ."
            );
        }

        clienteRepository.Adicionar(cliente);
    }

    // =====================================
    // CONSULTAR CLIENTES
    // =====================================

    public List<Cliente> ListarTodos()
    {
        return clienteRepository.ListarTodos();
    }

    public Cliente? BuscarPorCpf(string cpf)
    {
        return clienteRepository.BuscarPorCpf(
            NormalizarDocumento(cpf)
        );
    }

    // =====================================
    // ATUALIZAR CLIENTE
    // =====================================

    public void Atualizar(
        string cpfOriginal,
        Cliente clienteAtualizado)
    {
        if (clienteAtualizado == null)
        {
            throw new ArgumentException(
                "Informe os dados do cliente."
            );
        }

        string documentoOriginal =
            NormalizarDocumento(cpfOriginal);

        Cliente? existente =
            clienteRepository.BuscarPorCpf(documentoOriginal);

        if (existente == null)
        {
            throw new InvalidOperationException(
                "Cliente não encontrado."
            );
        }

        ValidarCliente(clienteAtualizado);

        Cliente? outroCliente =
            clienteRepository.BuscarPorCpf(clienteAtualizado.CPF);

        if (outroCliente != null &&
            !ReferenceEquals(outroCliente, existente))
        {
            throw new InvalidOperationException(
                "Já existe outro cliente com esse CPF/CNPJ."
            );
        }

        bool atualizado = clienteRepository.Atualizar(
            documentoOriginal,
            clienteAtualizado
        );

        if (!atualizado)
        {
            throw new InvalidOperationException(
                "Não foi possível atualizar o cliente."
            );
        }
    }

    // =====================================
    // EXCLUIR CLIENTE
    // =====================================

    public void Excluir(string cpf)
    {
        string documento = NormalizarDocumento(cpf);

        bool excluido =
            clienteRepository.Excluir(documento);

        if (!excluido)
        {
            throw new InvalidOperationException(
                "Cliente não encontrado."
            );
        }
    }

    // =====================================
    // VALIDAR DADOS DO CLIENTE
    // =====================================

    private void ValidarCliente(Cliente cliente)
    {
        if (cliente == null)
        {
            throw new ArgumentException(
                "Informe os dados do cliente."
            );
        }

        // NOME OBRIGATÓRIO
        if (string.IsNullOrWhiteSpace(cliente.Nome))
        {
            throw new ArgumentException(
                "O nome do cliente é obrigatório."
            );
        }

        cliente.Nome = cliente.Nome.Trim();

        // NORMALIZAR CPF OU CNPJ
        cliente.CPF = NormalizarDocumento(cliente.CPF);

        // VALIDAR DOCUMENTO CONFORME O TIPO
        if (cliente.TipoCliente == TipoCliente.PessoaFisica)
        {
            if (!ValidarCpf(cliente.CPF))
            {
                throw new ArgumentException(
                    "Informe um CPF válido."
                );
            }
        }
        else if (cliente.TipoCliente == TipoCliente.PessoaJuridica)
        {
            if (!ValidarCnpj(cliente.CPF))
            {
                throw new ArgumentException(
                    "Informe um CNPJ válido."
                );
            }
        }
        else
        {
            throw new ArgumentException(
                "Tipo de cliente inválido."
            );
        }

        // TELEFONE OBRIGATÓRIO
        if (string.IsNullOrWhiteSpace(cliente.Telefone))
        {
            throw new ArgumentException(
                "O telefone do cliente é obrigatório."
            );
        }

        cliente.Telefone = cliente.Telefone.Trim();

        string telefoneNumerico = new string(
            cliente.Telefone
                .Where(char.IsDigit)
                .ToArray()
        );

        if (telefoneNumerico.Length != 10 &&
            telefoneNumerico.Length != 11)
        {
            throw new ArgumentException(
                "Informe um telefone válido com DDD."
            );
        }

        // E-MAIL OPCIONAL, MAS DEVE SER VÁLIDO
        cliente.Email = (cliente.Email ?? string.Empty).Trim();

        if (!string.IsNullOrWhiteSpace(cliente.Email))
        {
            if (!MailAddress.TryCreate(
                    cliente.Email,
                    out MailAddress? endereco)
                || endereco.Address != cliente.Email)
            {
                throw new ArgumentException(
                    "Informe um e-mail válido."
                );
            }
        }

        // NORMALIZAR CAMPOS OPCIONAIS
        cliente.Endereco =
            (cliente.Endereco ?? string.Empty).Trim();

        cliente.Numero =
            (cliente.Numero ?? string.Empty).Trim();

        cliente.Complemento =
            (cliente.Complemento ?? string.Empty).Trim();

        cliente.Bairro =
            (cliente.Bairro ?? string.Empty).Trim();

        cliente.Cidade =
            (cliente.Cidade ?? string.Empty).Trim();

        cliente.Estado =
            (cliente.Estado ?? string.Empty).Trim().ToUpperInvariant();

        cliente.CEP =
            (cliente.CEP ?? string.Empty).Trim();

        cliente.Obs =
            (cliente.Obs ?? string.Empty).Trim();

        // VALIDAR CEP, SE PREENCHIDO
        if (!string.IsNullOrWhiteSpace(cliente.CEP))
        {
            string cepNumerico = new string(
                cliente.CEP.Where(char.IsDigit).ToArray()
            );

            if (cepNumerico.Length != 8)
            {
                throw new ArgumentException(
                    "Informe um CEP com 8 dígitos."
                );
            }

            cliente.CEP = cepNumerico;
        }

        // VALIDAR STATUS
        if (!Enum.IsDefined(cliente.StatusCliente))
        {
            throw new ArgumentException(
                "Status do cliente inválido."
            );
        }
    }

    // =====================================
    // NORMALIZAR DOCUMENTO
    // =====================================

    private static string NormalizarDocumento(string? documento)
    {
        return new string(
            (documento ?? string.Empty)
                .Where(char.IsDigit)
                .ToArray()
        );
    }

    // =====================================
    // VALIDAR CPF
    // =====================================

    private static bool ValidarCpf(string cpf)
    {
        if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            return false;

        // Rejeita sequências repetidas, como 11111111111.
        if (cpf.Distinct().Count() == 1)
            return false;

        int soma = 0;

        for (int i = 0; i < 9; i++)
        {
            soma += (cpf[i] - '0') * (10 - i);
        }

        int resto = soma % 11;

        int primeiroDigito =
            resto < 2 ? 0 : 11 - resto;

        if (primeiroDigito != cpf[9] - '0')
            return false;

        soma = 0;

        for (int i = 0; i < 10; i++)
        {
            soma += (cpf[i] - '0') * (11 - i);
        }

        resto = soma % 11;

        int segundoDigito =
            resto < 2 ? 0 : 11 - resto;

        return segundoDigito == cpf[10] - '0';
    }

    // =====================================
    // VALIDAR CNPJ
    // =====================================

    private static bool ValidarCnpj(string cnpj)
    {
        if (cnpj.Length != 14 || !cnpj.All(char.IsDigit))
            return false;

        // Rejeita sequências repetidas.
        if (cnpj.Distinct().Count() == 1)
            return false;

        int[] pesosPrimeiro =
        {
            5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2
        };

        int[] pesosSegundo =
        {
            6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2
        };

        int soma = 0;

        for (int i = 0; i < 12; i++)
        {
            soma += (cnpj[i] - '0') * pesosPrimeiro[i];
        }

        int resto = soma % 11;

        int primeiroDigito =
            resto < 2 ? 0 : 11 - resto;

        if (primeiroDigito != cnpj[12] - '0')
            return false;

        soma = 0;

        for (int i = 0; i < 13; i++)
        {
            soma += (cnpj[i] - '0') * pesosSegundo[i];
        }

        resto = soma % 11;

        int segundoDigito =
            resto < 2 ? 0 : 11 - resto;

        return segundoDigito == cnpj[13] - '0';
    }
}
