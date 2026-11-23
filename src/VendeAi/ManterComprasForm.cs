using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using FontAwesome.Sharp;
using VendeAi.Models;

namespace VendeAi;

public class ManterComprasForm : Form
{
    private readonly Color azul = Color.FromArgb(37, 74, 235);
    private readonly Color marinho = Color.FromArgb(15, 30, 60);
    private readonly Color fundo = Color.FromArgb(245, 247, 251);
    private readonly Color texto = Color.FromArgb(20, 30, 60);
    private readonly Color borda = Color.FromArgb(220, 225, 235);
    private readonly Color cinzaIcone = Color.FromArgb(103, 118, 146);
    private readonly CultureInfo br = CultureInfo.GetCultureInfo("pt-BR");

    // Cores dos "badges" de Categoria e Status (fundo, texto)
    private static readonly Dictionary<string, (Color Fundo, Color Texto)> coresBadge = new()
    {
        ["Roupas"]     = (Color.FromArgb(219, 234, 254), Color.FromArgb(37, 99, 235)),
        ["Cosméticos"] = (Color.FromArgb(252, 231, 243), Color.FromArgb(219, 39, 119)),
        ["Acessórios"] = (Color.FromArgb(237, 233, 254), Color.FromArgb(109, 40, 217)),
        ["Calçados"]   = (Color.FromArgb(220, 252, 231), Color.FromArgb(22, 128, 60)),
        ["Aberto"]     = (Color.FromArgb(224, 242, 254), Color.FromArgb(3, 105, 161)),
        ["Solicitado"] = (Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9)),
        ["Fechado"]    = (Color.FromArgb(220, 252, 231), Color.FromArgb(22, 128, 60)),
        ["Entregue"]   = (Color.FromArgb(237, 233, 254), Color.FromArgb(109, 40, 217)),
    };

    private readonly Dictionary<string, string> statusCompras = new();
    private List<LinhaCompra> linhas = new();
    private List<LinhaCompra> filtradas = new();
    private int pagina = 1;
    private const int PorPagina = 10;

    private Panel conteudo = null!;
    private FlowLayoutPanel areaIndicadores = null!;
    private DataGridView tabela = null!;
    private TextBox busca = null!;
    private ComboBox categoria = null!;
    private ComboBox status = null!;
    private DateTimePicker inicio = null!;
    private DateTimePicker fim = null!;
    private Label rodape = null!;
    private Label paginaAtual = null!;
    private Button anterior = null!;
    private Button proximo = null!;

    private sealed class LinhaCompra
    {
        public string Chave { get; set; } = "";
        public string Produto { get; set; } = "";
        public string Variacao { get; set; } = "";
        public string Categoria { get; set; } = "";
        public int Quantidade { get; set; }
        public string Status { get; set; } = "Aberto";
    }

    public ManterComprasForm()
    {
        Text = "VendeAí | Compras";
        Font = new Font("Segoe UI", 10);
        FormBorderStyle = FormBorderStyle.None;
        BackColor = fundo;
        MinimumSize = new Size(1180, 720);
        WindowState = FormWindowState.Maximized;
        MaximizedBounds = Screen.FromControl(this).WorkingArea;
        MontarTela();
        Shown += (_, _) => AtualizarDados();
    }

    private Panel painelPrincipal = null!;
    private Panel cardTabela = null!;
    private Panel painelFiltros = null!;
    private Label tituloPagina = null!;
    private Label subtituloPagina = null!;

    private void MontarTela()
    {
        SuspendLayout();
        var area = new Panel { Dock = DockStyle.Fill, BackColor = fundo };
        Controls.Add(area);

        var menu = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = marinho };
        area.Controls.Add(menu);
        var caminhoLogo = Path.Combine(AppContext.BaseDirectory, "Assets", "logo-vendeai.png");
        if (File.Exists(caminhoLogo))
        {
            var logo = new PictureBox
            {
                Image = Image.FromFile(caminhoLogo), SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(20, 24), Size = new Size(190, 65), BackColor = marinho
            };
            menu.Controls.Add(logo);
        }
        else
        {
            menu.Controls.Add(new Label
            {
                Text = "🛒 VendeAí", ForeColor = Color.White,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                Location = new Point(22, 32), AutoSize = true
            });
        }
        menu.Controls.Add(new Label
        {
            Text = "Gestão de vendas", ForeColor = Color.FromArgb(160, 175, 200),
            Font = new Font("Segoe UI", 9), Location = new Point(34, 86), AutoSize = true
        });

        int menuY = 135;
        foreach (var item in new (IconChar Icone, string Nome)[]
        {
            (IconChar.House, "Painel"), (IconChar.Box, "Produtos"),
            (IconChar.Users, "Clientes"), (IconChar.ClipboardList, "Pedidos"),
            (IconChar.CartShopping, "Compras"), (IconChar.BoxesStacked, "Separação"),
            (IconChar.ShieldHalved, "Permissões")
        })
        {
            var destino = item.Nome;
            var botao = CriarBotaoMenu(menu, item.Icone, destino, menuY, destino == "Compras");
            botao.Click += (_, _) => Navegar(destino);
            menuY += 55;
        }
        var configuracoes = CriarBotaoMenu(menu, IconChar.Gear, "Configurações", 0, false);
        configuracoes.Dock = DockStyle.Bottom;
        configuracoes.Height = 60;

        painelPrincipal = new Panel { Dock = DockStyle.Fill, BackColor = fundo, AutoScroll = false };
        area.Controls.Add(painelPrincipal);
        painelPrincipal.BringToFront();

        var topo = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White };
        painelPrincipal.Controls.Add(topo);
        var usuario = new Label
        {
            Text = "Mayumi  ▾", ForeColor = texto,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true
        };
        topo.Controls.Add(usuario);
        topo.Resize += (_, _) => usuario.Location =
            new Point(Math.Max(20, topo.ClientSize.Width - usuario.Width - 35), 29);

        conteudo = new Panel { BackColor = fundo, Location = new Point(25, 76) };
        painelPrincipal.Controls.Add(conteudo);
        var iconeTitulo = new IconPictureBox
        {
            IconChar = IconChar.CartShopping,
            IconColor = azul,
            IconSize = 29,
            Size = new Size(34, 36),
            Location = new Point(0, 8),
            BackColor = fundo,
            TabStop = false
        };
        conteudo.Controls.Add(iconeTitulo);
        tituloPagina = new Label
        {
            Text = "Compras", Location = new Point(46, 0), AutoSize = true,
            Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = texto
        };
        conteudo.Controls.Add(tituloPagina);
        // Ícone alinhado ao centro visual do texto "Compras" (ajuste fino: mude o 10)
        iconeTitulo.Location = new Point(0, 10);
        subtituloPagina = new Label
        {
            Text = "Lista consolidada de produtos dos pedidos pagos ou concluídos para compra e separação.",
            Location = new Point(48, 52), AutoSize = true,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(96, 108, 133)
        };
        conteudo.Controls.Add(subtituloPagina);

        // Filtros em uma única linha, com rótulos acima (conforme protótipo).
        painelFiltros = new Panel { Location = new Point(0, 100), Height = 70, BackColor = fundo };
        conteudo.Controls.Add(painelFiltros);

        AdicionarRotuloFiltro("Período dos pedidos", 2);
        AdicionarRotuloFiltro("Categoria", 346);
        AdicionarRotuloFiltro("Status do produto", 533);

        inicio = CriarData(0, 22, DateTime.Today.AddMonths(-1));
        fim = CriarData(166, 22, DateTime.Today);
        categoria = CriarCombo(344, 22, 175, "Todas", "Roupas", "Cosméticos", "Acessórios", "Calçados");
        status = CriarCombo(531, 22, 175, "Todos", "Aberto", "Solicitado", "Fechado", "Entregue");

        // IMPORTANTE: a posição do TextBox é relativa à caixa (Panel) onde ele está.
        busca = new TextBox
        {
            PlaceholderText = "Buscar por produto ou variação...",
            Location = new Point(40, 12), Size = new Size(250, 24),
            BorderStyle = BorderStyle.None, BackColor = Color.White,
            Font = new Font("Segoe UI", 10)
        };
        var caixaBusca = CaixaFiltro(714, 22, 301, 44);
        caixaBusca.Controls.Add(new IconPictureBox
        {
            IconChar = IconChar.MagnifyingGlass, IconColor = cinzaIcone,
            IconSize = 17, Size = new Size(20, 20), Location = new Point(12, 12),
            BackColor = Color.White, TabStop = false
        });
        caixaBusca.Controls.Add(busca);
        painelFiltros.Controls.Add(caixaBusca);

        areaIndicadores = new FlowLayoutPanel
        {
            Location = new Point(0, 182), Height = 93,
            WrapContents = false, BackColor = fundo, AutoScroll = false
        };
        conteudo.Controls.Add(areaIndicadores);

        cardTabela = new Panel { Location = new Point(0, 285), BackColor = Color.White };
        conteudo.Controls.Add(cardTabela);
        cardTabela.Controls.Add(new Label
        {
            Text = "Produtos para compra/separação", Location = new Point(22, 18),
            AutoSize = true, ForeColor = texto,
            Font = new Font("Segoe UI", 14, FontStyle.Bold)
        });
        tabela = new DataGridView
        {
            Location = new Point(20, 60), BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None, RowHeadersVisible = false,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false, ReadOnly = true,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false, ColumnHeadersHeight = 46,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowTemplate = { Height = 46 }, GridColor = Color.FromArgb(235, 239, 246),
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            ScrollBars = ScrollBars.Vertical
        };
        tabela.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 251);
        tabela.ColumnHeadersDefaultCellStyle.ForeColor = texto;
        tabela.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        tabela.ColumnHeadersDefaultCellStyle.Padding = new Padding(9, 0, 0, 0);
        tabela.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 247, 251);
        tabela.ColumnHeadersDefaultCellStyle.SelectionForeColor = texto;
        tabela.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        tabela.DefaultCellStyle.ForeColor = texto;
        tabela.DefaultCellStyle.Padding = new Padding(9, 0, 0, 0);
        tabela.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 241, 255);
        tabela.DefaultCellStyle.SelectionForeColor = texto;
        tabela.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);
        tabela.Columns.Add("Produto", "Produto");
        tabela.Columns.Add("Variacao", "Variação");
        tabela.Columns.Add("Categoria", "Categoria");
        tabela.Columns.Add("Quantidade", "Quantidade total");
        tabela.Columns.Add("Status", "Status");
        tabela.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "Editar", HeaderText = "Ações", Text = "✎",
            UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat
        });
        tabela.Columns["Produto"]!.FillWeight = 155;
        tabela.Columns["Variacao"]!.FillWeight = 120;
        tabela.Columns["Categoria"]!.FillWeight = 105;
        tabela.Columns["Quantidade"]!.FillWeight = 100;
        tabela.Columns["Status"]!.FillWeight = 105;
        tabela.Columns["Editar"]!.FillWeight = 80;

        // Colunas centralizadas (cabeçalho e conteúdo), como no protótipo
        foreach (var nome in new[] { "Categoria", "Quantidade", "Status", "Editar" })
        {
            var col = tabela.Columns[nome]!;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.DefaultCellStyle.Padding = Padding.Empty;
            col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            col.HeaderCell.Style.Padding = Padding.Empty;
        }

        tabela.CellPainting += PintarCelula;
        tabela.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == tabela.Columns["Editar"]!.Index)
                EditarStatus(e.RowIndex);
        };
        cardTabela.Controls.Add(tabela);

        rodape = new Label { AutoSize = true, ForeColor = Color.FromArgb(100, 110, 130) };
        cardTabela.Controls.Add(rodape);
        anterior = Botao("‹", 42, 34, false);
        proximo = Botao("›", 42, 34, false);
        paginaAtual = new Label
        {
            Size = new Size(38, 34), TextAlign = ContentAlignment.MiddleCenter,
            BackColor = azul, ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        cardTabela.Controls.Add(anterior);
        cardTabela.Controls.Add(proximo);
        cardTabela.Controls.Add(paginaAtual);
        anterior.Click += (_, _) => { if (pagina > 1) { pagina--; PreencherTabela(); } };
        proximo.Click += (_, _) =>
        {
            if (pagina * PorPagina < filtradas.Count) { pagina++; PreencherTabela(); }
        };
        inicio.ValueChanged += (_, _) => AtualizarDados();
        fim.ValueChanged += (_, _) => AtualizarDados();
        categoria.SelectedIndexChanged += (_, _) => Filtrar();
        status.SelectedIndexChanged += (_, _) => Filtrar();
        busca.TextChanged += (_, _) => Filtrar();

        var barra = CriarBarraTitulo();
        Controls.Add(barra);
        barra.BringToFront();
        painelPrincipal.Resize += (_, _) => Reposicionar();
        Shown += (_, _) => Reposicionar();
        Reposicionar();
        ResumeLayout(true);
    }

    // Desenha os "badges" coloridos (Categoria/Status) e o botão de editar.
    private void PintarCelula(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
        string col = tabela.Columns[e.ColumnIndex].Name;

        if (col == "Categoria" || col == "Status")
        {
            e.PaintBackground(e.CellBounds, true);
            string valor = e.Value?.ToString() ?? "";
            if (coresBadge.TryGetValue(valor, out var cor))
            {
                var fonte = tabela.DefaultCellStyle.Font!;
                var tam = TextRenderer.MeasureText(e.Graphics, valor, fonte);
                int larguraBadge = tam.Width + 22;
                var r = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - larguraBadge) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - 26) / 2,
                    larguraBadge, 26);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var caminho = RetanguloArredondado(r, 8);
                using var pincel = new SolidBrush(cor.Fundo);
                e.Graphics.FillPath(pincel, caminho);
                TextRenderer.DrawText(e.Graphics, valor, fonte, r, cor.Texto,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.NoPadding);
            }
            e.Handled = true;
        }
        else if (col == "Editar")
        {
            e.PaintBackground(e.CellBounds, true);
            var r = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - 42) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - 30) / 2, 42, 30);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var caminho = RetanguloArredondado(r, 6);
            using var caneta = new Pen(azul, 1.4f);
            using var fonteIcone = new Font("Segoe UI Symbol", 11, FontStyle.Bold);
            e.Graphics.DrawPath(caneta, caminho);
            TextRenderer.DrawText(e.Graphics, "✎", fonteIcone, r, azul,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.NoPadding);
            e.Handled = true;
        }
    }

    private static GraphicsPath RetanguloArredondado(Rectangle r, int raio)
    {
        int d = raio * 2;
        var p = new GraphicsPath();
        p.AddArc(r.Left, r.Top, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Top, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    private IconButton CriarBotaoMenu(Panel menu, IconChar icone, string nome, int y, bool ativo)
    {
        var botao = new IconButton
        {
            Text = "", IconChar = IconChar.None, Location = new Point(15, y),
            Size = new Size(200, 45), BackColor = ativo ? azul : marinho,
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand, TabStop = true
        };
        if (ativo)
        {
            void ArredondarDestaque()
            {
                if (botao.Width <= 0 || botao.Height <= 0) return;
                const int raio = 8;
                using var caminho = new GraphicsPath();
                int d = raio * 2;
                var r = new Rectangle(0, 0, botao.Width - 1, botao.Height - 1);
                caminho.AddArc(r.Left, r.Top, d, d, 180, 90);
                caminho.AddArc(r.Right - d, r.Top, d, d, 270, 90);
                caminho.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                caminho.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
                caminho.CloseFigure();
                var regiaoAnterior = botao.Region;
                botao.Region = new Region(caminho);
                regiaoAnterior?.Dispose();
            }
            botao.SizeChanged += (_, _) => ArredondarDestaque();
            ArredondarDestaque();
        }
        botao.FlatAppearance.BorderSize = 0;
        botao.FlatAppearance.MouseOverBackColor = ativo ? azul : Color.FromArgb(25, 45, 82);
        var figura = new IconPictureBox
        {
            IconChar = icone, IconColor = Color.White, IconSize = 21,
            Location = new Point(17, 11), Size = new Size(24, 24),
            SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        var legenda = new Label
        {
            Text = nome, ForeColor = Color.White, BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 10, ativo ? FontStyle.Bold : FontStyle.Regular),
            Location = new Point(48, 0), Size = new Size(145, 45),
            TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand
        };
        figura.Click += (_, _) => botao.PerformClick();
        legenda.Click += (_, _) => botao.PerformClick();
        botao.Controls.Add(figura);
        botao.Controls.Add(legenda);
        menu.Controls.Add(botao);
        return botao;
    }

    private Panel CriarBarraTitulo()
    {
        var barra = new Panel { Dock = DockStyle.Top, Height = 25, BackColor = Color.FromArgb(34, 59, 96) };
        var icone = new IconPictureBox
        {
            IconChar = IconChar.CartShopping, IconColor = Color.White,
            IconSize = 17, Size = new Size(20, 20), Location = new Point(15, 4),
            BackColor = barra.BackColor
        };
        barra.Controls.Add(icone);
        var titulo = new Label
        {
            Text = "VendeAí | Compras", ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Location = new Point(43, 5), AutoSize = true
        };
        barra.Controls.Add(titulo);
        Button BotaoJanela(string textoBotao, string nome)
        {
            var botao = new Button
            {
                Name = nome, Text = textoBotao, Dock = DockStyle.Right, Width = 46,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                BackColor = barra.BackColor, Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand, TabStop = false
            };
            botao.FlatAppearance.BorderSize = 0;
            botao.FlatAppearance.MouseOverBackColor = nome == "Fechar"
                ? Color.FromArgb(190, 40, 55) : Color.FromArgb(25, 45, 82);
            return botao;
        }
        var fechar = BotaoJanela("✕", "Fechar");
        var maximizar = BotaoJanela("□", "Maximizar");
        var minimizar = BotaoJanela("─", "Minimizar");
        fechar.Click += (_, _) => Close();
        minimizar.Click += (_, _) => WindowState = FormWindowState.Minimized;
        maximizar.Click += (_, _) => AlternarMaximizacao();
        SizeChanged += (_, _) => maximizar.Text =
            WindowState == FormWindowState.Maximized ? "❐" : "□";
        barra.Controls.Add(minimizar);
        barra.Controls.Add(maximizar);
        barra.Controls.Add(fechar);
        barra.MouseDown += IniciarArraste;
        titulo.MouseDown += IniciarArraste;
        icone.MouseDown += IniciarArraste;
        barra.DoubleClick += (_, _) => AlternarMaximizacao();
        titulo.DoubleClick += (_, _) => AlternarMaximizacao();
        return barra;
    }

    private void AlternarMaximizacao()
    {
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal : FormWindowState.Maximized;
    }

    private void IniciarArraste(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || e.Clicks > 1) return;
        ReleaseCapture();
        SendMessage(Handle, 0xA1, 0x2, 0);
    }

    private void AdicionarRotuloFiltro(string nome, int x)
    {
        painelFiltros.Controls.Add(new Label
        {
            Text = nome, Location = new Point(x, 2), AutoSize = true,
            ForeColor = texto, Font = new Font("Segoe UI", 9)
        });
    }

    private void Reposicionar()
    {
        if (conteudo == null || painelPrincipal == null || tabela == null) return;
        int largura = Math.Max(800, painelPrincipal.ClientSize.Width - 50);
        int altura = Math.Max(590, painelPrincipal.ClientSize.Height - 100);
        conteudo.Size = new Size(largura, altura);
        painelFiltros.Width = largura;
        if (busca.Parent is Panel caixaBusca)
        {
            caixaBusca.Width = Math.Max(175, largura - caixaBusca.Left);
            busca.Width = caixaBusca.Width - 52;
        }
        areaIndicadores.Width = largura;
        if (areaIndicadores.Controls.Count == 4)
        {
            int larguraIndicador = Math.Max(190, (largura - 250 - 36) / 3);
            for (int i = 0; i < 3; i++) areaIndicadores.Controls[i].Width = larguraIndicador;
        }
        cardTabela.Width = largura;
        cardTabela.Height = Math.Max(290, altura - cardTabela.Top - 10);
        tabela.Width = largura - 40;
        tabela.Height = Math.Max(145, cardTabela.Height - 130);
        int rodapeY = cardTabela.Height - 49;
        rodape.Location = new Point(22, rodapeY + 9);
        proximo.Location = new Point(largura - 63, rodapeY);
        paginaAtual.Location = new Point(largura - 108, rodapeY);
        anterior.Location = new Point(largura - 157, rodapeY);
    }

    private Panel CaixaFiltro(int x, int y, int largura, int altura)
    {
        var caixa = new Panel
        {
            Location = new Point(x, y), Size = new Size(largura, altura),
            BackColor = Color.White
        };
        caixa.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var caminho = new GraphicsPath();
            int r = 12;
            var rect = new Rectangle(0, 0, caixa.Width - 1, caixa.Height - 1);
            caminho.AddArc(rect.Left, rect.Top, r, r, 180, 90);
            caminho.AddArc(rect.Right - r, rect.Top, r, r, 270, 90);
            caminho.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
            caminho.AddArc(rect.Left, rect.Bottom - r, r, r, 90, 90);
            caminho.CloseFigure();
            using var caneta = new Pen(borda);
            e.Graphics.DrawPath(caneta, caminho);
        };
        return caixa;
    }

    private DateTimePicker CriarData(int x, int y, DateTime valor)
    {
        var caixa = CaixaFiltro(x, y, 155, 44);
        var calendario = new IconPictureBox
        {
            IconChar = IconChar.CalendarDays,
            IconColor = cinzaIcone,
            IconSize = 17,
            BackColor = Color.White,
            Size = new Size(20, 20),
            Location = new Point(11, 12),
            TabStop = false
        };
        caixa.Controls.Add(calendario);

        // O seletor nativo continua funcional (abre o calendário), mas fica ATRÁS
        // do texto e da seta, que o cobrem por completo. Assim ele não desenha
        // borda, seta nem pixels soltos sobre a interface.
        var data = new DateTimePicker
        {
            Location = new Point(34, 8), Size = new Size(108, 29),
            TabStop = false,
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy", Value = valor,
            Font = new Font("Segoe UI", 9),
            CalendarFont = new Font("Segoe UI", 9)
        };
        caixa.Controls.Add(data);

        var textoData = new Label
        {
            Text = valor.ToString("dd/MM/yyyy"),
            Location = new Point(34, 8), Size = new Size(108, 29),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.White, ForeColor = texto,
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand
        };
        var seta = new IconPictureBox
        {
            IconChar = IconChar.ChevronDown,
            IconColor = cinzaIcone,
            IconSize = 13,
            BackColor = Color.White,
            Size = new Size(18, 18),
            Location = new Point(128, 13),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        caixa.Controls.Add(textoData);
        caixa.Controls.Add(seta);
        data.SendToBack();
        textoData.BringToFront();
        seta.BringToFront();

        data.ValueChanged += (_, _) => textoData.Text = data.Value.ToString("dd/MM/yyyy");
        void AbrirCalendario()
        {
            data.Focus();
            SendMessage(data.Handle, 0x0100, 0x73, 0); // WM_KEYDOWN, VK_F4
        }
        textoData.Click += (_, _) => AbrirCalendario();
        seta.Click += (_, _) => AbrirCalendario();
        calendario.Click += (_, _) => AbrirCalendario();
        painelFiltros.Controls.Add(caixa);
        return data;
    }

    private ComboBox CriarCombo(int x, int y, int largura, params string[] opcoes)
    {
        var caixa = CaixaFiltro(x, y, largura, 44);
        var c = new ComboBox
        {
            Location = new Point(11, 10), Width = largura - 22,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9)
        };
        c.Items.AddRange(opcoes.Cast<object>().ToArray());
        c.SelectedIndex = 0;
        caixa.Controls.Add(c);
        painelFiltros.Controls.Add(caixa);
        return c;
    }

    private Button Botao(string nome, int largura, int altura, bool principal)
    {
        var b = new Button
        {
            Text = nome, Size = new Size(largura, altura),
            BackColor = principal ? azul : Color.White,
            ForeColor = principal ? Color.White : azul,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        b.FlatAppearance.BorderColor = borda;
        return b;
    }

    // O repositório atual é mantido em memória. Localizamos o método público de
    // listagem sem exigir alterações na interface existente do PedidoRepository.
    private static IEnumerable<Pedido> ObterPedidos()
    {
        object repositorio = AppContexto.Pedidos;
        Type tipo = repositorio.GetType();
        foreach (string nome in new[] { "ListarTodos", "Listar", "ObterTodos", "BuscarTodos", "GetAll" })
        {
            MethodInfo? metodo = tipo.GetMethod(nome, Type.EmptyTypes);
            if (metodo == null) continue;
            if (metodo.Invoke(repositorio, null) is IEnumerable dados)
                return dados.Cast<object>().OfType<Pedido>().ToList();
        }
        return Array.Empty<Pedido>();
    }

    private void AtualizarDados()
    {
        if (tabela == null || inicio == null || fim == null) return;
        var pedidos = ObterPedidos()
            .Where(p => string.Equals(p.Status.ToString(), "Pago", StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.Status.ToString(), "Concluido", StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.Status.ToString(), "Concluído", StringComparison.OrdinalIgnoreCase))
            .Where(p => p.DataPedido.Date >= inicio.Value.Date && p.DataPedido.Date <= fim.Value.Date)
            .ToList();

        var agrupados = pedidos
            .SelectMany(p => p.Itens.Select(i => new { Pedido = p, Item = i }))
            .GroupBy(x => new
            {
                Produto = x.Item.Produto.CodigoSku,
                Variacao = x.Item.ProdutoVariacao?.CodigoSku ?? ""
            })
            .Select(g =>
            {
                var item = g.First().Item;
                string chave = $"{g.Key.Produto}|{g.Key.Variacao}";
                string variacao = item.ProdutoVariacao == null
                    ? "Produto principal"
                    : string.Join(" - ", new[] { item.ProdutoVariacao.Cor, item.ProdutoVariacao.Tamanho }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
                if (string.IsNullOrWhiteSpace(variacao)) variacao = "Produto principal";
                return new LinhaCompra
                {
                    Chave = chave,
                    Produto = item.Produto.Nome,
                    Variacao = variacao,
                    Categoria = item.Produto.Categoria.ToString() switch
                    {
                        "Cosmeticos" => "Cosméticos",
                        "Acessorios" => "Acessórios",
                        "Calcados" => "Calçados",
                        _ => item.Produto.Categoria.ToString()
                    },
                    Quantidade = g.Sum(x => x.Item.Quantidade),
                    Status = statusCompras.TryGetValue(chave, out string? s) ? s : "Aberto"
                };
            }).OrderBy(x => x.Produto).ThenBy(x => x.Variacao).ToList();
        linhas = agrupados;
        Filtrar();
        AtualizarIndicadores(pedidos.Count);
    }

    private void AtualizarIndicadores(int pedidos)
    {
        areaIndicadores.Controls.Clear();
        int largura = Math.Max(190, (areaIndicadores.Width - 250 - 36) / 3);
        AdicionarIndicador(IconChar.Box, linhas.Sum(l => l.Quantidade).ToString(br), "peças no total", largura);
        AdicionarIndicador(IconChar.CartShopping, linhas.Count.ToString(br), "produtos diferentes", largura);
        AdicionarIndicador(IconChar.ClipboardList, pedidos.ToString(br), "pedidos incluídos", largura);
        var caixaExportacao = new Panel
        {
            Width = 230, Height = 81, BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 0)
        };
        var exportar = Botao("↓  Exportar lista CSV", 194, 43, true);
        exportar.Location = new Point(18, 19);
        exportar.Click += (_, _) => Exportar();
        caixaExportacao.Controls.Add(exportar);
        areaIndicadores.Controls.Add(caixaExportacao);
        Reposicionar();
    }

    private void AdicionarIndicador(IconChar icone, string valor, string descricao, int largura)
    {
        var painel = new Panel
        {
            Width = largura, Height = 81, BackColor = Color.White,
            Margin = new Padding(0, 0, 12, 0)
        };
        var fundoIcone = new Panel
        {
            Location = new Point(14, 17), Size = new Size(48, 48),
            BackColor = Color.FromArgb(235, 243, 255)
        };
        fundoIcone.Controls.Add(new IconPictureBox
        {
            IconChar = icone, IconColor = Color.FromArgb(0, 100, 210),
            IconSize = 26, Location = new Point(10, 10), Size = new Size(29, 29),
            BackColor = fundoIcone.BackColor
        });
        painel.Controls.Add(fundoIcone);
        painel.Controls.Add(new Label
        {
            Text = valor, Location = new Point(75, 12), AutoSize = true,
            Font = new Font("Segoe UI", 19, FontStyle.Bold), ForeColor = texto
        });
        painel.Controls.Add(new Label
        {
            Text = descricao, Location = new Point(77, 50), AutoSize = true,
            ForeColor = Color.FromArgb(100, 110, 130), Font = new Font("Segoe UI", 9)
        });
        areaIndicadores.Controls.Add(painel);
    }

    private void Filtrar()
    {
        if (busca == null || categoria == null || status == null) return;
        string termo = busca.Text.Trim();
        filtradas = linhas.Where(l =>
            (categoria.SelectedIndex == 0 || l.Categoria == categoria.Text) &&
            (status.SelectedIndex == 0 || l.Status == status.Text) &&
            (termo.Length == 0 || l.Produto.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || l.Variacao.Contains(termo, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        pagina = 1;
        PreencherTabela();
    }

    private void PreencherTabela()
    {
        tabela.Rows.Clear();
        foreach (var l in filtradas.Skip((pagina - 1) * PorPagina).Take(PorPagina))
        {
            int i = tabela.Rows.Add(l.Produto, l.Variacao, l.Categoria, l.Quantidade, l.Status);
            tabela.Rows[i].Tag = l;
        }
        rodape.Text = $"Mostrando {tabela.Rows.Count} de {filtradas.Count} produtos";
        paginaAtual.Text = pagina.ToString();
        anterior.Enabled = pagina > 1;
        proximo.Enabled = pagina * PorPagina < filtradas.Count;
        tabela.ClearSelection();
    }

    private void EditarStatus(int indice)
    {
        if (tabela.Rows[indice].Tag is not LinhaCompra linha) return;

        const int LarguraJanela = 600, AlturaJanela = 504, Margem = 32;
        int larguraConteudo = LarguraJanela - 2 - Margem * 2;      // 534
        int larguraColuna = (larguraConteudo - 24) / 2;            // 255
        int xColuna2 = Margem + larguraColuna + 24;
        var corContorno = Color.FromArgb(170, 182, 210);
        var corDivisor = Color.FromArgb(225, 230, 240);

        // Janela sem borda, com cantos arredondados e contorno de 1 px
        using var janela = new Form
        {
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            ClientSize = new Size(LarguraJanela, AlturaJanela),
            BackColor = corContorno,
            Font = new Font("Segoe UI", 10),
            KeyPreview = true
        };
        janela.Region = new Region(RetanguloArredondado(
            new Rectangle(0, 0, LarguraJanela, AlturaJanela), 16));
        janela.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape) janela.DialogResult = DialogResult.Cancel;
        };

        var raiz = new Panel
        {
            Location = new Point(1, 1),
            Size = new Size(LarguraJanela - 2, AlturaJanela - 2),
            BackColor = Color.White
        };
        raiz.Region = new Region(RetanguloArredondado(
            new Rectangle(0, 0, raiz.Width, raiz.Height), 15));
        janela.Controls.Add(raiz);

        void Arrastar(object? s, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(janela.Handle, 0xA1, 0x2, 0);
        }
        Label Rotulo(string t, int x, int y, float tamanho = 9, bool negrito = true) => new Label
        {
            Text = t, Location = new Point(x, y), AutoSize = true, ForeColor = texto,
            Font = new Font("Segoe UI", tamanho, negrito ? FontStyle.Bold : FontStyle.Regular)
        };
        Panel Campo(string rotulo, int x, int y, int largura)
        {
            raiz.Controls.Add(Rotulo(rotulo, x, y));
            var caixa = CaixaFiltro(x, y + 22, largura, 40);
            raiz.Controls.Add(caixa);
            return caixa;
        }
        void Valor(Panel caixa, string t) => caixa.Controls.Add(new Label
        {
            Text = t, Location = new Point(12, 2), Size = new Size(caixa.Width - 24, caixa.Height - 4),
            TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true,
            BackColor = Color.White, ForeColor = texto, Font = new Font("Segoe UI", 10)
        });
        Panel Divisor(int y) => new Panel
        {
            Location = new Point(0, y), Size = new Size(raiz.Width, 1), BackColor = corDivisor
        };
        Panel Botao2(string rotulo, int x, int y, int largura, bool principal, Action acao)
        {
            var b = new Panel
            {
                Location = new Point(x, y), Size = new Size(largura, 44),
                BackColor = Color.White, Cursor = Cursors.Hand
            };
            bool sobre = false;
            b.MouseEnter += (_, _) => { sobre = true; b.Invalidate(); };
            b.MouseLeave += (_, _) => { sobre = false; b.Invalidate(); };
            b.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
                using var caminho = RetanguloArredondado(r, 10);
                var preenchimento = principal
                    ? (sobre ? Color.FromArgb(29, 60, 205) : azul)
                    : (sobre ? Color.FromArgb(245, 247, 251) : Color.White);
                using var pincel = new SolidBrush(preenchimento);
                e.Graphics.FillPath(pincel, caminho);
                if (!principal)
                {
                    using var caneta = new Pen(Color.FromArgb(190, 198, 215));
                    e.Graphics.DrawPath(caneta, caminho);
                }
                using var fonte = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, rotulo, fonte, r,
                    principal ? Color.White : azul,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            b.Click += (_, _) => acao();
            return b;
        }

        // ===== Cabeçalho =====
        var titulo = Rotulo("Atualizar status da compra", Margem, 20, 18);
        var subtitulo = Rotulo("Atualize o status da compra deste produto.", Margem + 2, 58, 9, false);
        subtitulo.ForeColor = Color.FromArgb(96, 108, 133);
        var fechar = new IconPictureBox
        {
            IconChar = IconChar.Xmark, IconColor = Color.FromArgb(140, 152, 178), IconSize = 16,
            Size = new Size(20, 20), Location = new Point(raiz.Width - Margem - 20, 26),
            BackColor = Color.White, Cursor = Cursors.Hand, TabStop = false
        };
        fechar.Click += (_, _) => janela.DialogResult = DialogResult.Cancel;
        raiz.Controls.Add(titulo);
        raiz.Controls.Add(subtitulo);
        raiz.Controls.Add(fechar);
        raiz.Controls.Add(Divisor(90));
        raiz.MouseDown += Arrastar;
        titulo.MouseDown += Arrastar;
        subtitulo.MouseDown += Arrastar;

        // ===== Informações do produto (somente leitura) =====
        raiz.Controls.Add(Rotulo("Informações do produto", Margem, 106, 12));
        Valor(Campo("Produto", Margem, 142, larguraColuna), linha.Produto);
        Valor(Campo("Variação", xColuna2, 142, larguraColuna), linha.Variacao);

        var caixaCategoria = Campo("Categoria", Margem, 216, larguraColuna);
        var (fundoBadge, textoBadge) = coresBadge.TryGetValue(linha.Categoria, out var corBadge)
            ? corBadge : (Color.FromArgb(235, 239, 246), texto);
        caixaCategoria.Controls.Add(new Label
        {
            Text = linha.Categoria, AutoSize = true, Padding = new Padding(9, 3, 9, 3),
            BackColor = fundoBadge, ForeColor = textoBadge,
            Font = new Font("Segoe UI", 9), Location = new Point(10, 8)
        });
        Valor(Campo("Quantidade total", xColuna2, 216, larguraColuna), linha.Quantidade.ToString(br));

        // ===== Status =====
        raiz.Controls.Add(Rotulo("Andamento da compra", Margem, 298, 12));
        var caixaStatus = Campo("Status da compra *", Margem, 330, larguraConteudo);
        var opcoes = new ComboBox
        {
            Location = new Point(10, 8), Width = larguraConteudo - 20,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat, BackColor = Color.White,
            Font = new Font("Segoe UI", 10),
            DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 22
        };
        // Desenho próprio: sem o destaque azul forte do ComboBox padrão
        opcoes.DrawItem += (_, e) =>
        {
            if (e.Index < 0) return;
            bool naLista = (e.State & DrawItemState.ComboBoxEdit) == 0;
            bool selecionado = naLista && (e.State & DrawItemState.Selected) != 0;
            using var pincel = new SolidBrush(selecionado ? Color.FromArgb(235, 241, 255) : Color.White);
            e.Graphics.FillRectangle(pincel, e.Bounds);
            TextRenderer.DrawText(e.Graphics, opcoes.Items[e.Index]?.ToString() ?? "", opcoes.Font,
                new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height),
                texto, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        };
        opcoes.Items.AddRange(new object[] { "Aberto", "Solicitado", "Fechado", "Entregue" });
        opcoes.SelectedItem = linha.Status;
        caixaStatus.Controls.Add(opcoes);

        // ===== Rodapé =====
        raiz.Controls.Add(Divisor(420));
        raiz.Controls.Add(Botao2("Cancelar", Margem, 436, 150, false,
            () => janela.DialogResult = DialogResult.Cancel));
        raiz.Controls.Add(Botao2("Salvar alterações", raiz.Width - Margem - 200, 436, 200, true, () =>
        {
            linha.Status = opcoes.Text;
            statusCompras[linha.Chave] = linha.Status;
            janela.DialogResult = DialogResult.OK;
        }));

        if (janela.ShowDialog(this) == DialogResult.OK) Filtrar();
    }

    private void Exportar()
    {
        if (filtradas.Count == 0)
        {
            MessageBox.Show("Não há produtos para exportar.", "VendeAí");
            return;
        }
        using var dialogo = new SaveFileDialog
        {
            Filter = "Arquivo CSV (*.csv)|*.csv",
            FileName = $"compras_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        static string Csv(string valor) => "\"" + valor.Replace("\"", "\"\"") + "\"";
        var sb = new StringBuilder("Produto;Variação;Categoria;Quantidade;Status\r\n");
        foreach (var l in filtradas)
            sb.AppendLine(string.Join(";", Csv(l.Produto), Csv(l.Variacao),
                Csv(l.Categoria), l.Quantidade.ToString(), Csv(l.Status)));
        File.WriteAllText(dialogo.FileName, sb.ToString(), new UTF8Encoding(true));
        MessageBox.Show("Lista exportada com sucesso!", "VendeAí");
    }

    private void Navegar(string destino)
    {
        switch (destino)
        {
            case "Compras": return;
            case "Produtos": Navegacao.Abrir(new CadastroProdutoForm()); return;
            case "Clientes": Navegacao.Abrir(new ManterClientesForm()); return;
            case "Pedidos": Navegacao.Abrir(new ManterPedidosForm()); return;
            default:
                MessageBox.Show("Esta funcionalidade ainda não foi implementada.", "VendeAí");
                return;
        }
    }

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
}