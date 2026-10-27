
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FontAwesome.Sharp;
using VendeAi.Controllers;
using VendeAi.Models;
using VendeAi.Repositories;
using VendeAi.Service;

namespace VendeAi;

public class LoginForm : Form
{
    // ==========================================
    // CORES DO VENDEAÍ
    // ==========================================

    private readonly Color corMenu = Color.FromArgb(15, 30, 60);
    private readonly Color corBarra = Color.FromArgb(34, 59, 96);
    private readonly Color corPrimaria = Color.FromArgb(37, 74, 235);
    private readonly Color corTexto = Color.FromArgb(15, 30, 60);
    private readonly Color corSecundaria = Color.FromArgb(91, 103, 128);
    private readonly Color corFundo = Color.FromArgb(245, 247, 251);
    private readonly LoginController loginController;

    // ==========================================
    // COMPONENTES
    // ==========================================

    private TextBox txtEmail = null!;
    private TextBox txtSenha = null!;
    private CheckBox chkLembrar = null!;

    private Panel painelEsquerdo = null!;
    private Panel painelDireito = null!;
    private Panel formulario = null!;

    private Image? imagemLogo;
    private Image? imagemLogoNegativa;
    private Image? imagemFundo;

    // ==========================================
    // CONTROLE DA JANELA
    // ==========================================

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        int msg,
        int wParam,
        int lParam);

    public LoginForm()
    {
        Text = "Login | VendeAí";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 650);
        BackColor = corFundo;
        Font = new Font("Segoe UI", 10);

        UsuarioRepository usuarioRepository =
            new UsuarioRepository();

        AutenticacaoService autenticacaoService =
            new AutenticacaoService(usuarioRepository);

        loginController =
            new LoginController(autenticacaoService);
            
        CarregarImagens();
        CriarInterface();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        MaximizedBounds = Screen.FromControl(this).WorkingArea;

        if (WindowState == FormWindowState.Maximized)
        {
            WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
        }
    }

    // ==========================================
    // CARREGAMENTO DAS IMAGENS
    // ==========================================

    private Image? CarregarImagem(string nomeArquivo)
    {
        string pastaAssets = Path.Combine(
            AppContext.BaseDirectory,
            "Assets"
        );

        string caminho = Path.Combine(
            pastaAssets,
            nomeArquivo
        );

        // Se a imagem ainda não foi copiada para bin,
        // tenta encontrá-la na pasta original do projeto.
        if (!File.Exists(caminho))
        {
            string pastaProjeto = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", ".."
                )
            );

            caminho = Path.Combine(
                pastaProjeto,
                "Assets",
                nomeArquivo
            );
        }

        if (!File.Exists(caminho))
            return null;

        try
        {
            using Image original = Image.FromFile(caminho);
            return new Bitmap(original);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Não foi possível carregar {nomeArquivo}.\n\n" +
                ex.Message,
                "VendeAí",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            return null;
        }
    }

    private void CarregarImagens()
    {
        // Logo com letras brancas: lado escuro.
        imagemLogo = CarregarImagem(
            "logo-vendeai.png"
        );

        // Logo com letras escuras: lado branco.
        imagemLogoNegativa = CarregarImagem(
            "logo-vendeai-negativo.png"
        );

        // Nome correto, conforme sua pasta Assets.
        imagemFundo = CarregarImagem(
            "login-fundo.png"
        );
    }

    // ==========================================
    // INTERFACE PRINCIPAL
    // ==========================================

    private void CriarInterface()
    {
        SuspendLayout();
        Controls.Clear();

        Panel areaPrincipal = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = corFundo,
            Padding = new Padding(25)
        };

        Controls.Add(areaPrincipal);

        Panel barraTitulo = CriarBarraTitulo();
        Controls.Add(barraTitulo);
        barraTitulo.BringToFront();

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = Color.White
        };

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50)
        );

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50)
        );

        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100)
        );

        areaPrincipal.Controls.Add(layout);

        CriarPainelEsquerdo(layout);
        CriarPainelDireito(layout);

        ResumeLayout(true);
    }

    // ==========================================
    // BARRA DE TÍTULO
    // ==========================================

    private Panel CriarBarraTitulo()
    {
        Panel barra = new Panel
        {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = corBarra
        };

        IconPictureBox icone = new IconPictureBox
        {
            IconChar = IconChar.CartShopping,
            IconColor = Color.White,
            IconSize = 15,
            Size = new Size(20, 20),
            Location = new Point(14, 5),
            BackColor = corBarra
        };

        Label titulo = new Label
        {
            Text = "VendeAí | Login",
            ForeColor = Color.White,
            Font = new Font(
                "Segoe UI", 9, FontStyle.Bold
            ),
            AutoSize = true,
            Location = new Point(43, 6),
            BackColor = corBarra
        };

        barra.Controls.Add(icone);
        barra.Controls.Add(titulo);

        // Área exclusiva para os botões.
        // Posições fixas evitam inversão.
        Panel areaBotoes = new Panel
        {
            Dock = DockStyle.Right,
            Width = 138,
            Height = 30,
            BackColor = corBarra
        };

        barra.Controls.Add(areaBotoes);

        Button CriarBotao(
            string simbolo,
            string nome,
            int x)
        {
            Button botao = new Button
            {
                Name = nome,
                Text = simbolo,
                Location = new Point(x, 0),
                Size = new Size(46, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = corBarra,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                TabStop = false
            };

            botao.FlatAppearance.BorderSize = 0;

            botao.FlatAppearance.MouseOverBackColor =
                nome == "Fechar"
                    ? Color.FromArgb(200, 45, 55)
                    : Color.FromArgb(52, 73, 104);

            return botao;
        }

        Button btnMinimizar = CriarBotao(
            "─", "Minimizar", 0
        );

        Button btnMaximizar = CriarBotao(
            "□", "Maximizar", 46
        );

        Button btnFechar = CriarBotao(
            "✕", "Fechar", 92
        );

        btnMinimizar.Click += (_, _) =>
            WindowState = FormWindowState.Minimized;

        btnMaximizar.Click += (_, _) =>
            AlternarMaximizacao();

        btnFechar.Click += (_, _) => Close();

        areaBotoes.Controls.Add(btnMinimizar);
        areaBotoes.Controls.Add(btnMaximizar);
        areaBotoes.Controls.Add(btnFechar);

        barra.MouseDown += ArrastarJanela;
        titulo.MouseDown += ArrastarJanela;
        icone.MouseDown += ArrastarJanela;

        barra.DoubleClick += (_, _) =>
            AlternarMaximizacao();

        titulo.DoubleClick += (_, _) =>
            AlternarMaximizacao();

        return barra;
    }

    private void ArrastarJanela(
        object? sender,
        MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
            return;

        ReleaseCapture();
        SendMessage(Handle, 0xA1, 0x2, 0);
    }

    private void AlternarMaximizacao()
    {
        WindowState =
            WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
    }

    // ==========================================
    // PAINEL ESQUERDO
    // ==========================================

    private void CriarPainelEsquerdo(
        TableLayoutPanel layout)
    {
        painelEsquerdo = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = corMenu,
            Padding = new Padding(65, 55, 35, 35)
        };

        layout.Controls.Add(painelEsquerdo, 0, 0);

        painelEsquerdo.Paint += DesenharFundoEsquerdo;

        Panel conteudo = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            AutoScroll = false
        };

        painelEsquerdo.Controls.Add(conteudo);

        PictureBox logo = new PictureBox
        {
            Location = new Point(0, 0),
            Size = new Size(240, 65),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Image = imagemLogo
        };

        conteudo.Controls.Add(logo);

        if (imagemLogo == null)
        {
            Label logoAlternativa = new Label
            {
                Text = "VendeAí",
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI", 25, FontStyle.Bold
                ),
                AutoSize = true,
                Location = new Point(0, 10),
                BackColor = Color.Transparent
            };

            conteudo.Controls.Add(logoAlternativa);
        }

        Label chamada = new Label
        {
            Text =
                "Gestão de vendas\n" +
                "de forma simples\n" +
                "e prática",
            ForeColor = Color.White,
            Font = new Font(
                "Segoe UI", 26, FontStyle.Bold
            ),
            AutoSize = true,
            Location = new Point(0, 135),
            BackColor = Color.Transparent
        };

        conteudo.Controls.Add(chamada);

        Label descricao = new Label
        {
            Text =
                "Organize seus produtos, clientes e vendas\n" +
                "em um só lugar.",
            ForeColor = Color.FromArgb(225, 230, 242),
            Font = new Font("Segoe UI", 12),
            AutoSize = true,
            Location = new Point(0, 300),
            BackColor = Color.Transparent
        };

        conteudo.Controls.Add(descricao);

        AdicionarBeneficio(
            conteudo,
            IconChar.Box,
            "Cadastro de produtos",
            "Organize seus produtos e suas variações",
            390
        );

        AdicionarBeneficio(
            conteudo,
            IconChar.Users,
            "Gestão de clientes",
            "Mantenha seus clientes sempre por perto",
            475
        );

        AdicionarBeneficio(
            conteudo,
            IconChar.ChartColumn,
            "Acompanhamento de vendas",
            "Visualize relatórios e resultados",
            560
        );

        Panel indicadores = new Panel
        {
            Location = new Point(0, 670),
            Size = new Size(150, 8),
            BackColor = Color.Transparent
        };

        for (int i = 0; i < 3; i++)
        {
            Panel indicador = new Panel
            {
                Location = new Point(i * 43, 1),
                Size = new Size(34, 5),
                BackColor = i == 0
                    ? Color.FromArgb(47, 156, 255)
                    : Color.FromArgb(110, 125, 153)
            };

            indicadores.Controls.Add(indicador);
        }

        conteudo.Controls.Add(indicadores);
    }

    private void DesenharFundoEsquerdo(
        object? sender,
        PaintEventArgs e)
    {
        Rectangle area = painelEsquerdo.ClientRectangle;

        if (area.Width <= 0 || area.Height <= 0)
            return;

        if (imagemFundo != null)
        {
            e.Graphics.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            e.Graphics.DrawImage(
                imagemFundo,
                painelEsquerdo.ClientRectangle
            );
        }

        // Camada escura para destacar os textos.
        using (SolidBrush sombra = new SolidBrush(
            Color.FromArgb(205, 10, 20, 43)))
        {
            e.Graphics.FillRectangle(sombra, area);
        }

        e.Graphics.SmoothingMode =
            SmoothingMode.AntiAlias;

        using GraphicsPath curva = new GraphicsPath();

        int w = area.Width;
        int h = area.Height;

        Point[] pontos =
        {
            new Point(0, h - 60),
            new Point(w / 3, h - 20),
            new Point(w * 2 / 3, h - 110),
            new Point(w, h - 150),
            new Point(w, h),
            new Point(0, h)
        };

        curva.AddPolygon(pontos);

        using SolidBrush pincel = new SolidBrush(
            Color.FromArgb(85, 55, 78, 220)
        );

        e.Graphics.FillPath(pincel, curva);
    }

    private void AdicionarBeneficio(
        Panel painel,
        IconChar icone,
        string titulo,
        string descricao,
        int y)
    {
        Panel linha = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(470, 70),
            BackColor = Color.Transparent
        };

        Panel fundoIcone = new Panel
        {
            Location = new Point(0, 4),
            Size = new Size(54, 54),
            BackColor = Color.FromArgb(37, 51, 80)
        };

        IconPictureBox imagem = new IconPictureBox
        {
            IconChar = icone,
            IconColor = Color.White,
            IconSize = 27,
            Size = new Size(38, 38),
            Location = new Point(8, 8),
            BackColor = fundoIcone.BackColor
        };

        fundoIcone.Controls.Add(imagem);
        linha.Controls.Add(fundoIcone);

        Label lblTitulo = new Label
        {
            Text = titulo,
            ForeColor = Color.White,
            Font = new Font(
                "Segoe UI", 12, FontStyle.Bold
            ),
            AutoSize = true,
            Location = new Point(72, 5),
            BackColor = Color.Transparent
        };

        Label lblDescricao = new Label
        {
            Text = descricao,
            ForeColor = Color.FromArgb(220, 227, 240),
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(72, 34),
            BackColor = Color.Transparent
        };

        linha.Controls.Add(lblTitulo);
        linha.Controls.Add(lblDescricao);

        painel.Controls.Add(linha);
    }

    // ==========================================
    // PAINEL DIREITO
    // ==========================================

    private void CriarPainelDireito(
        TableLayoutPanel layout)
    {
        painelDireito = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            AutoScroll = false
        };

        layout.Controls.Add(painelDireito, 1, 0);

        formulario = new Panel
        {
            Size = new Size(470, 610),
            BackColor = Color.White
        };

        painelDireito.Controls.Add(formulario);

        painelDireito.Resize += (_, _) =>
            CentralizarFormulario();

        CentralizarFormulario();

        // ======================================
        // LOGO NEGATIVA
        // ======================================

        PictureBox logo = new PictureBox
        {
            Location = new Point(110, 0),
            Size = new Size(250, 75),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.White,
            Image = imagemLogoNegativa
        };

        formulario.Controls.Add(logo);

        if (imagemLogoNegativa == null)
        {
            Label logoAlternativa = new Label
            {
                Text = "VendeAí",
                ForeColor = corTexto,
                Font = new Font(
                    "Segoe UI", 25, FontStyle.Bold
                ),
                AutoSize = true,
                Location = new Point(125, 12)
            };

            formulario.Controls.Add(logoAlternativa);
        }

        // ======================================
        // BOAS-VINDAS
        // ======================================

        Label titulo = new Label
        {
            Text = "Bem-vindo de volta!",
            ForeColor = corTexto,
            Font = new Font(
                "Segoe UI", 24, FontStyle.Bold
            ),
            AutoSize = true,
            Location = new Point(45, 100)
        };

        formulario.Controls.Add(titulo);

        Label subtitulo = new Label
        {
            Text = "Faça login para acessar o sistema.",
            ForeColor = corSecundaria,
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            Location = new Point(90, 155)
        };

        formulario.Controls.Add(subtitulo);

        // ======================================
        // CAMPO E-MAIL
        // ======================================

        formulario.Controls.Add(
            CriarLabelCampo("E-mail", 215)
        );

        Panel caixaEmail = CriarCaixaCampo(247);

        caixaEmail.Controls.Add(
            CriarIconeCampo(IconChar.Envelope)
        );

        txtEmail = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 12),
            ForeColor = corTexto,
            PlaceholderText = "seu@email.com",
            Location = new Point(55, 17),
            Width = 390,
            BackColor = Color.White
        };

        caixaEmail.Controls.Add(txtEmail);
        formulario.Controls.Add(caixaEmail);

        // ======================================
        // CAMPO SENHA
        // ======================================

        formulario.Controls.Add(
            CriarLabelCampo("Senha", 325)
        );

        Panel caixaSenha = CriarCaixaCampo(357);

        caixaSenha.Controls.Add(
            CriarIconeCampo(IconChar.Lock)
        );

        txtSenha = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 12),
            ForeColor = corTexto,
            PlaceholderText = "Digite sua senha",
            UseSystemPasswordChar = true,
            Location = new Point(55, 17),
            Width = 340,
            BackColor = Color.White
        };

        caixaSenha.Controls.Add(txtSenha);

        IconButton btnMostrarSenha = new IconButton
        {
            IconChar = IconChar.EyeSlash,
            IconColor = corSecundaria,
            IconSize = 22,
            Text = "",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Size = new Size(40, 40),
            Location = new Point(420, 7),
            Cursor = Cursors.Hand,
            TabStop = false
        };

        btnMostrarSenha.FlatAppearance.BorderSize = 0;

        btnMostrarSenha.Click += (_, _) =>
        {
            txtSenha.UseSystemPasswordChar =
                !txtSenha.UseSystemPasswordChar;

            btnMostrarSenha.IconChar =
                txtSenha.UseSystemPasswordChar
                    ? IconChar.EyeSlash
                    : IconChar.Eye;
        };

        caixaSenha.Controls.Add(btnMostrarSenha);
        formulario.Controls.Add(caixaSenha);

        // ======================================
        // LEMBRAR DE MIM
        // ======================================

        chkLembrar = new CheckBox
        {
            Text = "Lembrar de mim",
            ForeColor = corTexto,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(0, 435),
            Cursor = Cursors.Hand
        };

        formulario.Controls.Add(chkLembrar);

        // ======================================
        // ESQUECEU SUA SENHA?
        // ======================================

        LinkLabel linkSenha = new LinkLabel
        {
            Text = "Esqueceu sua senha?",
            LinkColor = corPrimaria,
            ActiveLinkColor = corPrimaria,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(310, 436)
        };

        linkSenha.LinkClicked += (_, _) =>
        {
            MessageBox.Show(
                "A recuperação de senha será implementada " +
                "junto com a autenticação do sistema.",
                "VendeAí",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        };

        formulario.Controls.Add(linkSenha);

        // ======================================
        // BOTÃO ENTRAR
        // ======================================

        Button btnEntrar = new Button
        {
            Text = "Entrar",
            Font = new Font(
                "Segoe UI", 12, FontStyle.Bold
            ),
            ForeColor = Color.White,
            BackColor = corPrimaria,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(0, 495),
            Size = new Size(470, 55),
            Cursor = Cursors.Hand
        };

        btnEntrar.FlatAppearance.BorderSize = 0;

        btnEntrar.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(30, 62, 205);

        btnEntrar.Click += (_, _) =>
            ValidarLogin();

        formulario.Controls.Add(btnEntrar);

        AcceptButton = btnEntrar;
    }

    // ==========================================
    // COMPONENTES AUXILIARES
    // ==========================================

    private Label CriarLabelCampo(
        string texto,
        int y)
    {
        return new Label
        {
            Text = texto,
            ForeColor = corTexto,
            Font = new Font(
                "Segoe UI", 11, FontStyle.Bold
            ),
            AutoSize = true,
            Location = new Point(0, y)
        };
    }

    private Panel CriarCaixaCampo(int y)
    {
        Panel caixa = new Panel
        {
            Location = new Point(0, y),
            Size = new Size(470, 55),
            BackColor = Color.White
        };

        caixa.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            Rectangle retangulo = new Rectangle(
                0,
                0,
                caixa.Width - 1,
                caixa.Height - 1
            );

            using Pen borda = new Pen(
                Color.FromArgb(205, 213, 228),
                1
            );

            e.Graphics.DrawRectangle(
                borda,
                retangulo
            );
        };

        return caixa;
    }

    private IconPictureBox CriarIconeCampo(
        IconChar icone)
    {
        return new IconPictureBox
        {
            IconChar = icone,
            IconColor = corSecundaria,
            IconSize = 22,
            Size = new Size(30, 30),
            Location = new Point(15, 13),
            BackColor = Color.White
        };
    }

    private void CentralizarFormulario()
    {
        if (formulario == null ||
            painelDireito == null)
            return;

        formulario.Left = Math.Max(
            10,
            (painelDireito.ClientSize.Width -
             formulario.Width) / 2
        );

        formulario.Top = Math.Max(
            10,
            (painelDireito.ClientSize.Height -
             formulario.Height) / 2
        );
    }

    // ==========================================
    // VALIDAÇÃO INICIAL
    // ==========================================

    
private void ValidarLogin()
{
    string email = txtEmail.Text.Trim();
    string senha = txtSenha.Text;

    if (string.IsNullOrWhiteSpace(email))
    {
        MessageBox.Show(
            "Por favor, informe seu e-mail.",
            "VendeAí",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        txtEmail.Focus();
        return;
    }

    if (!MailAddress.TryCreate(
            email,
            out MailAddress? endereco)
        || endereco.Address != email)
    {
        MessageBox.Show(
            "Informe um e-mail válido.",
            "VendeAí",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        txtEmail.Focus();
        return;
    }

    if (string.IsNullOrWhiteSpace(senha))
    {
        MessageBox.Show(
            "Por favor, informe sua senha.",
            "VendeAí",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        txtSenha.Focus();
        return;
    }

    Usuario? usuario = loginController.FazerLogin(
        email,
        senha
    );

    if (usuario == null)
    {
        MessageBox.Show(
            "E-mail ou senha incorretos.",
            "VendeAí",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        txtSenha.Clear();
        txtSenha.Focus();
        return;
    }

    MessageBox.Show(
        $"Bem-vindo(a), {usuario.Nome}!",
        "VendeAí",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information
    );

    // Próxima etapa:
    // abrir a tela principal após o login.
}


    // ==========================================
    // LIBERAÇÃO DAS IMAGENS
    // ==========================================

    protected override void OnFormClosed(
        FormClosedEventArgs e)
    {
        base.OnFormClosed(e);

        // As imagens são compartilhadas com os
        // PictureBoxes, então são liberadas
        // somente após o fechamento do formulário.
        imagemLogo?.Dispose();
        imagemLogoNegativa?.Dispose();
        imagemFundo?.Dispose();
    }
}
