
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using VendeAi.Controllers;
using VendeAi.Models;

namespace VendeAi;

public class ClienteForm : Form
{
    private readonly ClienteController controller;
    private readonly Cliente? clienteOriginal;
    private readonly bool somenteLeitura;

    private readonly Color azul = Color.FromArgb(37, 74, 235);
    private readonly Color azulHover = Color.FromArgb(29, 60, 200);
    private readonly Color texto = Color.FromArgb(20, 30, 60);
    private readonly Color cinza = Color.FromArgb(105, 115, 140);
    private readonly Color borda = Color.FromArgb(218, 225, 240);
    private readonly Color fundoCampo = Color.White;

    private TextBox txtNome = null!;
    private TextBox txtDocumento = null!;
    private TextBox txtTelefone = null!;
    private TextBox txtEmail = null!;
    private TextBox txtEndereco = null!;
    private TextBox txtNumero = null!;
    private TextBox txtComplemento = null!;
    private TextBox txtBairro = null!;
    private TextBox txtCidade = null!;
    private ComboBox cmbEstado = null!;
    private TextBox txtCep = null!;
    private TextBox txtObs = null!;
    private ComboBox cmbStatus = null!;

    private Button btnSalvar = null!;

    public ClienteForm(
        ClienteController controller,
        Cliente? cliente = null,
        bool somenteLeitura = false)
    {
        this.controller = controller;
        clienteOriginal = cliente;
        this.somenteLeitura = somenteLeitura;

        CriarInterface();
        ConfigurarMascaras();

        if (cliente != null)
            PreencherCampos(cliente);

        if (somenteLeitura)
            ConfigurarSomenteLeitura();
    }

    private void CriarInterface()
    {
        SuspendLayout();

        Text = "VendeAí | Cliente";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 690);
        MinimumSize = new Size(900, 690);
        MaximumSize = new Size(900, 690);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10);
        ShowInTaskbar = false;
        AutoScroll = false;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true
        );

        Shown += (_, _) => Arredondar(this, 18);

        // CABEÇALHO
        Panel cabecalho = new Panel
        {
            Dock = DockStyle.Top,
            Height = 88,
            BackColor = Color.White
        };
        Controls.Add(cabecalho);

        Label titulo = new Label
        {
            Text = clienteOriginal == null
                ? "Cadastrar cliente"
                : somenteLeitura
                    ? "Visualizar cliente"
                    : "Editar cliente",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = texto,
            AutoSize = true,
            Location = new Point(32, 17)
        };
        cabecalho.Controls.Add(titulo);

        Label subtitulo = new Label
        {
            Text = clienteOriginal == null
                ? "Preencha as informações do cliente para cadastrá-lo no sistema."
                : somenteLeitura
                    ? "Consulte as informações cadastradas do cliente."
                    : "Atualize as informações do cliente.",
            Font = new Font("Segoe UI", 9),
            ForeColor = cinza,
            AutoSize = true,
            Location = new Point(34, 53)
        };
        cabecalho.Controls.Add(subtitulo);

        Button btnFechar = new Button
        {
            Text = "✕",
            Size = new Size(36, 36),
            Location = new Point(836, 20),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = cinza,
            Font = new Font("Segoe UI", 12),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        btnFechar.FlatAppearance.BorderSize = 0;
        btnFechar.Click += (_, _) => Close();
        cabecalho.Controls.Add(btnFechar);

        Panel linhaSuperior = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = Color.FromArgb(237, 240, 246)
        };
        cabecalho.Controls.Add(linhaSuperior);

        // RODAPÉ
        Panel rodape = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 75,
            BackColor = Color.White
        };
        Controls.Add(rodape);

        Panel linhaRodape = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(237, 240, 246)
        };
        rodape.Controls.Add(linhaRodape);

        Button btnCancelar = CriarBotao(
            somenteLeitura ? "Fechar" : "Cancelar",
            false
        );
        btnCancelar.Location = new Point(32, 16);
        btnCancelar.Size = new Size(145, 43);
        btnCancelar.Click += (_, _) => Close();
        rodape.Controls.Add(btnCancelar);

        btnSalvar = CriarBotao(
            clienteOriginal == null
                ? "Salvar cliente"
                : "Salvar alterações",
            true
        );
        btnSalvar.Location = new Point(675, 16);
        btnSalvar.Size = new Size(195, 43);
        btnSalvar.Click += SalvarCliente;
        btnSalvar.Visible = !somenteLeitura;
        rodape.Controls.Add(btnSalvar);

        // ÁREA CENTRAL
        Panel conteudo = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            AutoScroll = false
        };
        Controls.Add(conteudo);
        conteudo.BringToFront();

        const int margem = 32;
        const int espacamento = 18;
        const int larguraTotal = 836;
        const int coluna = (larguraTotal - espacamento) / 2;

        int x1 = margem;
        int x2 = margem + coluna + espacamento;

        // INFORMAÇÕES BÁSICAS
        CriarSecao(conteudo, "Informações básicas", 15);

        txtNome = CriarCampo(
            conteudo, "Nome completo *",
            x1, 48, coluna,
            "Ex.: Ana Carolina Souza"
        );

        txtDocumento = CriarCampo(
            conteudo, "CPF/CNPJ *",
            x2, 48, coluna,
            "Ex.: 123.456.789-00"
        );

        txtTelefone = CriarCampo(
            conteudo, "Telefone *",
            x1, 113, coluna,
            "Ex.: (11) 99999-9999"
        );

        txtEmail = CriarCampo(
            conteudo, "E-mail",
            x2, 113, coluna,
            "Ex.: cliente@email.com"
        );

        txtEndereco = CriarCampo(
            conteudo, "Endereço",
            x1, 178, larguraTotal,
            "Ex.: Rua das Flores"
        );

        int larguraTres =
            (larguraTotal - 2 * espacamento) / 3;

        txtNumero = CriarCampo(
            conteudo, "Número",
            x1, 243, larguraTres,
            "Ex.: 123"
        );

        txtComplemento = CriarCampo(
            conteudo, "Complemento",
            x1 + larguraTres + espacamento,
            243, larguraTres,
            "Ex.: Apto 12"
        );

        txtBairro = CriarCampo(
            conteudo, "Bairro",
            x1 + (larguraTres + espacamento) * 2,
            243, larguraTres,
            "Ex.: Centro"
        );

        txtCidade = CriarCampo(
            conteudo, "Cidade",
            x1, 308, larguraTres,
            "Ex.: Itu"
        );

        cmbEstado = CriarCombo(
            conteudo, "Estado",
            x1 + larguraTres + espacamento,
            308, larguraTres,
            new[]
            {
                "Selecione",
                "AC", "AL", "AP", "AM", "BA", "CE", "DF",
                "ES", "GO", "MA", "MT", "MS", "MG", "PA",
                "PB", "PR", "PE", "PI", "RJ", "RN", "RS",
                "RO", "RR", "SC", "SP", "SE", "TO"
            }
        );

        txtCep = CriarCampo(
            conteudo, "CEP",
            x1 + (larguraTres + espacamento) * 2,
            308, larguraTres,
            "Ex.: 13300-000"
        );

        // INFORMAÇÕES ADICIONAIS
        CriarSecao(
            conteudo,
            "Informações adicionais",
            390
        );

        CriarLabel(
            conteudo,
            "Observações",
            x1,
            426
        );

        Panel painelObs = CriarPainelCampo(
            conteudo, x1, 449, 575, 76
        );

        txtObs = new TextBox
        {
            Location = new Point(12, 9),
            Size = new Size(550, 58),
            Multiline = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = ScrollBars.None,
            MaxLength = 500,
            Font = new Font("Segoe UI", 10),
            ForeColor = texto,
            BackColor = fundoCampo,
            PlaceholderText =
                "Adicione observações sobre o cliente (opcional)..."
        };
        painelObs.Controls.Add(txtObs);

        cmbStatus = CriarCombo(
            conteudo, "Status *",
            627, 426, 241,
            new[] { "Ativo", "Inativo" }
        );
        cmbStatus.SelectedIndex = 0;

        ResumeLayout();
    }

    // =====================================
    // MÁSCARAS AUTOMÁTICAS
    // =====================================

    private void ConfigurarMascaras()
    {
        txtDocumento.TextChanged += (_, _) =>
            AplicarMascara(
                txtDocumento,
                14,
                FormatarDocumento
            );

        txtTelefone.TextChanged += (_, _) =>
            AplicarMascara(
                txtTelefone,
                11,
                FormatarTelefone
            );

        txtCep.TextChanged += (_, _) =>
            AplicarMascara(
                txtCep,
                8,
                FormatarCep
            );
    }

    private static void AplicarMascara(
        TextBox campo,
        int limiteDigitos,
        Func<string, string> formatador)
    {
        string textoAnterior = campo.Text;
        int posicaoAnterior = campo.SelectionStart;

        // Conta quantos dígitos estavam antes do cursor.
        int digitosAntesCursor = textoAnterior
            .Take(posicaoAnterior)
            .Count(char.IsDigit);

        string digitos = ApenasDigitos(textoAnterior);

        if (digitos.Length > limiteDigitos)
            digitos = digitos[..limiteDigitos];

        string textoFormatado = formatador(digitos);

        if (textoAnterior == textoFormatado)
            return;

        campo.Text = textoFormatado;

        // Mantém o cursor próximo da posição editada,
        // inclusive quando a alteração ocorre no meio.
        int novaPosicao = 0;
        int digitosEncontrados = 0;

        while (novaPosicao < textoFormatado.Length &&
               digitosEncontrados < digitosAntesCursor)
        {
            if (char.IsDigit(textoFormatado[novaPosicao]))
                digitosEncontrados++;

            novaPosicao++;
        }

        campo.SelectionStart = Math.Min(
            novaPosicao,
            campo.Text.Length
        );

        campo.SelectionLength = 0;
    }

    private static string ApenasDigitos(string? valor)
    {
        return new string(
            (valor ?? string.Empty)
                .Where(char.IsDigit)
                .ToArray()
        );
    }

    private static string FormatarDocumento(string digitos)
    {
        if (digitos.Length <= 11)
        {
            return AplicarPadrao(
                digitos,
                "000.000.000-00"
            );
        }

        return AplicarPadrao(
            digitos,
            "00.000.000/0000-00"
        );
    }

    private static string FormatarTelefone(string digitos)
    {
        if (digitos.Length <= 10)
        {
            return AplicarPadrao(
                digitos,
                "(00) 0000-0000"
            );
        }

        return AplicarPadrao(
            digitos,
            "(00) 00000-0000"
        );
    }

    private static string FormatarCep(string digitos)
    {
        return AplicarPadrao(
            digitos,
            "00000-000"
        );
    }

    private static string AplicarPadrao(
        string digitos,
        string padrao)
    {
        if (string.IsNullOrEmpty(digitos))
            return string.Empty;

        string resultado = string.Empty;
        int indice = 0;

        foreach (char caractere in padrao)
        {
            if (indice >= digitos.Length)
                break;

            if (caractere == '0')
            {
                resultado += digitos[indice];
                indice++;
            }
            else
            {
                resultado += caractere;
            }
        }

        return resultado;
    }

    // =====================================
    // COMPONENTES VISUAIS
    // =====================================

    private void CriarSecao(
        Control pai,
        string titulo,
        int y)
    {
        Label label = new Label
        {
            Text = titulo,
            Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Bold
            ),
            ForeColor = texto,
            AutoSize = true,
            Location = new Point(32, y)
        };

        pai.Controls.Add(label);
    }

    private void CriarLabel(
        Control pai,
        string titulo,
        int x,
        int y)
    {
        Label label = new Label
        {
            Text = titulo,
            Font = new Font(
                "Segoe UI",
                9,
                FontStyle.Bold
            ),
            ForeColor = texto,
            AutoSize = true,
            Location = new Point(x, y)
        };

        pai.Controls.Add(label);
    }

    private Panel CriarPainelCampo(
        Control pai,
        int x,
        int y,
        int largura,
        int altura)
    {
        Panel painel = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(largura, altura),
            BackColor = fundoCampo
        };

        painel.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using GraphicsPath caminho =
                CriarCaminhoArredondado(
                    new Rectangle(
                        0,
                        0,
                        painel.Width - 1,
                        painel.Height - 1
                    ),
                    7
                );

            using Pen caneta = new Pen(borda, 1);

            e.Graphics.DrawPath(caneta, caminho);
        };

        pai.Controls.Add(painel);
        return painel;
    }

    private TextBox CriarCampo(
        Control pai,
        string titulo,
        int x,
        int y,
        int largura,
        string exemplo)
    {
        CriarLabel(
            pai,
            titulo,
            x,
            y
        );

        Panel painel = CriarPainelCampo(
            pai,
            x,
            y + 23,
            largura,
            36
        );

        TextBox campo = new TextBox
        {
            Location = new Point(11, 9),
            Width = largura - 22,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10),
            ForeColor = texto,
            BackColor = fundoCampo,
            PlaceholderText = exemplo
        };

        painel.Controls.Add(campo);

        return campo;
    }

    private ComboBox CriarCombo(
        Control pai,
        string titulo,
        int x,
        int y,
        int largura,
        string[] opcoes)
    {
        CriarLabel(
            pai,
            titulo,
            x,
            y
        );

        Panel painel = CriarPainelCampo(
            pai,
            x,
            y + 23,
            largura,
            36
        );

        ComboBox combo = new ComboBox
        {
            Location = new Point(7, 5),
            Width = largura - 14,
            Font = new Font("Segoe UI", 10),
            ForeColor = texto,
            BackColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };

        combo.Items.AddRange(opcoes);
        combo.SelectedIndex = 0;

        painel.Controls.Add(combo);

        return combo;
    }

    private Button CriarBotao(
        string titulo,
        bool primario)
    {
        Button botao = new Button
        {
            Text = titulo,
            BackColor = primario
                ? azul
                : Color.White,
            ForeColor = primario
                ? Color.White
                : texto,
            FlatStyle = FlatStyle.Flat,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Bold
            ),
            Cursor = Cursors.Hand
        };

        botao.FlatAppearance.BorderSize =
            primario ? 0 : 1;

        botao.FlatAppearance.BorderColor = borda;

        botao.MouseEnter += (_, _) =>
        {
            if (primario)
                botao.BackColor = azulHover;
        };

        botao.MouseLeave += (_, _) =>
        {
            if (primario)
                botao.BackColor = azul;
        };

        botao.Resize += (_, _) =>
            Arredondar(botao, 8);

        return botao;
    }

    private static GraphicsPath CriarCaminhoArredondado(
        Rectangle retangulo,
        int raio)
    {
        GraphicsPath caminho = new GraphicsPath();
        int d = raio * 2;

        caminho.AddArc(
            retangulo.X,
            retangulo.Y,
            d,
            d,
            180,
            90
        );

        caminho.AddArc(
            retangulo.Right - d,
            retangulo.Y,
            d,
            d,
            270,
            90
        );

        caminho.AddArc(
            retangulo.Right - d,
            retangulo.Bottom - d,
            d,
            d,
            0,
            90
        );

        caminho.AddArc(
            retangulo.X,
            retangulo.Bottom - d,
            d,
            d,
            90,
            90
        );

        caminho.CloseFigure();
        return caminho;
    }

    private static void Arredondar(
        Control controle,
        int raio)
    {
        if (controle.Width <= 0 ||
            controle.Height <= 0)
        {
            return;
        }

        using GraphicsPath caminho =
            CriarCaminhoArredondado(
                new Rectangle(
                    0,
                    0,
                    controle.Width,
                    controle.Height
                ),
                raio
            );

        Region? regiaoAnterior = controle.Region;

        controle.Region = new Region(caminho);

        regiaoAnterior?.Dispose();
    }

    // =====================================
    // PREENCHER CAMPOS
    // =====================================

    private void PreencherCampos(Cliente cliente)
    {
        txtNome.Text = cliente.Nome;
        txtDocumento.Text = cliente.CPF;
        txtTelefone.Text = cliente.Telefone;
        txtEmail.Text = cliente.Email;
        txtEndereco.Text = cliente.Endereco;
        txtNumero.Text = cliente.Numero;
        txtComplemento.Text = cliente.Complemento;
        txtBairro.Text = cliente.Bairro;
        txtCidade.Text = cliente.Cidade;
        txtCep.Text = cliente.CEP;
        txtObs.Text = cliente.Obs;

        cmbEstado.SelectedItem = cliente.Estado;

        cmbStatus.SelectedItem =
            cliente.StatusCliente == StatusCliente.Ativo
                ? "Ativo"
                : "Inativo";
    }

    // =====================================
    // MODO VISUALIZAÇÃO
    // =====================================

    private void ConfigurarSomenteLeitura()
    {
        foreach (Control controle in ObterControles(this))
        {
            if (controle is TextBox campo)
                campo.ReadOnly = true;

            if (controle is ComboBox combo)
                combo.Enabled = false;
        }
    }

    private static IEnumerable<Control> ObterControles(
        Control pai)
    {
        foreach (Control controle in pai.Controls)
        {
            yield return controle;

            foreach (Control filho in ObterControles(controle))
                yield return filho;
        }
    }

    // =====================================
    // SALVAR CLIENTE
    // =====================================

    private void SalvarCliente(
        object? sender,
        EventArgs e)
    {
        if (somenteLeitura)
            return;

        try
        {
            string documento =
                ApenasDigitos(txtDocumento.Text);

            TipoCliente tipo;

            if (documento.Length == 11)
            {
                tipo = TipoCliente.PessoaFisica;
            }
            else if (documento.Length == 14)
            {
                tipo = TipoCliente.PessoaJuridica;
            }
            else
            {
                throw new ArgumentException(
                    "Informe um CPF com 11 dígitos ou CNPJ com 14 dígitos."
                );
            }

            Cliente cliente = new Cliente
            {
                Nome = txtNome.Text.Trim(),
                CPF = documento,
                Telefone = txtTelefone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Endereco = txtEndereco.Text.Trim(),
                Numero = txtNumero.Text.Trim(),
                Complemento = txtComplemento.Text.Trim(),
                Bairro = txtBairro.Text.Trim(),
                Cidade = txtCidade.Text.Trim(),

                Estado =
                    cmbEstado.SelectedItem?.ToString()
                        == "Selecione"
                        ? string.Empty
                        : cmbEstado.SelectedItem?.ToString()
                            ?? string.Empty,

                CEP = txtCep.Text.Trim(),
                Obs = txtObs.Text.Trim(),
                TipoCliente = tipo,

                StatusCliente =
                    cmbStatus.SelectedItem?.ToString()
                        == "Inativo"
                        ? StatusCliente.Inativo
                        : StatusCliente.Ativo
            };

            if (clienteOriginal == null)
            {
                controller.CadastrarCliente(cliente);
            }
            else
            {
                controller.AtualizarCliente(
                    clienteOriginal.CPF,
                    cliente
                );
            }

            MessageBox.Show(
                clienteOriginal == null
                    ? "Cliente cadastrado com sucesso!"
                    : "Cliente atualizado com sucesso!",
                "VendeAí",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Verifique os dados",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }
}
