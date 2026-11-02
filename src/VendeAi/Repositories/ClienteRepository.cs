
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;

namespace VendeAi.Repositories;

public class ClienteRepository
{
    private readonly List<Cliente> clientes = new();

    // Cadastrar cliente
    public void Adicionar(Cliente cliente)
    {
        clientes.Add(cliente);
    }

    // Consultar todos os clientes
    public List<Cliente> ListarTodos()
    {
        return new List<Cliente>(clientes);
    }

    // Buscar cliente pelo CPF
    public Cliente? BuscarPorCpf(string cpf)
    {
        return clientes.FirstOrDefault(
            cliente => cliente.CPF == cpf
        );
    }

    // Atualizar cliente
    public bool Atualizar(string cpf, Cliente clienteAtualizado)
    {
        Cliente? cliente = BuscarPorCpf(cpf);

        if (cliente == null)
        {
            return false;
        }

        cliente.Nome = clienteAtualizado.Nome;
        cliente.CPF = clienteAtualizado.CPF;
        cliente.Email = clienteAtualizado.Email;
        cliente.Telefone = clienteAtualizado.Telefone;
        cliente.Endereco = clienteAtualizado.Endereco;
        cliente.Numero = clienteAtualizado.Numero;
        cliente.Complemento = clienteAtualizado.Complemento;
        cliente.Bairro = clienteAtualizado.Bairro;
        cliente.Cidade = clienteAtualizado.Cidade;
        cliente.Estado = clienteAtualizado.Estado;
        cliente.CEP = clienteAtualizado.CEP;
        cliente.Obs = clienteAtualizado.Obs;
        cliente.StatusCliente = clienteAtualizado.StatusCliente;
        cliente.TipoCliente = clienteAtualizado.TipoCliente;

        return true;
    }

    // Excluir cliente
    public bool Excluir(string cpf)
    {
        Cliente? cliente = BuscarPorCpf(cpf);

        if (cliente == null)
        {
            return false;
        }

        return clientes.Remove(cliente);
    }
}
