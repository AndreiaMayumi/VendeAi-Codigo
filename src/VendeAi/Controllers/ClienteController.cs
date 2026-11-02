
using System.Collections.Generic;
using VendeAi.Models;
using VendeAi.Service;

namespace VendeAi.Controllers;

public class ClienteController
{
    private readonly ClienteService clienteService;

    public ClienteController(ClienteService clienteService)
    {
        this.clienteService = clienteService;
    }

    // Cadastrar um novo cliente
    public void CadastrarCliente(Cliente cliente)
    {
        clienteService.Cadastrar(cliente);
    }

    // Listar todos os clientes
    public List<Cliente> ListarClientes()
    {
        return clienteService.ListarTodos();
    }

    // Consultar cliente pelo CPF ou CNPJ
    public Cliente? BuscarClientePorCpf(string cpf)
    {
        return clienteService.BuscarPorCpf(cpf);
    }

    // Alterar os dados de um cliente
    public void AtualizarCliente(
        string cpfOriginal,
        Cliente clienteAtualizado)
    {
        clienteService.Atualizar(
            cpfOriginal,
            clienteAtualizado
        );
    }

    // Excluir um cliente
    public void ExcluirCliente(string cpf)
    {
        clienteService.Excluir(cpf);
    }
}
