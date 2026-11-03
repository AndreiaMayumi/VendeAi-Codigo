
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
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

public class ManterPedidosForm : Form
{
    // ==========================================
    // CORES
    // ==========================================

    private readonly Color corPrimaria = Color.FromArgb(37, 74, 235);
    private readonly Color corPrimariaHover = Color.FromArgb(29, 60, 200);
    private readonly Color corMenu = Color.FromArgb(15, 30, 60);
    private readonly Color corMenuHover = Color.FromArgb(25, 45, 82);
    private readonly Color corFundo = Color.FromArgb(247, 249, 252);
    private readonly Color corTexto = Color.FromArgb(30, 41, 59);
    private readonly Color corSecundaria = Color.FromArgb(107, 114, 128);
    private readonly Color corBorda = Color.FromArgb(235, 238, 244);
    private readonly Color corBarra = Color.FromArgb(34, 59, 96);

    // ==========================================
    // CAMADAS
    // ==========================================

    private readonly PedidoRepository pedidoRepository;
    private readonly PedidoController pedidoController;

    // ==========================================
    // CONTROLES
    // ==========================================

    private TextBox txtPesquisa = null!;
    private ComboBox cmbStatus = null!;
    private DataGridView tabelaPedidos = null!;
    private Label lblTotal = null!;

    private Panel painelFiltros = null!;
    private Panel painelPesquisa = null!;
    private Panel painelStatus = null!;
    private Panel painelDataInicio = null!;
    private Panel painelDataFim = null!;

    private Label lblDataInicio = null!;
    private Label lblDataFim = null!;
    private Button btnLimparFiltros = null!;

    private TableLayoutPanel cardPedidos = null!;

    private DateTime dataInicial = DateTime.Today.AddMonths(-1);
    private DateTime dataFinal = DateTime.Today;

    private bool filtrarDataInicio;
    private bool filtrarDataFim;

    private readonly CultureInfo culturaBrasil =
        new CultureInfo("pt-BR");

    // ==========================================
    // CONSTRUTOR
    // ==========================================

    public ManterPedidosForm()
    {
        pedidoRepository = AppContexto.Pedidos;

        PedidoService service = new PedidoService(pedidoRepository);

        pedidoController = new PedidoController(service);

        CriarInterface();
        CarregarPedidos();
    }

    // ==========================================
    // INTERFACE PRINCIPAL
    // ==========================================

    private void CriarInterface()
    {
        SuspendLayout();

        Text = "VendeAí | Pedidos";
        BackColor = corFundo;
        FormBorderStyle = FormBorderStyle.None;
        MinimumSize = new Size(1050, 650);
        Font = new Font("Segoe UI", 9);
        AutoScroll = false;

        Controls.Clear();

        // Estrutura principal:
        // Linha 0: barra azul
        // Linha 1: área do sistema

        TableLayoutPanel estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = corFundo
        };

        estrutura.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        estrutura.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 25));

        estrutura.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        Controls.Add(estrutura);

        // ==========================================
        // BARRA AZUL
        // ==========================================

        Panel barraTitulo = CriarBarraTitulo();

        barraTitulo.Dock = DockStyle.Fill;
        barraTitulo.Margin = Padding.Empty;

        estrutura.Controls.Add(barraTitulo, 0, 0);

        // ==========================================
        // MENU + CONTEÚDO
        // ==========================================

        TableLayoutPanel areaTrabalho = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = corFundo
        };

        areaTrabalho.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 210));

        areaTrabalho.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        areaTrabalho.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        estrutura.Controls.Add(areaTrabalho, 0, 1);

        // MENU LATERAL

        Panel menu = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            BackColor = corMenu
        };

        areaTrabalho.Controls.Add(menu, 0, 0);
        CriarMenu(menu);

        // ==========================================
        // ÁREA DIREITA
        // ==========================================

        TableLayoutPanel principal = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = corFundo
        };

        principal.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        principal.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 55));

        principal.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        areaTrabalho.Controls.Add(principal, 1, 0);

        // ==========================================
        // BARRA BRANCA DO USUÁRIO
        // ==========================================

        Panel topo = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            BackColor = Color.White
        };

        Label usuario = new Label
        {
            Text = "Mayumi  ▾",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = corTexto,
            AutoSize = true
        };

        topo.Controls.Add(usuario);

        void PosicionarUsuario()
        {
            usuario.Location = new Point(
                Math.Max(
                    15,
                    topo.ClientSize.Width - usuario.Width - 30
                ),
                Math.Max(
                    0,
                    (topo.ClientSize.Height - usuario.Height) / 2
                )
            );
        }

        topo.Resize += (_, _) => PosicionarUsuario();
        usuario.SizeChanged += (_, _) => PosicionarUsuario();

        principal.Controls.Add(topo, 0, 0);

        // ==========================================
        // CONTEÚDO DA PÁGINA
        // ==========================================

        TableLayoutPanel conteudo = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = new Padding(24, 18, 24, 24),
            BackColor = corFundo
        };

        conteudo.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        conteudo.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 100));

        conteudo.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        principal.Controls.Add(conteudo, 0, 1);

        // ==========================================
        // CABEÇALHO PEDIDOS
        // ==========================================

        Panel cabecalho = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            BackColor = corFundo
        };

        IconPictureBox iconeTitulo = new IconPictureBox
        {
            IconChar = IconChar.ClipboardList,
            IconColor = corPrimaria,
            IconSize = 28,
            Size = new Size(35, 35),
            Location = new Point(0, 9),
            BackColor = corFundo
        };

        cabecalho.Controls.Add(iconeTitulo);

        Label titulo = new Label
        {
            Text = "Pedidos",
            Font = new Font("Segoe UI", 21, FontStyle.Bold),
            ForeColor = corTexto,
            AutoSize = true,
            Location = new Point(43, 2)
        };

        cabecalho.Controls.Add(titulo);

        Label subtitulo = new Label
        {
            Text = "Gerencie e acompanhe seus pedidos.",
            Font = new Font("Segoe UI", 9),
            ForeColor = corSecundaria,
            AutoSize = true,
            Location = new Point(45, 46)
        };

        cabecalho.Controls.Add(subtitulo);

        Button btnNovo = CriarBotao("+  Novo pedido", true);

        btnNovo.Size = new Size(155, 40);
        cabecalho.Controls.Add(btnNovo);

        btnNovo.Click += (_, _) => AbrirCadastro();

        void PosicionarBotaoNovo()
        {
            btnNovo.Location = new Point(
                Math.Max(
                    0,
                    cabecalho.ClientSize.Width - btnNovo.Width
                ),
                8
            );

            ArredondarControle(btnNovo, 9);
        }

        cabecalho.Resize += (_, _) => PosicionarBotaoNovo();

        conteudo.Controls.Add(cabecalho, 0, 0);

        // ==========================================
        // CARD DOS PEDIDOS
        // ==========================================

        cardPedidos = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = new Padding(16),
            BackColor = Color.White
        };

        cardPedidos.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        cardPedidos.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 72));

        cardPedidos.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        cardPedidos.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 43));

        conteudo.Controls.Add(cardPedidos, 0, 1);

        // ==========================================
        // FILTROS
        // ==========================================

        painelFiltros = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            BackColor = Color.White
        };

        cardPedidos.Controls.Add(painelFiltros, 0, 0);

        CriarFiltros();

        // ==========================================
        // RODAPÉ
        // ==========================================

        Panel rodape = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            BackColor = Color.White
        };

        lblTotal = new Label
        {
            Text = "0 pedidos encontrados",
            ForeColor = corSecundaria,
            Font = new Font("Segoe UI", 8.5f),
            AutoSize = true,
            Location = new Point(5, 15)
        };

        rodape.Controls.Add(lblTotal);

        cardPedidos.Controls.Add(rodape, 0, 2);

        // ==========================================
        // TABELA DE PEDIDOS
        // ==========================================

        tabelaPedidos = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
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

            ColumnHeadersHeight = 40,
            RowTemplate = { Height = 46 },

            EnableHeadersVisualStyles = false,

            CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal,

            ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.None,

            GridColor = Color.FromArgb(240, 242, 247),

            ShowCellToolTips = true
        };

        tabelaPedidos.ColumnHeadersDefaultCellStyle.BackColor =
            Color.FromArgb(249, 250, 252);

        tabelaPedidos.ColumnHeadersDefaultCellStyle.ForeColor =
            corSecundaria;

        tabelaPedidos.ColumnHeadersDefaultCellStyle.Font =
            new Font("Segoe UI", 8.5f, FontStyle.Bold);

        tabelaPedidos.ColumnHeadersDefaultCellStyle.Padding =
            new Padding(8, 0, 8, 0);

        tabelaPedidos.DefaultCellStyle.Font =
            new Font("Segoe UI", 9);

        tabelaPedidos.DefaultCellStyle.ForeColor = corTexto;

        tabelaPedidos.DefaultCellStyle.BackColor =
            Color.White;

        tabelaPedidos.DefaultCellStyle.Padding =
            new Padding(8, 4, 8, 4);

        tabelaPedidos.DefaultCellStyle.SelectionBackColor =
            Color.FromArgb(241, 245, 255);

        tabelaPedidos.DefaultCellStyle.SelectionForeColor =
            corTexto;

        tabelaPedidos.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(252, 253, 255);

        tabelaPedidos.Columns.Add("Codigo", "Nº Pedido");
        tabelaPedidos.Columns.Add("Data", "Data");
        tabelaPedidos.Columns.Add("Cliente", "Cliente");
        tabelaPedidos.Columns.Add("Itens", "Itens");
        tabelaPedidos.Columns.Add("Total", "Total");
        tabelaPedidos.Columns.Add("Status", "Status");
        tabelaPedidos.Columns.Add("Pagamento", "Pagamento");

        AdicionarColunaAcao("Visualizar");
        AdicionarColunaAcao("Editar");
        AdicionarColunaAcao("Excluir");

        tabelaPedidos.Columns["Codigo"]!.FillWeight = 95;
        tabelaPedidos.Columns["Data"]!.FillWeight = 120;
        tabelaPedidos.Columns["Cliente"]!.FillWeight = 160;
        tabelaPedidos.Columns["Itens"]!.FillWeight = 55;
        tabelaPedidos.Columns["Total"]!.FillWeight = 100;
        tabelaPedidos.Columns["Status"]!.FillWeight = 145;
        tabelaPedidos.Columns["Pagamento"]!.FillWeight = 120;

        foreach (string nome in new[]
        {
            "Visualizar", "Editar", "Excluir"
        })
        {
            tabelaPedidos.Columns[nome]!.FillWeight = 40;
        }

        tabelaPedidos.CellContentClick +=
            TabelaPedidos_CellContentClick;

        tabelaPedidos.CellFormatting +=
            TabelaPedidos_CellFormatting;

        tabelaPedidos.CellPainting +=
            TabelaPedidos_CellPainting;

        tabelaPedidos.CellToolTipTextNeeded +=
            TabelaPedidos_CellToolTipTextNeeded;

        cardPedidos.Controls.Add(tabelaPedidos, 0, 1);

        ResumeLayout(true);

        PosicionarUsuario();
        PosicionarBotaoNovo();

        MaximizedBounds = Screen.FromControl(this).WorkingArea;
        WindowState = FormWindowState.Maximized;
    }

    // ==========================================
    // FILTROS
    // ==========================================

    private void CriarFiltros()
    {
        // PESQUISA

        painelPesquisa = CriarCampoArredondado(
            painelFiltros, 0, 12, 300, 40
        );

        IconPictureBox iconePesquisa = new IconPictureBox
        {
            IconChar = IconChar.MagnifyingGlass,
            IconColor = corSecundaria,
            IconSize = 16,
            Size = new Size(20, 20),
            Location = new Point(13, 11),
            BackColor = Color.Transparent
        };

        painelPesquisa.Controls.Add(iconePesquisa);

        txtPesquisa = new TextBox
        {
            PlaceholderText = "Buscar pedido, cliente ou produto...",
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 9),
            ForeColor = corTexto,
            BackColor = Color.White,
            Location = new Point(42, 12),
            Width = 245
        };

        painelPesquisa.Controls.Add(txtPesquisa);

        // STATUS

        painelStatus = CriarCampoArredondado(
            painelFiltros, 312, 12, 175, 40
        );

        cmbStatus = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9),
            ForeColor = corTexto,
            BackColor = Color.White,
            Location = new Point(10, 8),
            Width = 155
        };

        cmbStatus.Items.AddRange(new object[]
        {
            "Todos os status",
            "Aguardando pagamento",
            "Em andamento",
            "Pago",
            "Cancelado"
        });

        cmbStatus.SelectedIndex = 0;
        painelStatus.Controls.Add(cmbStatus);

        // DATA INICIAL

        painelDataInicio = CriarCampoArredondado(
            painelFiltros, 499, 12, 155, 40
        );

        CriarCampoData(painelDataInicio, true);

        // DATA FINAL

        painelDataFim = CriarCampoArredondado(
            painelFiltros, 666, 12, 155, 40
        );

        CriarCampoData(painelDataFim, false);

        // LIMPAR

        btnLimparFiltros = new Button
        {
            Text = "Limpar",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = corPrimaria,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Size = new Size(65, 40),
            Location = new Point(833, 12)
        };

        btnLimparFiltros.FlatAppearance.BorderSize = 0;

        btnLimparFiltros.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(247, 249, 255);

        painelFiltros.Controls.Add(btnLimparFiltros);

        // EVENTOS

        txtPesquisa.TextChanged +=
            (_, _) => CarregarPedidos();

        cmbStatus.SelectedIndexChanged +=
            (_, _) => CarregarPedidos();

        btnLimparFiltros.Click +=
            (_, _) => LimparFiltros();

        painelFiltros.Resize +=
            (_, _) => AjustarFiltros();

        AjustarFiltros();
    }

    // ==========================================
    // CAMPO DE DATA PERSONALIZADO
    // ==========================================

    private void CriarCampoData(Panel painel, bool inicial)
    {
        IconPictureBox icone = new IconPictureBox
        {
            IconChar = IconChar.CalendarDays,
            IconColor = corSecundaria,
            IconSize = 15,
            Size = new Size(19, 19),
            Location = new Point(11, 11),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        painel.Controls.Add(icone);

        Label data = new Label
        {
            Text = (inicial ? dataInicial : dataFinal)
                .ToString("dd/MM/yyyy"),

            Font = new Font("Segoe UI", 9),
            ForeColor = corTexto,
            BackColor = Color.Transparent,

            Location = new Point(35, 0),
            Size = new Size(90, 40),

            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand
        };

        painel.Controls.Add(data);

        IconPictureBox seta = new IconPictureBox
        {
            IconChar = IconChar.ChevronDown,
            IconColor = corSecundaria,
            IconSize = 12,
            Size = new Size(16, 16),
            Location = new Point(130, 13),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        painel.Controls.Add(seta);

        if (inicial)
            lblDataInicio = data;
        else
            lblDataFim = data;

        painel.Cursor = Cursors.Hand;

        painel.Click += (_, _) =>
            AbrirCalendario(painel, inicial);

        icone.Click += (_, _) =>
            AbrirCalendario(painel, inicial);

        data.Click += (_, _) =>
            AbrirCalendario(painel, inicial);

        seta.Click += (_, _) =>
            AbrirCalendario(painel, inicial);
    }

    // ==========================================
    // CALENDÁRIO POP-UP
    // ==========================================

    private void AbrirCalendario(Control campo, bool inicial)
    {
        Form popup = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            BackColor = Color.White,
            TopMost = true
        };

        MonthCalendar calendario = new MonthCalendar
        {
            MaxSelectionCount = 1,
            ShowToday = true,
            ShowTodayCircle = true,

            SelectionStart = inicial ? dataInicial : dataFinal,
            SelectionEnd = inicial ? dataInicial : dataFinal,

            Location = new Point(8, 8)
        };

        popup.Controls.Add(calendario);

        popup.ClientSize = new Size(
            calendario.Width + 16,
            calendario.Height + 16
        );

        Point posicao = campo.PointToScreen(
            new Point(0, campo.Height + 5)
        );

        Rectangle tela =
            Screen.FromControl(campo).WorkingArea;

        int x = Math.Min(
            posicao.X,
            tela.Right - popup.Width
        );

        int y = posicao.Y;

        if (y + popup.Height > tela.Bottom)
        {
            y = campo.PointToScreen(
                new Point(0, -popup.Height - 5)
            ).Y;
        }

        popup.Location = new Point(
            Math.Max(tela.Left, x),
            Math.Max(tela.Top, y)
        );

        calendario.DateSelected += (_, e) =>
        {
            if (inicial)
            {
                dataInicial = e.Start.Date;
                filtrarDataInicio = true;

                lblDataInicio.Text =
                    dataInicial.ToString("dd/MM/yyyy");
            }
            else
            {
                dataFinal = e.Start.Date;
                filtrarDataFim = true;

                lblDataFim.Text =
                    dataFinal.ToString("dd/MM/yyyy");
            }

            popup.Close();
            CarregarPedidos();
        };

        popup.Deactivate += (_, _) =>
        {
            if (!popup.IsDisposed)
                popup.Close();
        };

        popup.Show(this);
        popup.Activate();
    }

    // ==========================================
    // CAMPOS ARREDONDADOS
    // ==========================================

    private Panel CriarCampoArredondado(
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
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            Rectangle area = new Rectangle(
                0,
                0,
                painel.Width - 1,
                painel.Height - 1
            );

            using GraphicsPath caminho =
                CriarCaminhoArredondado(area, 10);

            using Pen caneta = new Pen(corBorda, 1);

            e.Graphics.DrawPath(caneta, caminho);
        };

        pai.Controls.Add(painel);

        return painel;
    }

    private static GraphicsPath CriarCaminhoArredondado(
        Rectangle area,
        int raio)
    {
        GraphicsPath caminho = new GraphicsPath();

        int d = raio * 2;

        caminho.AddArc(
            area.X, area.Y, d, d, 180, 90);

        caminho.AddArc(
            area.Right - d, area.Y, d, d, 270, 90);

        caminho.AddArc(
            area.Right - d,
            area.Bottom - d,
            d, d, 0, 90
        );

        caminho.AddArc(
            area.X,
            area.Bottom - d,
            d, d, 90, 90
        );

        caminho.CloseFigure();

        return caminho;
    }

    // ==========================================
    // FILTROS RESPONSIVOS
    // ==========================================

    private void AjustarFiltros()
    {
        if (painelPesquisa == null ||
            painelStatus == null ||
            painelDataInicio == null ||
            painelDataFim == null ||
            btnLimparFiltros == null ||
            txtPesquisa == null)
        {
            return;
        }

        int largura = painelFiltros.ClientSize.Width;

        // Os filtros permanecem na mesma linha
        // quando existe espaço suficiente.

        if (largura >= 910)
        {
            cardPedidos.RowStyles[0].Height = 72;

            painelPesquisa.Location = new Point(0, 12);
            painelPesquisa.Width = largura - 598;

            txtPesquisa.Width =
                painelPesquisa.Width - 55;

            painelStatus.Location = new Point(
                painelPesquisa.Right + 12, 12);

            painelDataInicio.Location = new Point(
                painelStatus.Right + 12, 12);

            painelDataFim.Location = new Point(
                painelDataInicio.Right + 12, 12);

            btnLimparFiltros.Location = new Point(
                painelDataFim.Right + 12, 12);
        }
        else
        {
            cardPedidos.RowStyles[0].Height = 120;

            painelPesquisa.Location = new Point(0, 10);

            painelPesquisa.Width =
                Math.Max(250, largura - 190);

            txtPesquisa.Width =
                painelPesquisa.Width - 55;

            painelStatus.Location = new Point(
                painelPesquisa.Right + 12, 10);

            painelDataInicio.Location =
                new Point(0, 64);

            painelDataFim.Location =
                new Point(167, 64);

            btnLimparFiltros.Location =
                new Point(334, 64);
        }

        painelPesquisa.Invalidate();
        painelStatus.Invalidate();
        painelDataInicio.Invalidate();
        painelDataFim.Invalidate();
    }

    // ==========================================
    // LIMPAR FILTROS
    // ==========================================

    private void LimparFiltros()
    {
        txtPesquisa.Clear();

        cmbStatus.SelectedIndex = 0;

        dataInicial = DateTime.Today.AddMonths(-1);
        dataFinal = DateTime.Today;

        filtrarDataInicio = false;
        filtrarDataFim = false;

        lblDataInicio.Text =
            dataInicial.ToString("dd/MM/yyyy");

        lblDataFim.Text =
            dataFinal.ToString("dd/MM/yyyy");

        CarregarPedidos();
    }

    // ==========================================
    // MENU LATERAL
    // ==========================================

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
                Location = new Point(15, 20),
                Size = new Size(180, 55),
                BackColor = corMenu
            };

            menu.Controls.Add(logo);
        }

        Label descricao = new Label
        {
            Text = "Gestão de vendas",
            ForeColor = Color.FromArgb(160, 175, 200),
            Font = new Font("Segoe UI", 8.5f),
            Location = new Point(30, 77),
            AutoSize = true
        };

        menu.Controls.Add(descricao);

        int y = 125;

        foreach (var item in new
            (IconChar Icone, string Texto)[]
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
                item.Texto == "Pedidos"
            );

            y += 49;
        }

        IconButton configuracoes = CriarBotaoMenu(
            menu,
            IconChar.Gear,
            "Configurações",
            0,
            false
        );

        configuracoes.Dock = DockStyle.Bottom;
        configuracoes.Height = 55;
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
            Location = new Point(12, y),
            Size = new Size(186, 41),
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
            IconSize = 18,
            Location = new Point(16, 10),
            Size = new Size(22, 22),
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
                9,
                selecionado
                    ? FontStyle.Bold
                    : FontStyle.Regular
            ),

            Location = new Point(46, 0),
            Size = new Size(130, 41),
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand
        };

        imagemIcone.Click +=
            (_, _) => botao.PerformClick();

        textoMenu.Click +=
            (_, _) => botao.PerformClick();

        if (!selecionado)
        {
            botao.MouseEnter +=
                (_, _) => botao.BackColor = corMenuHover;

            botao.MouseLeave +=
                (_, _) => botao.BackColor = corMenu;
        }

        botao.Controls.Add(imagemIcone);
        botao.Controls.Add(textoMenu);

        menu.Controls.Add(botao);

        ArredondarControle(botao, 6);

        return botao;
    }

    // ==========================================
    // BARRA DE TÍTULO
    // ==========================================

    private Panel CriarBarraTitulo()
    {
        Panel barra = new Panel
        {
            Height = 25,
            BackColor = corBarra,
            Margin = Padding.Empty
        };

        IconPictureBox icone = new IconPictureBox
        {
            IconChar = IconChar.CartShopping,
            IconColor = Color.White,
            IconSize = 17,
            Size = new Size(20, 20),
            Location = new Point(15, 4),
            BackColor = corBarra
        };

        barra.Controls.Add(icone);

        Label titulo = new Label
        {
            Text = "VendeAí | Pedidos",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(43, 4),
            BackColor = corBarra
        };

        barra.Controls.Add(titulo);

        // Botões posicionados da direita para a esquerda.

        Button fechar = CriarBotaoJanela("✕", "Fechar");
        Button maximizar = CriarBotaoJanela("□", "Maximizar");
        Button minimizar = CriarBotaoJanela("─", "Minimizar");

        fechar.Click += (_, _) => Close();

        minimizar.Click += (_, _) =>
            WindowState = FormWindowState.Minimized;

        maximizar.Click += (_, _) =>
            AlternarMaximizacao();

        barra.Controls.Add(minimizar);
        barra.Controls.Add(maximizar);
        barra.Controls.Add(fechar);

        void PosicionarBotoes()
        {
            fechar.Location =
                new Point(barra.ClientSize.Width - 46, 0);

            maximizar.Location =
                new Point(barra.ClientSize.Width - 92, 0);

            minimizar.Location =
                new Point(barra.ClientSize.Width - 138, 0);
        }

        barra.Resize += (_, _) => PosicionarBotoes();

        SizeChanged += (_, _) =>
        {
            maximizar.Text =
                WindowState == FormWindowState.Maximized
                    ? "❐"
                    : "□";
        };

        barra.MouseDown += IniciarArrasteJanela;
        titulo.MouseDown += IniciarArrasteJanela;
        icone.MouseDown += IniciarArrasteJanela;

        barra.DoubleClick +=
            (_, _) => AlternarMaximizacao();

        return barra;
    }

    private Button CriarBotaoJanela(
        string simbolo,
        string nome)
    {
        Button botao = new Button
        {
            Name = nome,
            Text = simbolo,
            Size = new Size(46, 25),
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
        SendMessage(Handle, 0xA1, 0x2, 0);
    }

    // ==========================================
    // BOTÕES
    // ==========================================

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
                9,
                FontStyle.Bold
            ),

            Cursor = Cursors.Hand
        };

        botao.FlatAppearance.BorderSize =
            primario ? 0 : 1;

        botao.FlatAppearance.BorderColor = corPrimaria;

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
            CriarCaminhoArredondado(
                new Rectangle(
                    0,
                    0,
                    controle.Width,
                    controle.Height
                ),
                raio
            );

        controle.Region = new Region(caminho);
    }

    // ==========================================
    // COLUNAS DE AÇÃO
    // ==========================================

    private void AdicionarColunaAcao(string nome)
    {
        DataGridViewTextBoxColumn coluna =
            new DataGridViewTextBoxColumn
            {
                Name = nome,
                HeaderText = "",
                ReadOnly = true,

                SortMode =
                    DataGridViewColumnSortMode.NotSortable
            };

        tabelaPedidos.Columns.Add(coluna);
    }

    // ==========================================
    // LISTAGEM E FILTROS
    // ==========================================

    private void CarregarPedidos()
    {
        if (tabelaPedidos == null ||
            tabelaPedidos.IsDisposed ||
            txtPesquisa == null ||
            cmbStatus == null ||
            lblTotal == null)
        {
            return;
        }

        string pesquisa = txtPesquisa.Text.Trim();

        var pedidos = pedidoController
            .ListarPedidos()
            .Where(p =>
                p.Codigo.ToString().Contains(
                    pesquisa,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                p.Cliente.Nome.Contains(
                    pesquisa,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                p.Itens.Any(item =>
                    item.Produto.Nome.Contains(
                        pesquisa,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            );

        // FILTRO POR STATUS

        if (cmbStatus.SelectedIndex > 0)
        {
            StatusPedido status =
                cmbStatus.SelectedIndex switch
                {
                    1 => StatusPedido.AguardandoPagamento,
                    2 => StatusPedido.EmAndamento,
                    3 => StatusPedido.Pago,
                    4 => StatusPedido.Cancelado,
                    _ => StatusPedido.EmAndamento
                };

            pedidos = pedidos.Where(
                p => p.Status == status
            );
        }

        // FILTRO POR DATA INICIAL

        if (filtrarDataInicio)
        {
            pedidos = pedidos.Where(
                p => p.DataPedido.Date >= dataInicial.Date
            );
        }

        // FILTRO POR DATA FINAL

        if (filtrarDataFim)
        {
            pedidos = pedidos.Where(
                p => p.DataPedido.Date <= dataFinal.Date
            );
        }

        var resultado = pedidos
            .OrderByDescending(p => p.DataPedido)
            .ToList();

        tabelaPedidos.Rows.Clear();

        foreach (Pedido pedido in resultado)
        {
            int indice = tabelaPedidos.Rows.Add(
                $"#{pedido.Codigo:000000}",
                pedido.DataPedido.ToString("dd/MM/yyyy HH:mm"),
                pedido.Cliente.Nome,
                pedido.Itens.Count,
                pedido.ValorTotal.ToString("C2", culturaBrasil),
                TextoStatus(pedido.Status),
                TextoFormaPagamento(pedido.FormaPagamento),
                "",
                "",
                ""
            );

            tabelaPedidos.Rows[indice].Tag =
                pedido.Codigo;
        }

        lblTotal.Text =
            $"{resultado.Count} pedido(s) encontrado(s)";
    }

    private static string TextoStatus(
        StatusPedido status)
    {
        return status switch
        {
            StatusPedido.AguardandoPagamento =>
                "Aguardando pagamento",

            StatusPedido.EmAndamento =>
                "Em andamento",

            StatusPedido.Pago =>
                "Pago",

            StatusPedido.Cancelado =>
                "Cancelado",

            _ => status.ToString()
        };
    }

    private static string TextoFormaPagamento(
        FormaPagamento? forma)
    {
        if (!forma.HasValue)
            return "Não informado";

        return forma.Value switch
        {
            FormaPagamento.Pix =>
                "Pix",

            FormaPagamento.Dinheiro =>
                "Dinheiro",

            FormaPagamento.CartaoCredito =>
                "Cartão de crédito",

            FormaPagamento.CartaoDebito =>
                "Cartão de débito",

            FormaPagamento.Boleto =>
                "Boleto",

            _ => forma.Value.ToString()
        };
    }

    // ==========================================
    // FORMATAÇÃO DA TABELA
    // ==========================================

    private void TabelaPedidos_CellFormatting(
        object? sender,
        DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0)
        {
            return;
        }

        string nomeColuna =
            tabelaPedidos.Columns[e.ColumnIndex].Name;

        if (nomeColuna == "Status")
        {
            string status =
                e.Value?.ToString() ?? "";

            e.CellStyle.ForeColor = status switch
            {
                "Pago" =>
                    Color.FromArgb(22, 130, 80),

                "Cancelado" =>
                    Color.FromArgb(190, 45, 55),

                "Em andamento" =>
                    corPrimaria,

                _ =>
                    Color.FromArgb(180, 115, 25)
            };

            e.CellStyle.Font = new Font(
                "Segoe UI",
                8.5f,
                FontStyle.Bold
            );
        }
    }

    private void TabelaPedidos_CellPainting(
        object? sender,
        DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0)
        {
            return;
        }

        string coluna =
            tabelaPedidos.Columns[e.ColumnIndex].Name;

        IconChar icone;
        Color corIcone;

        if (coluna == "Visualizar")
        {
            icone = IconChar.Eye;
            corIcone = corSecundaria;
        }
        else if (coluna == "Editar")
        {
            icone = IconChar.Pen;
            corIcone = corPrimaria;
        }
        else if (coluna == "Excluir")
        {
            icone = IconChar.Trash;
            corIcone = Color.FromArgb(190, 45, 55);
        }
        else
        {
            return;
        }

        e.PaintBackground(e.CellBounds, true);

        using Bitmap imagem =
            icone.ToBitmap(corIcone, 16);

        int x = e.CellBounds.X +
            (e.CellBounds.Width - imagem.Width) / 2;

        int y = e.CellBounds.Y +
            (e.CellBounds.Height - imagem.Height) / 2;

        e.Graphics.DrawImage(imagem, x, y);

        e.Handled = true;
    }

    private void TabelaPedidos_CellToolTipTextNeeded(
        object? sender,
        DataGridViewCellToolTipTextNeededEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0)
        {
            return;
        }

        string coluna =
            tabelaPedidos.Columns[e.ColumnIndex].Name;

        e.ToolTipText = coluna switch
        {
            "Visualizar" => "Visualizar pedido",
            "Editar" => "Editar pedido",
            "Excluir" => "Excluir pedido",
            _ => ""
        };
    }

    // ==========================================
    // CADASTRO
    // ==========================================

    private void AbrirCadastro()
    {
        using var formulario = new CadastrarPedidoForm();

        if (formulario.ShowDialog(this) == DialogResult.OK)
        {
            CarregarPedidos();
        }
    }

    // ==========================================
    // AÇÕES DA TABELA
    // ==========================================

    private void TabelaPedidos_CellContentClick(
        object? sender,
        DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0)
        {
            return;
        }

        string coluna =
            tabelaPedidos.Columns[e.ColumnIndex].Name;

        if (coluna != "Excluir" &&
            coluna != "Editar" &&
            coluna != "Visualizar")
        {
            return;
        }

        if (tabelaPedidos.Rows[e.RowIndex].Tag
            is not int codigo)
        {
            return;
        }

        if (coluna == "Excluir")
        {
            DialogResult confirmacao = MessageBox.Show(
                $"Deseja realmente excluir o pedido #{codigo:000000}?",
                "Confirmar exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmacao != DialogResult.Yes)
                return;

            try
            {
                pedidoController.ExcluirPedido(codigo);

                CarregarPedidos();

                MessageBox.Show(
                    "Pedido excluído com sucesso!",
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
        else
        {
            Pedido? pedido =
                pedidoController.BuscarPedidoPorCodigo(codigo);

            if (pedido == null)
            {
                MessageBox.Show(
                    "Pedido não encontrado.",
                    "VendeAí",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string acao =
                coluna == "Editar"
                    ? "edição"
                    : "visualização";

            MessageBox.Show(
                $"A tela de {acao} do pedido #{codigo:000000} será implementada na próxima etapa.",
                "VendeAí",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}
