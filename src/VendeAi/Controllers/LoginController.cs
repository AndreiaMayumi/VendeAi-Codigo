
using VendeAi.Models;
using VendeAi.Service;

namespace VendeAi.Controllers;

public class LoginController
{
    private readonly AutenticacaoService autenticacaoService;

    public LoginController(
        AutenticacaoService autenticacaoService)
    {
        this.autenticacaoService = autenticacaoService;
    }

    public Usuario? FazerLogin(
        string email,
        string senha)
    {
        return autenticacaoService.Autenticar(
            email,
            senha
        );
    }
}
