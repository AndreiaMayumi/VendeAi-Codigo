
using VendeAi.Models;
using VendeAi.Repositories;
using VendeAi.Controllers;

namespace VendeAi.Tests;

public class ProdutoTests
{
    private readonly ProdutoRepository repository = new();
    private readonly ProdutoController controller;

    public ProdutoTests()
    {
        controller = new ProdutoController(repository);
    }

    private Produto CriarProdutoValido()
    {
        return new Produto
        {
            Nome = "Regata básica",
            CodigoSku = "REG001",
            Categoria = "Roupas",
            Modalidade = ModalidadeVenda.Varejo,
            QuantidadeMinimaVenda = 1,
            PrecoVenda = 39.90m
        };
    }

    [Fact]
    public void DeveCadastrarProdutoValido()
    {
        var produto = CriarProdutoValido();

        string resultado = controller.Cadastrar(produto);

        Assert.Equal(
            "Produto cadastrado com sucesso.",
            resultado
        );

        Assert.Single(repository.ListarTodos());
        Assert.Equal(StatusProduto.Ativo, produto.Status);
    }

    [Fact]
    public void NaoDeveCadastrarProdutoComSkuDuplicado()
    {
        var primeiro = CriarProdutoValido();
        controller.Cadastrar(primeiro);

        var segundo = CriarProdutoValido();
        string resultado = controller.Cadastrar(segundo);

        Assert.Equal(
            "Já existe um produto cadastrado com este SKU.",
            resultado
        );

        Assert.Single(repository.ListarTodos());
    }

    [Fact]
    public void NaoDeveCadastrarProdutoSemNome()
    {
        var produto = CriarProdutoValido();
        produto.Nome = "";

        string resultado = controller.Cadastrar(produto);

        Assert.Equal(
            "O nome do produto é obrigatório.",
            resultado
        );

        Assert.Empty(repository.ListarTodos());
    }
}
