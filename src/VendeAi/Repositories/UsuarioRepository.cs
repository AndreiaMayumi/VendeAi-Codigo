
using System;
using System.Collections.Generic;
using System.Linq;
using VendeAi.Models;

namespace VendeAi.Repositories;

public class UsuarioRepository
{
    private readonly List<Usuario> usuarios = new();

    public UsuarioRepository()
    {
        // Usuário temporário para desenvolvimento.
        // Não utilizar esta conta em produção.
        usuarios.Add(new Usuario
        {
            Id = 1,
            Nome = "Usuário de teste",
            Email = "teste@vendeai.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(
                "Teste@12345"
            ),
            Ativo = true
        });
    }

    public Usuario? BuscarPorEmail(string email)
    {
        return usuarios.FirstOrDefault(usuario =>
            usuario.Email.Equals(
                email,
                StringComparison.OrdinalIgnoreCase
            )
        );
    }
}
