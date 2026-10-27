
using System;
using VendeAi.Models;
using VendeAi.Repositories;

namespace VendeAi.Service;

public class AutenticacaoService
{
    private readonly UsuarioRepository usuarioRepository;

    public AutenticacaoService(
        UsuarioRepository usuarioRepository)
    {
        this.usuarioRepository = usuarioRepository;
    }

    public Usuario? Autenticar(
        string email,
        string senha)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(senha))
        {
            return null;
        }

        Usuario? usuario =
            usuarioRepository.BuscarPorEmail(email.Trim());

        if (usuario == null || !usuario.Ativo)
        {
            return null;
        }

        bool senhaValida = BCrypt.Net.BCrypt.Verify(
            senha,
            usuario.SenhaHash
        );

        if (!senhaValida)
        {
            return null;
        }

        return usuario;
    }
}
