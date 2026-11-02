
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FontAwesome.Sharp;
using VendeAi.Controllers;
using VendeAi.Models;
using VendeAi.Repositories;
using VendeAi.Service;

namespace VendeAi;

public class ManterClientesForm : Form
{
    // =====================================
    // CORES DO VENDEAÍ
    // =====================================

    private readonly Color corPrimaria = Color.FromArgb(37, 74, 235);
    private readonly Color corPrimariaHover = Color.FromArgb(29, 60, 200);
    private readonly Color corMenu = Color.FromArgb(15, 30, 60);
    private readonly Color corMenuHover = Color.FromArgb(25, 45, 82);
    private readonly Color corFundo = Color.FromArgb(245, 247, 251);
    private readonly Color corTexto = Color.FromArgb(20, 30, 50);
    private readonly Color corSecundaria = Color.FromArgb(100, 110, 130);
    private readonly Color corBorda = Color.FromArgb(225, 230, 240);
    private readonly Color corBarra = Color.FromArgb(34, 59, 96);

    // =====================================
    // CAMADAS
    // =====================================

    private readonly ClienteRepository clienteRepository;
    private readonly ClienteController clienteController;

    // =====================================
    // CONTROLES
    // =====================================

    private TextBox txtPesquisa = null!;
    private ComboBox cmbStatus = null!;
    private DataGridView tabelaClientes = null!;
    private Label lblTotal = null!;

    // =====================================
    // CONSTRUTOR
    // =====================================

    public ManterClientesForm()
    {
        clienteRepository = new ClienteRepository();

        ClienteService service = new ClienteService(clienteRepository);

        clienteController = new ClienteController(service);

        CriarInterface();

        CarregarClientes();
    }

    // =====================================
    // INTERFACE PRINCIPAL
    // =====================================

    private void CriarInterface()
    {
        SuspendLayout();

        Text = "VendeAí | Clientes";
        BackColor = corFundo;
        FormBorderStyle = FormBorderStyle.None;
        MaximizedBounds = Screen.FromControl(this).WorkingArea;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1050, 700);
        Font = new Font("Segoe UI", 10);
        AutoScroll = false;

        Controls.Clear();

        // ÁREA DE TRABALHO
        Panel areaTrabalho = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = corFundo
        };

        Controls.Add(areaTrabalho);

        // MENU LATERAL
        Panel menu = new Panel
        {
            Dock = DockStyle.Left,
            Width = 230,
            BackColor = corMenu
        };

        areaTrabalho.Controls.Add(menu);

        CriarMenu(menu);

        // ÁREA PRINCIPAL
        Panel principal = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = corFundo
        };

        areaTrabalho.Controls.Add(principal);
        principal.BringToFront();

        // BARRA SUPERIOR PERSONALIZADA
        Panel barraTitulo = CriarBarraTitulo();

        Controls.Add(barraTitulo);
        barraTitulo.BringToFront();

        // TOPO DO SISTEMA
        Panel topo = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.White
        };

        principal.Controls.Add(topo);

        Label usuario = new Label
        {
            Text = "Mayumi  ▾",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = corTexto,
            AutoSize = true
        };

        topo.Controls.Add(usuario);

        topo.Resize += (_, _) =>
        {
            usuario.Location = new Point(
                Math.Max(
                    15,
                    topo.ClientSize.Width - usuario.Width - 35
                ),
                22
            );
        };

        // CONTEÚDO PRINCIPAL
        Panel conteudo = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = corFundo,
            Padding = new Padding(25, 20, 25, 25)
        };

        principal.Controls.Add(conteudo);
        conteudo.BringToFront();

        // CABEÇALHO
        Panel cabecalho = new Panel
        {
            Dock = DockStyle.Top,
            Height = 125,
            BackColor = corFundo
        };

        conteudo.Controls.Add(cabecalho);

        IconPictureBox iconeTitulo = new IconPictureBox
        {
            IconChar = IconChar.Users,
            IconColor = corPrimaria,
            IconSize = 35,
            Size = new Size(45, 45),
            Location = new Point(0, 12),
            BackColor = corFundo
        };

        cabecalho.Controls.Add(iconeTitulo);

        Label titulo = new Label
        {
            Text = "Clientes",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = corTexto,
            AutoSize = true,
            Location = new Point(55, 6)
        };

        cabecalho.Controls.Add(titulo);

        Label subtitulo = new Label
        {
            Text = "Gerencie seus clientes, cadastre novos e mantenha as informações atualizadas.",
            Font = new Font("Segoe UI", 10),
            ForeColor = corSecundaria,
            AutoSize = true,
            Location = new Point(57, 60)
        };

        cabecalho.Controls.Add(subtitulo);

        Button btnNovo = CriarBotao(
            "+  Novo cliente",
            true
        );

        btnNovo.Name = "btnNovoCliente";
        btnNovo.Size = new Size(185, 46);
        btnNovo.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        cabecalho.Controls.Add(btnNovo);

        btnNovo.Click += (_, _) => AbrirCadastro();

        cabecalho.Resize += (_, _) =>
        {
            btnNovo.Location = new Point(
                Math.Max(
                    0,
                    cabecalho.ClientSize.Width - btnNovo.Width
                ),
                15
            );

            ArredondarControle(btnNovo, 8);
        };

        // CARD DA LISTAGEM
        Panel card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(20)
        };

        conteudo.Controls.Add(card);
        card.BringToFront();

        // FILTROS
        Panel filtros = new Panel
        {
            Dock = DockStyle.Top,
            Height = 75,
            BackColor = Color.White
        };

        card.Controls.Add(filtros);

        // CAMPO DE PESQUISA
        Panel painelPesquisa = CriarContornoCampo(
            filtros,
            0,
            12,
            390,
            42
        );

        IconPictureBox iconePesquisa = new IconPictureBox
        {
            IconChar = IconChar.MagnifyingGlass,
            IconColor = corSecundaria,
            IconSize = 18,
            Location = new Point(12, 11),
            Size = new Size(20, 20),
            BackColor = Color.White
        };

        painelPesquisa.Controls.Add(iconePesquisa);

        txtPesquisa = new TextBox
        {
            PlaceholderText = "Buscar por nome ou CPF/CNPJ...",
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10),
            ForeColor = corTexto,
            BackColor = Color.White,
            Location = new Point(43, 12),
            Width = 330
        };

        painelPesquisa.Controls.Add(txtPesquisa);

        // FILTRO DE STATUS
        Panel painelStatus = CriarContornoCampo(
            filtros,
            405,
            12,
            190,
            42
        );

        cmbStatus = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
            ForeColor = corTexto,
            BackColor = Color.White,
            Location = new Point(7, 8),
            Width = 175
        };

        cmbStatus.Items.AddRange(new object[]
        {
            "Todos os status",
            "Ativo",
            "Inativo"
        });

        cmbStatus.SelectedIndex = 0;

        painelStatus.Controls.Add(cmbStatus);

        txtPesquisa.TextChanged += (_, _) => CarregarClientes();

        cmbStatus.SelectedIndexChanged += (_, _) => CarregarClientes();

        // RODAPÉ DA TABELA
        Panel rodape = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 55,
            BackColor = Color.White
        };

        card.Controls.Add(rodape);

        lblTotal = new Label
        {
            Text = "0 clientes encontrados",
            ForeColor = corSecundaria,
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(5, 19)
        };

        rodape.Controls.Add(lblTotal);

        // =====================================
        // TABELA DE CLIENTES
        // =====================================

        tabelaClientes = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,

            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,

            ReadOnly = true,
            MultiSelect = false,

            SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,

            RowHeadersVisible = false,

            AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill,

            ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing,

            ColumnHeadersHeight = 48,

            RowTemplate = { Height = 56 },

            EnableHeadersVisualStyles = false,

            CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal,

            ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None,

            GridColor = Color.FromArgb(238, 241, 247),

            ShowCellToolTips = true
        };

        tabelaClientes.ColumnHeadersDefaultCellStyle.BackColor =
            Color.FromArgb(248, 250, 254);

        tabelaClientes.ColumnHeadersDefaultCellStyle.ForeColor =
            corTexto;

        tabelaClientes.ColumnHeadersDefaultCellStyle.Font =
            new Font("Segoe UI", 10, FontStyle.Bold);

        tabelaClientes.ColumnHeadersDefaultCellStyle.Padding =
            new Padding(10, 0, 10, 0);

        tabelaClientes.DefaultCellStyle.Font =
            new Font("Segoe UI", 10);

        tabelaClientes.DefaultCellStyle.ForeColor =
            corTexto;

        tabelaClientes.DefaultCellStyle.BackColor =
            Color.White;

        tabelaClientes.DefaultCellStyle.Padding =
            new Padding(10, 5, 10, 5);

        tabelaClientes.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(237, 242, 255);

        tabelaClientes.DefaultCellStyle.SelectionForeColor =
            corTexto;

        tabelaClientes.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(250, 251, 254);

        // COLUNAS
        tabelaClientes.Columns.Add("Nome", "Nome");
        tabelaClientes.Columns.Add("Documento", "CPF/CNPJ");
        tabelaClientes.Columns.Add("Telefone", "Telefone");
        tabelaClientes.Columns.Add("Email", "E-mail");
        tabelaClientes.Columns.Add("Status", "Status");

        AdicionarColunaAcao("Visualizar", "Ver");
        AdicionarColunaAcao("Editar", "Editar");
        AdicionarColunaAcao("Excluir", "Excluir");

        tabelaClientes.Columns["Nome"]!.FillWeight = 170;
        tabelaClientes.Columns["Documento"]!.FillWeight = 120;
        tabelaClientes.Columns["Telefone"]!.FillWeight = 110;
        tabelaClientes.Columns["Email"]!.FillWeight = 170;
        tabelaClientes.Columns["Status"]!.FillWeight = 80;

        foreach (string nome in new[]
        {
            "Visualizar",
            "Editar",
            "Excluir"
        })
        {
            tabelaClientes.Columns[nome]!.FillWeight = 65;
        }

        tabelaClientes.CellContentClick +=
            TabelaClientes_CellContentClick;

        tabelaClientes.CellFormatting +=
            TabelaClientes_CellFormatting;

        card.Controls.Add(tabelaClientes);
        tabelaClientes.BringToFront();

        ResumeLayout();
    }

    // =====================================
    // MENU LATERAL
    // =====================================

    private void CriarMenu(Panel menu)
    {
        string caminhoLogo = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "logo-vendeai.png"
        );

        if (File.Exists(caminhoLogo))
        {
            PictureBox logo = new PictureBox
            {
                Image = Image.FromFile(caminhoLogo),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(20, 22),
                Size = new Size(190, 65),
                BackColor = corMenu
            };

            menu.Controls.Add(logo);
        }

        Label descricao = new Label
        {
            Text = "Gestão de vendas",
            ForeColor = Color.FromArgb(160, 175, 200),
            Font = new Font("Segoe UI", 9),
            Location = new Point(35, 85),
            AutoSize = true
        };

        menu.Controls.Add(descricao);

        int y = 140;

        foreach (var item in new (IconChar Icone, string Texto)[]
        {
            (IconChar.House, "Painel"),
            (IconChar.Box, "Produtos"),
            (IconChar.Users, "Clientes"),
            (IconChar.ClipboardList, "Pedidos"),
            (IconChar.CartShopping, "Compras"),
            (IconChar.BoxesStacked, "Separação"),
            (IconChar.ShieldHalved, "Permissões")
        })
        {
            CriarBotaoMenu(
                menu,
                item.Icone,
                item.Texto,
                y,
                item.Texto == "Clientes"
            );

            y += 55;
        }

        // CONFIGURAÇÕES FIXADO NO RODAPÉ
        IconButton configuracoes = CriarBotaoMenu(
            menu,
            IconChar.Gear,
            "Configurações",
            0,
            false
        );

        configuracoes.Dock = DockStyle.Bottom;
        configuracoes.Height = 60;
    }

    private IconButton CriarBotaoMenu(
        Panel menu,
        IconChar icone,
        string texto,
        int y,
        bool selecionado)
    {
        IconButton botao = new IconButton
        {
            Text = string.Empty,
            IconChar = IconChar.None,
            Location = new Point(15, y),
            Size = new Size(200, 45),
            FlatStyle = FlatStyle.Flat,

            BackColor =
                selecionado ? corPrimaria : corMenu,

            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            TabStop = true
        };

        botao.FlatAppearance.BorderSize = 0;

        botao.FlatAppearance.MouseOverBackColor =
            selecionado ? corPrimaria : corMenuHover;

        IconPictureBox imagemIcone = new IconPictureBox
        {
            IconChar = icone,
            IconColor = Color.White,
            IconSize = 21,
            Location = new Point(17, 11),
            Size = new Size(24, 24),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        Label textoMenu = new Label
        {
            Text = texto,
            ForeColor = Color.White,
            BackColor = Color.Transparent,

            Font = new Font(
                "Segoe UI",
                10,
                selecionado
                    ? FontStyle.Bold
                    : FontStyle.Regular
            ),

            Location = new Point(48, 0),
            Size = new Size(145, 45),
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand
        };

        imagemIcone.Click += (_, _) =>
            botao.PerformClick();

        textoMenu.Click += (_, _) =>
            botao.PerformClick();

        if (!selecionado)
        {
            botao.MouseEnter += (_, _) =>
                botao.BackColor = corMenuHover;

            botao.MouseLeave += (_, _) =>
                botao.BackColor = corMenu;
        }

        botao.Controls.Add(imagemIcone);
        botao.Controls.Add(textoMenu);

        menu.Controls.Add(botao);

        ArredondarControle(botao, 6);

        return botao;
    }

    // =====================================
    // BARRA SUPERIOR PERSONALIZADA
    // =====================================

    private Panel CriarBarraTitulo()
    {
        Panel barra = new Panel
        {
            Dock = DockStyle.Top,
            Height = 25,
            BackColor = corBarra
        };

        IconPictureBox icone = new IconPictureBox
        {
            IconChar = IconChar.CartShopping,
            IconColor = Color.White,
            IconSize = 17,
            Size = new Size(20, 20),
            Location = new Point(15, 5),
            BackColor = corBarra
        };

        barra.Controls.Add(icone);

        Label titulo = new Label
        {
            Text = "VendeAí | Clientes",
            ForeColor = Color.White,
            Font = new Font(
                "Segoe UI",
                9,
                FontStyle.Bold
            ),
            AutoSize = true,
            Location = new Point(43, 6),
            BackColor = corBarra
        };

        barra.Controls.Add(titulo);

        Button CriarBotaoJanela(
            string simbolo,
            string nome)
        {
            Button botao = new Button
            {
                Name = nome,
                Text = simbolo,
                Dock = DockStyle.Right,
                Width = 46,
                FlatStyle = FlatStyle.Flat,
                BackColor = corBarra,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                TabStop = false
            };

            botao.FlatAppearance.BorderSize = 0;

            botao.FlatAppearance.MouseOverBackColor =
                nome == "Fechar"
                    ? Color.FromArgb(190, 40, 55)
                    : corMenuHover;

            return botao;
        }

        Button fechar =
            CriarBotaoJanela("✕", "Fechar");

        Button maximizar =
            CriarBotaoJanela("□", "Maximizar");

        Button minimizar =
            CriarBotaoJanela("─", "Minimizar");

        fechar.Click += (_, _) => Close();

        minimizar.Click += (_, _) =>
            WindowState = FormWindowState.Minimized;

        maximizar.Click += (_, _) =>
            AlternarMaximizacao();

        SizeChanged += (_, _) =>
        {
            maximizar.Text =
                WindowState == FormWindowState.Maximized
                    ? "❐"
                    : "□";
        };

        // ORDEM CORRETA DOS BOTÕES
        barra.Controls.Add(minimizar);
        barra.Controls.Add(maximizar);
        barra.Controls.Add(fechar);

        barra.MouseDown += IniciarArrasteJanela;
        titulo.MouseDown += IniciarArrasteJanela;
        icone.MouseDown += IniciarArrasteJanela;

        barra.DoubleClick += (_, _) =>
            AlternarMaximizacao();

        return barra;
    }

    private void AlternarMaximizacao()
    {
        WindowState =
            WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
    }

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        int msg,
        int wParam,
        int lParam
    );

    private void IniciarArrasteJanela(
        object? sender,
        MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || e.Clicks > 1)
            return;

        ReleaseCapture();

        SendMessage(
            Handle,
            0xA1,
            0x2,
            0
        );
    }

    // =====================================
    // BOTÕES
    // =====================================

    private Button CriarBotao(
        string texto,
        bool primario)
    {
        Button botao = new Button
        {
            Text = texto,

            BackColor =
                primario ? corPrimaria : Color.White,

            ForeColor =
                primario ? Color.White : corPrimaria,

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

        botao.FlatAppearance.BorderColor =
            corPrimaria;

        botao.MouseEnter += (_, _) =>
        {
            if (primario)
                botao.BackColor = corPrimariaHover;
        };

        botao.MouseLeave += (_, _) =>
        {
            if (primario)
                botao.BackColor = corPrimaria;
        };

        return botao;
    }

    // =====================================
    // CAMPOS COM BORDA
    // =====================================

    private Panel CriarContornoCampo(
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
            BackColor = Color.White
        };

        painel.Paint += (_, e) =>
        {
            using Pen caneta =
                new Pen(corBorda, 1);

            e.Graphics.DrawRectangle(
                caneta,
                0,
                0,
                painel.Width - 1,
                painel.Height - 1
            );
        };

        pai.Controls.Add(painel);

        return painel;
    }

    // =====================================
    // BORDAS ARREDONDADAS
    // =====================================

    private void ArredondarControle(
        Control controle,
        int raio)
    {
        if (controle.Width <= 0 ||
            controle.Height <= 0)
        {
            return;
        }

        using GraphicsPath caminho =
            new GraphicsPath();

        int d = raio * 2;

        caminho.AddArc(
            0, 0, d, d, 180, 90
        );

        caminho.AddArc(
            controle.Width - d,
            0,
            d, d, 270, 90
        );

        caminho.AddArc(
            controle.Width - d,
            controle.Height - d,
            d, d, 0, 90
        );

        caminho.AddArc(
            0,
            controle.Height - d,
            d, d, 90, 90
        );

        caminho.CloseFigure();

        controle.Region = new Region(caminho);
    }

    // =====================================
    // COLUNAS DE AÇÃO
    // =====================================

    private void AdicionarColunaAcao(
        string nome,
        string texto)
    {
        DataGridViewButtonColumn coluna =
            new DataGridViewButtonColumn
            {
                Name = nome,
                HeaderText = "",
                Text = texto,
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat
            };

        tabelaClientes.Columns.Add(coluna);
    }

    // =====================================
    // LISTAR E FILTRAR CLIENTES
    // =====================================

    private void CarregarClientes()
    {
        if (tabelaClientes == null ||
            tabelaClientes.IsDisposed ||
            txtPesquisa == null ||
            cmbStatus == null)
        {
            return;
        }

        string pesquisa =
            txtPesquisa.Text.Trim();

        var clientes =
            clienteController.ListarClientes()
                .Where(c =>
                    c.Nome.Contains(
                        pesquisa,
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    c.CPF.Contains(pesquisa)
                );

        if (cmbStatus.SelectedIndex == 1)
        {
            clientes = clientes.Where(c =>
                c.StatusCliente ==
                    StatusCliente.Ativo
            );
        }
        else if (cmbStatus.SelectedIndex == 2)
        {
            clientes = clientes.Where(c =>
                c.StatusCliente ==
                    StatusCliente.Inativo
            );
        }

        var resultado = clientes.ToList();

        tabelaClientes.Rows.Clear();

        foreach (Cliente cliente in resultado)
        {
            int indice =
                tabelaClientes.Rows.Add(
                    cliente.Nome,
                    cliente.CPF,
                    cliente.Telefone,
                    cliente.Email,
                    cliente.StatusCliente.ToString()
                );

            tabelaClientes.Rows[indice].Tag =
                cliente.CPF;
        }

        lblTotal.Text =
            $"{resultado.Count} cliente(s) encontrado(s)";
    }

    // =====================================
    // ESTILIZAÇÃO DAS CÉLULAS
    // =====================================

    private void TabelaClientes_CellFormatting(
        object? sender,
        DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        string nomeColuna =
            tabelaClientes.Columns[e.ColumnIndex].Name;

        if (nomeColuna == "Status")
        {
            string status =
                e.Value?.ToString() ?? "";

            e.CellStyle.ForeColor =
                status == "Ativo"
                    ? Color.FromArgb(22, 130, 80)
                    : Color.FromArgb(190, 45, 55);

            e.CellStyle.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold
                );
        }
    }

    // =====================================
    // POP-UP DE CLIENTES
    // =====================================

    private DialogResult MostrarPopupCliente(
        Cliente? cliente = null,
        bool somenteLeitura = false)
    {
        // JANELA SEMITRANSPARENTE
        using Form sombra = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            ShowInTaskbar = false,
            BackColor = Color.Black,
            Opacity = 0.45,
            Bounds = Bounds
        };

        sombra.Show(this);

        try
        {
            using ClienteForm formulario =
                new ClienteForm(
                    clienteController,
                    cliente,
                    somenteLeitura
                );

            formulario.StartPosition =
                FormStartPosition.CenterParent;

            return formulario.ShowDialog(sombra);
        }
        finally
        {
            sombra.Close();
        }
    }

    // =====================================
    // CADASTRAR CLIENTE
    // =====================================

    private void AbrirCadastro()
    {
        if (MostrarPopupCliente() == DialogResult.OK)
        {
            CarregarClientes();
        }
    }

    // =====================================
    // AÇÕES DA TABELA
    // =====================================

    private void TabelaClientes_CellContentClick(
        object? sender,
        DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0)
        {
            return;
        }

        string coluna =
            tabelaClientes.Columns[e.ColumnIndex].Name;

        string documento =
            tabelaClientes.Rows[e.RowIndex]
                .Tag?.ToString()
            ?? string.Empty;

        // EXCLUIR
        if (coluna == "Excluir")
        {
            DialogResult confirmacao =
                MessageBox.Show(
                    "Deseja realmente excluir este cliente?",
                    "Confirmar exclusão",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

            if (confirmacao != DialogResult.Yes)
                return;

            try
            {
                clienteController.ExcluirCliente(documento);

                CarregarClientes();

                MessageBox.Show(
                    "Cliente excluído com sucesso!",
                    "VendeAí",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Erro ao excluir",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // EDITAR OU VISUALIZAR
        else if (
            coluna == "Editar" ||
            coluna == "Visualizar")
        {
            Cliente? cliente =
                clienteController.BuscarClientePorCpf(
                    documento
                );

            if (cliente == null)
            {
                MessageBox.Show(
                    "Cliente não encontrado.",
                    "VendeAí",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            bool somenteLeitura =
                coluna == "Visualizar";

            if (
                MostrarPopupCliente(
                    cliente,
                    somenteLeitura
                ) == DialogResult.OK
            )
            {
                CarregarClientes();
            }
        }
    }
}
