using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FontAwesome.Sharp;
using VendeAi.Models;
using VendeAi.Controllers;

namespace VendeAi;

public class ManterProdutosForm : Form
{
    private readonly Color azul = Color.FromArgb(37, 74, 235);
    private readonly Color marinho = Color.FromArgb(15, 30, 60);
    private readonly Color fundo = Color.FromArgb(245, 247, 251);
    private readonly Color texto = Color.FromArgb(20, 30, 60);
    private readonly Color textoSecundario = Color.FromArgb(100, 110, 130);
    private readonly Color borda = Color.FromArgb(220, 225, 235);
    private readonly Color cinzaIcone = Color.FromArgb(103, 118, 146);
    private readonly CultureInfo br = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ProdutoController produtoService = new(AppContexto.Produtos, AppContexto.Pedidos);

    // Cores dos badges de modalidade e status (fundo, texto)
    private static readonly Dictionary<string, (Color Fundo, Color Texto)> coresBadge = new()
    {
        ["Varejo"]                 = (Color.FromArgb(224, 231, 255), Color.FromArgb(55, 90, 230)),
        ["Atacado"]                = (Color.FromArgb(237, 233, 254), Color.FromArgb(109, 40, 217)),
        ["Pacote (Internacional)"] = (Color.FromArgb(254, 243, 199), Color.FromArgb(180, 83, 9)),
        ["Lote (Nacional)"]        = (Color.FromArgb(204, 251, 241), Color.FromArgb(15, 118, 110)),
        ["Ativo"]                  = (Color.FromArgb(220, 252, 231), Color.FromArgb(22, 128, 60)),
        ["Inativo"]                = (Color.FromArgb(254, 226, 226), Color.FromArgb(185, 28, 28)),
    };

    private sealed class LinhaProduto
    {
        public Produto Produto { get; set; } = null!;
        public ProdutoVariacao? Variacao { get; set; }
        public ProdutoModalidade Venda { get; set; } = null!;
        public string Codigo { get; set; } = "";
        public string Nome { get; set; } = "";
        public string Subtitulo { get; set; } = "";
        public string Categoria { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Modalidade { get; set; } = "";
        public decimal Preco { get; set; }
        public int QuantidadeMinima { get; set; }
        public string Status { get; set; } = "Ativo";
        public string? Imagem { get; set; }
    }

    private List<LinhaProduto> linhas = new();
    private List<LinhaProduto> filtradas = new();
    private readonly Dictionary<string, Image?> miniaturas = new();
    private int pagina = 1;
    private const int PorPagina = 10;

    private Panel painelPrincipal = null!;
    private Panel viewLista = null!;
    private Panel viewCadastro = null!;
    private Panel cabecalho = null!;
    private Panel card = null!;
    private Button botaoNovo = null!;
    private DataGridView tabela = null!;
    private TextBox busca = null!;
    private ComboBox categoria = null!;
    private ComboBox modalidade = null!;
    private ComboBox status = null!;
    private Label rodape = null!;
    private Label paginaAtual = null!;
    private Button anterior = null!;
    private Button proximo = null!;
    private CadastroProdutoForm? cadastroAtual;

    public ManterProdutosForm()
    {
        Text = "VendeAí | Produtos";
        Font = new Font("Segoe UI", 10);
        FormBorderStyle = FormBorderStyle.None;
        BackColor = fundo;
        MinimumSize = new Size(1180, 720);
        WindowState = FormWindowState.Maximized;
        MaximizedBounds = Screen.FromControl(this).WorkingArea;
        MontarTela();
        Shown += (_, _) => AtualizarLista();
    }

    // =========================================================
    // MONTAGEM DA TELA
    // =========================================================

    private void MontarTela()
    {
        SuspendLayout();
        var area = new Panel { Dock = DockStyle.Fill, BackColor = fundo };
        Controls.Add(area);

        // ---------- Menu lateral ----------
        var menu = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = marinho };
        area.Controls.Add(menu);
        var caminhoLogo = Path.Combine(AppContext.BaseDirectory, "Assets", "logo-vendeai.png");
        if (File.Exists(caminhoLogo))
        {
            menu.Controls.Add(new PictureBox
            {
                Image = Image.FromFile(caminhoLogo), SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(20, 24), Size = new Size(190, 65), BackColor = marinho
            });
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
            var botao = CriarBotaoMenu(menu, item.Icone, destino, menuY, destino == "Produtos");
            botao.Click += (_, _) => Navegar(destino);
            menuY += 55;
        }
        var configuracoes = CriarBotaoMenu(menu, IconChar.Gear, "Configurações", 0, false);
        configuracoes.Dock = DockStyle.Bottom;
        configuracoes.Height = 60;

        // ---------- Área principal ----------
        painelPrincipal = new Panel { Dock = DockStyle.Fill, BackColor = fundo };
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

        // Duas "views" ocupam o mesmo espaço: a lista e o cadastro embutido.
        viewLista = new Panel
        {
            Dock = DockStyle.Fill, BackColor = fundo, Padding = new Padding(25, 24, 25, 20)
        };
        painelPrincipal.Controls.Add(viewLista);
        viewCadastro = new Panel { Dock = DockStyle.Fill, BackColor = fundo, Visible = false };
        painelPrincipal.Controls.Add(viewCadastro);
        viewLista.BringToFront();
        viewCadastro.BringToFront();

        MontarLista();

        var barra = CriarBarraTitulo();
        Controls.Add(barra);
        barra.BringToFront();
        ResumeLayout(true);
    }

    private void MontarLista()
    {
        // ---------- Cabeçalho da página ----------
        cabecalho = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = fundo };
        viewLista.Controls.Add(cabecalho);

        var fundoIcone = new Panel
        {
            Location = new Point(0, 4), Size = new Size(64, 64),
            BackColor = Color.FromArgb(228, 236, 255)
        };
        ArredondarRegiao(fundoIcone, 12);
        fundoIcone.Controls.Add(new IconPictureBox
        {
            IconChar = IconChar.Box, IconColor = azul, IconSize = 32,
            Size = new Size(38, 38), Location = new Point(13, 13),
            BackColor = fundoIcone.BackColor, TabStop = false
        });
        cabecalho.Controls.Add(fundoIcone);
        cabecalho.Controls.Add(new Label
        {
            Text = "Produtos", Location = new Point(78, 0), AutoSize = true,
            Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = texto
        });
        cabecalho.Controls.Add(new Label
        {
            Text = "Gerencie seus produtos, crie variações e mantenha as informações atualizadas.",
            Location = new Point(82, 50), AutoSize = true,
            Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(96, 108, 133)
        });

        botaoNovo = new Button
        {
            Text = "+   Novo produto", Size = new Size(175, 46), BackColor = azul,
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        botaoNovo.FlatAppearance.BorderSize = 0;
        botaoNovo.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 60, 205);
        ArredondarRegiao(botaoNovo, 8);
        botaoNovo.Click += (_, _) => AbrirCadastro();
        cabecalho.Controls.Add(botaoNovo);
        cabecalho.Resize += (_, _) =>
            botaoNovo.Location = new Point(cabecalho.ClientSize.Width - botaoNovo.Width, 10);

        // ---------- Cartão com filtros e tabela ----------
        card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        viewLista.Controls.Add(card);
        card.BringToFront();

        var caixaBusca = CaixaFiltro(20, 20, 400, 44);
        caixaBusca.Controls.Add(new IconPictureBox
        {
            IconChar = IconChar.MagnifyingGlass, IconColor = cinzaIcone, IconSize = 17,
            Size = new Size(20, 20), Location = new Point(12, 12),
            BackColor = Color.White, TabStop = false
        });
        busca = new TextBox
        {
            PlaceholderText = "Buscar por nome, código ou categoria...",
            Location = new Point(40, 12), Size = new Size(340, 24),
            BorderStyle = BorderStyle.None, BackColor = Color.White,
            Font = new Font("Segoe UI", 10)
        };
        caixaBusca.Controls.Add(busca);
        card.Controls.Add(caixaBusca);

        categoria = CriarCombo(0, 20, 200, "Todas as categorias", "Roupas", "Cosméticos", "Acessórios");
        modalidade = CriarCombo(0, 20, 210, "Todas as modalidades", "Varejo", "Atacado",
            "Pacote (Internacional)", "Lote (Nacional)");
        status = CriarCombo(0, 20, 170, "Todos os status", "Ativo", "Inativo");

        tabela = new DataGridView
        {
            Location = new Point(20, 90), BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None, RowHeadersVisible = false,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false, ReadOnly = true,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false, ColumnHeadersHeight = 46,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowTemplate = { Height = 62 }, GridColor = Color.FromArgb(235, 239, 246),
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            ScrollBars = ScrollBars.Vertical
        };
        tabela.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 251);
        tabela.ColumnHeadersDefaultCellStyle.ForeColor = texto;
        tabela.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 247, 251);
        tabela.ColumnHeadersDefaultCellStyle.SelectionForeColor = texto;
        tabela.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        tabela.ColumnHeadersDefaultCellStyle.Padding = new Padding(9, 0, 0, 0);
        tabela.DefaultCellStyle.Font = new Font("Segoe UI", 10);
        tabela.DefaultCellStyle.ForeColor = texto;
        tabela.DefaultCellStyle.Padding = new Padding(9, 0, 0, 0);
        tabela.DefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 245, 255);
        tabela.DefaultCellStyle.SelectionForeColor = texto;

        tabela.Columns.Add("Imagem", "Imagem");
        tabela.Columns.Add("Codigo", "Código");
        tabela.Columns.Add("Produto", "Produto");
        tabela.Columns.Add("Categoria", "Categoria");
        tabela.Columns.Add("Modalidade", "Modalidade");
        tabela.Columns.Add("Preco", "Preço de venda");
        tabela.Columns.Add("QtdMinima", "Qtd. mínima");
        tabela.Columns.Add("Status", "Status");
        tabela.Columns.Add("Acoes", "Ações");
        tabela.Columns["Imagem"]!.FillWeight = 70;
        tabela.Columns["Codigo"]!.FillWeight = 85;
        tabela.Columns["Produto"]!.FillWeight = 190;
        tabela.Columns["Categoria"]!.FillWeight = 100;
        tabela.Columns["Modalidade"]!.FillWeight = 150;
        tabela.Columns["Preco"]!.FillWeight = 105;
        tabela.Columns["QtdMinima"]!.FillWeight = 85;
        tabela.Columns["Status"]!.FillWeight = 90;
        tabela.Columns["Acoes"]!.FillWeight = 70;
        tabela.Columns["Acoes"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        tabela.Columns["Acoes"]!.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        tabela.Columns["Acoes"]!.HeaderCell.Style.Padding = Padding.Empty;
        tabela.Columns["Acoes"]!.DefaultCellStyle.Padding = Padding.Empty;
        foreach (DataGridViewColumn coluna in tabela.Columns)
            coluna.SortMode = DataGridViewColumnSortMode.NotSortable;

        tabela.CellPainting += PintarCelula;
        tabela.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == tabela.Columns["Acoes"]!.Index)
                MostrarMenuAcoes(e.RowIndex, e.ColumnIndex);
        };
        card.Controls.Add(tabela);

        rodape = new Label { AutoSize = true, ForeColor = textoSecundario };
        card.Controls.Add(rodape);
        anterior = BotaoPagina("‹");
        proximo = BotaoPagina("›");
        paginaAtual = new Label
        {
            Size = new Size(38, 34), TextAlign = ContentAlignment.MiddleCenter,
            BackColor = azul, ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        card.Controls.Add(anterior);
        card.Controls.Add(proximo);
        card.Controls.Add(paginaAtual);
        anterior.Click += (_, _) => { if (pagina > 1) { pagina--; PreencherTabela(); } };
        proximo.Click += (_, _) =>
        {
            if (pagina * PorPagina < filtradas.Count) { pagina++; PreencherTabela(); }
        };

        busca.TextChanged += (_, _) => Filtrar();
        categoria.SelectedIndexChanged += (_, _) => Filtrar();
        modalidade.SelectedIndexChanged += (_, _) => Filtrar();
        status.SelectedIndexChanged += (_, _) => Filtrar();

        card.SizeChanged += (_, _) =>
        {
            ArredondarRegiao(card, 10);
            AjustarCard();
        };
        AjustarCard();
    }

    private void AjustarCard()
    {
        if (card == null || tabela == null || rodape == null) return;
        int w = card.ClientSize.Width;
        int h = card.ClientSize.Height;

        var caixaStatus = (Panel)status.Parent!;
        var caixaModalidade = (Panel)modalidade.Parent!;
        var caixaCategoria = (Panel)categoria.Parent!;
        var caixaBusca = (Panel)busca.Parent!;

        caixaStatus.Left = w - 20 - caixaStatus.Width;
        caixaModalidade.Left = caixaStatus.Left - 12 - caixaModalidade.Width;
        caixaCategoria.Left = caixaModalidade.Left - 12 - caixaCategoria.Width;
        caixaBusca.Width = Math.Max(180, caixaCategoria.Left - 12 - caixaBusca.Left);
        busca.Width = caixaBusca.Width - 52;

        tabela.Width = Math.Max(300, w - 40);
        tabela.Height = Math.Max(150, h - 90 - 70);
        int rodapeY = h - 52;
        rodape.Location = new Point(22, rodapeY + 9);
        proximo.Location = new Point(w - 63, rodapeY);
        paginaAtual.Location = new Point(w - 108, rodapeY);
        anterior.Location = new Point(w - 157, rodapeY);
    }

    // =========================================================
    // CADASTRO EMBUTIDO
    // =========================================================

    private void AbrirCadastro()
    {
        FecharCadastro();

        cadastroAtual = new CadastroProdutoForm(true);

        cadastroAtual.VoltarParaLista += (_, _) =>
            BeginInvoke(new Action(MostrarLista));

        cadastroAtual.TopLevel = false;
        cadastroAtual.FormBorderStyle = FormBorderStyle.None;
        cadastroAtual.Dock = DockStyle.Fill;

        viewCadastro.Controls.Add(cadastroAtual);

        viewLista.Visible = false;
        viewCadastro.Visible = true;
        viewCadastro.BringToFront();

        cadastroAtual.Show();
        cadastroAtual.BringToFront();
    }

    private void MostrarLista()
    {
        FecharCadastro();
        viewCadastro.Visible = false;
        viewLista.Visible = true;
        AtualizarLista();
    }

    private void FecharCadastro()
    {
        if (cadastroAtual == null) return;
        viewCadastro.Controls.Remove(cadastroAtual);
        cadastroAtual.Dispose();
        cadastroAtual = null;
    }

    // =========================================================
    // DADOS
    // =========================================================

    private static string NomeCategoria(Categoria c) => c.ToString() switch
    {
        "Cosmeticos" => "Cosméticos",
        "Acessorios" => "Acessórios",
        _ => c.ToString()
    };

    private static string NomeModalidade(Modalidade m) => m switch
    {
        Modalidade.Atacado => "Atacado",
        Modalidade.Pacote => "Pacote (Internacional)",
        Modalidade.Lote => "Lote (Nacional)",
        _ => "Varejo"
    };

    private void AtualizarLista()
    {
        if (tabela == null) return;
        var resultado = new List<LinhaProduto>();

        foreach (Produto p in produtoService.ListarProdutos())
        {
            string nomeCategoria = NomeCategoria(p.Categoria);
            bool ativo = p.Status == StatusProduto.Ativo;

            if (p.Modalidades.Count == 0)
                resultado.Add(new LinhaProduto { Produto = p, Codigo = p.CodigoSku, Nome = p.Nome, Subtitulo = "Produto principal", Categoria = nomeCategoria, Marca = p.Marca ?? "", Status = ativo ? "Ativo" : "Inativo", Imagem = p.Imagem });

            foreach (ProdutoModalidade m in p.Modalidades)
            {
                resultado.Add(new LinhaProduto
                {
                    Produto = p, Venda = m, Codigo = p.CodigoSku, Nome = p.Nome,
                    Subtitulo = "Produto principal", Categoria = nomeCategoria,
                    Marca = p.Marca ?? "", Modalidade = NomeModalidade(m.Modalidade),
                    Preco = m.PrecoVenda, QuantidadeMinima = m.QuantidadeMinimaVenda,
                    Status = ativo ? "Ativo" : "Inativo", Imagem = p.Imagem
                });
            }

            foreach (ProdutoVariacao v in p.Variacoes)
            {
                string subtitulo = string.Join(" · ",
                    new[] { v.Cor, v.Tamanho }.Where(s => !string.IsNullOrWhiteSpace(s)));
                if (string.IsNullOrWhiteSpace(subtitulo)) subtitulo = "Variação";
                foreach (ProdutoModalidade m in v.Modalidades)
                {
                    resultado.Add(new LinhaProduto
                    {
                        Produto = p,
                        Variacao = v, Venda = m,
                        Codigo = string.IsNullOrWhiteSpace(v.CodigoSku) ? p.CodigoSku : v.CodigoSku,
                        Nome = p.Nome, Subtitulo = subtitulo, Categoria = nomeCategoria,
                        Marca = p.Marca ?? "", Modalidade = NomeModalidade(m.Modalidade),
                        Preco = m.PrecoVenda, QuantidadeMinima = m.QuantidadeMinimaVenda,
                        Status = ativo && v.Status == StatusProduto.Ativo && m.Status == StatusProduto.Ativo ? "Ativo" : "Inativo",
                        Imagem = v.Imagem ?? p.Imagem
                    });
                }
            }
        }

        linhas = resultado.OrderBy(l => l.Nome).ThenBy(l => l.Codigo).ToList();
        Filtrar();
    }

    private void Filtrar()
    {
        if (busca == null || categoria == null || modalidade == null || status == null) return;
        string termo = busca.Text.Trim();
        filtradas = linhas.Where(l =>
            (categoria.SelectedIndex == 0 || l.Categoria == categoria.Text) &&
            (modalidade.SelectedIndex == 0 || l.Modalidade == modalidade.Text) &&
            (status.SelectedIndex == 0 || l.Status == status.Text) &&
            (termo.Length == 0
                || l.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || l.Codigo.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || l.Categoria.Contains(termo, StringComparison.OrdinalIgnoreCase)
                || l.Marca.Contains(termo, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        pagina = 1;
        PreencherTabela();
    }

    private void PreencherTabela()
    {
        tabela.Rows.Clear();
        foreach (var l in filtradas.Skip((pagina - 1) * PorPagina).Take(PorPagina))
        {
            int i = tabela.Rows.Add("", l.Codigo, l.Nome, l.Categoria, l.Modalidade,
                l.Venda == null ? "-" : l.Preco.ToString("C2", br), l.Venda == null ? "-" : l.QuantidadeMinima.ToString(br), l.Status, "");
            tabela.Rows[i].Tag = l;
        }

        if (filtradas.Count == 0)
        {
            rodape.Text = "Nenhum produto encontrado";
        }
        else
        {
            int primeiro = (pagina - 1) * PorPagina + 1;
            int ultimo = primeiro + tabela.Rows.Count - 1;
            rodape.Text = $"{primeiro} a {ultimo} de {filtradas.Count} produtos";
        }
        paginaAtual.Text = pagina.ToString();
        anterior.Enabled = pagina > 1;
        proximo.Enabled = pagina * PorPagina < filtradas.Count;
        tabela.ClearSelection();
    }

    private Image? ObterMiniatura(string? caminho)
    {
        if (string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho)) return null;
        if (miniaturas.TryGetValue(caminho, out Image? existente)) return existente;
        try
        {
            using Image original = Image.FromFile(caminho);
            const int lado = 88;
            var mini = new Bitmap(lado, lado);
            using (var g = Graphics.FromImage(mini))
            {
                g.Clear(Color.White);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                float escala = Math.Min((float)lado / original.Width, (float)lado / original.Height);
                int w = (int)(original.Width * escala), h = (int)(original.Height * escala);
                g.DrawImage(original, (lado - w) / 2, (lado - h) / 2, w, h);
            }
            miniaturas[caminho] = mini;
            return mini;
        }
        catch
        {
            miniaturas[caminho] = null;
            return null;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        foreach (var imagem in miniaturas.Values) imagem?.Dispose();
        miniaturas.Clear();
        base.OnFormClosed(e);
    }

    // =========================================================
    // DESENHO DA TABELA
    // =========================================================

    private void PintarCelula(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics == null) return;
        if (tabela.Rows[e.RowIndex].Tag is not LinhaProduto linha) return;
        string col = tabela.Columns[e.ColumnIndex].Name;
        var g = e.Graphics;
        var c = e.CellBounds;

        switch (col)
        {
            case "Imagem":
            {
                e.PaintBackground(c, true);
                var r = new Rectangle(c.X + 9, c.Y + (c.Height - 46) / 2, 46, 46);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var caminho = RetanguloArredondado(r, 8);
                Image? mini = ObterMiniatura(linha.Imagem);
                if (mini != null)
                {
                    var estadoClip = g.Save();
                    g.SetClip(caminho, CombineMode.Intersect);
                    g.DrawImage(mini, r);
                    g.Restore(estadoClip);
                }
                else
                {
                    using var pincel = new SolidBrush(Color.FromArgb(238, 241, 247));
                    g.FillPath(pincel, caminho);
                    using var fonte = new Font("Segoe UI", 7);
                    TextRenderer.DrawText(g, "sem\nimagem", fonte, r, Color.FromArgb(140, 150, 170),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                        | TextFormatFlags.NoPadding);
                }
                e.Handled = true;
                break;
            }
            case "Produto":
            {
                e.PaintBackground(c, true);
                using var negrito = new Font("Segoe UI", 10);
                using var pequena = new Font("Segoe UI", 9);
                var flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
                TextRenderer.DrawText(g, linha.Nome, negrito,
                    new Rectangle(c.X + 9, c.Y + 12, c.Width - 14, 20), texto, flags);
                TextRenderer.DrawText(g, linha.Subtitulo, pequena,
                    new Rectangle(c.X + 9, c.Y + 33, c.Width - 14, 18),
                    Color.FromArgb(120, 130, 150), flags);
                e.Handled = true;
                break;
            }
            case "Modalidade":
            case "Status":
            {
                e.PaintBackground(c, true);
                string valor = col == "Status" ? linha.Status : linha.Modalidade;
                if (coresBadge.TryGetValue(valor, out var cor))
                {
                    using var fonte = new Font("Segoe UI", 9);
                    var tam = TextRenderer.MeasureText(g, valor, fonte);
                    var r = new Rectangle(c.X + 9, c.Y + (c.Height - 28) / 2, tam.Width + 24, 28);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var caminho = RetanguloArredondado(r, 8);
                    using var pincel = new SolidBrush(cor.Fundo);
                    g.FillPath(pincel, caminho);
                    TextRenderer.DrawText(g, valor, fonte, r, cor.Texto,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                        | TextFormatFlags.NoPadding);
                }
                e.Handled = true;
                break;
            }
            case "Acoes":
            {
                e.PaintBackground(c, true);
                var r = new Rectangle(c.X + (c.Width - 36) / 2, c.Y + (c.Height - 34) / 2, 36, 34);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var caminho = RetanguloArredondado(r, 7);
                using var caneta = new Pen(Color.FromArgb(200, 208, 225));
                g.DrawPath(caneta, caminho);
                using var ponto = new SolidBrush(texto);
                int cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
                foreach (int dy in new[] { -7, 0, 7 })
                    g.FillEllipse(ponto, cx - 2, cy + dy - 2, 4, 4);
                e.Handled = true;
                break;
            }
        }
    }

    // =========================================================
    // MENU DE AÇÕES (⋮)
    // =========================================================

    private void MostrarMenuAcoes(int indiceLinha, int indiceColuna)
    {
        if (tabela.Rows[indiceLinha].Tag is not LinhaProduto linha) return;
        Produto produto = linha.Produto;
        bool produtoAtivo = produto.Status == StatusProduto.Ativo;
        var vermelho = Color.FromArgb(220, 38, 38);
        var verde = Color.FromArgb(22, 128, 60);

        var menu = new ContextMenuStrip
        {
            Font = new Font("Segoe UI", 10), ShowImageMargin = false,
            BackColor = Color.White
        };

        ToolStripMenuItem NovoItem(string rotulo, Color? cor = null)
        {
            var item = new ToolStripMenuItem(rotulo) { Padding = new Padding(8, 7, 24, 7) };
            if (cor.HasValue) item.ForeColor = cor.Value;
            return item;
        }

        var editar = NovoItem("Editar produto");
        var criarVariacao = NovoItem("Criar variação");
        editar.Click += (_, _) => EditarProduto(produto);
        criarVariacao.Click += (_, _) => CriarVariacao(produto);
        menu.Items.Add(editar);
        menu.Items.Add(criarVariacao);

        // Ações específicas da variação (só aparecem nas linhas de variação).
        if (linha.Variacao != null && linha.Venda != null)
        {
            ProdutoVariacao alvo = linha.Variacao;
            ProdutoModalidade venda = linha.Venda;
            bool variacaoAtiva = linha.Status == "Ativo";

            menu.Items.Add(new ToolStripSeparator());

            var alterarVariacao = NovoItem(
                variacaoAtiva ? "Inativar variação" : "Ativar variação",
                variacaoAtiva ? vermelho : verde);
            // A variação depende do produto: não dá para ativá-la com o produto inativo.
            if (!variacaoAtiva && !produtoAtivo)
            {
                alterarVariacao.Enabled = false;
                alterarVariacao.ToolTipText = "Ative o produto antes de ativar a variação.";
            }
            alterarVariacao.Click += (_, _) => AlterarStatusVariacao(produto, alvo, venda, variacaoAtiva);

            var excluirVariacao = NovoItem("Excluir variação", vermelho);
            excluirVariacao.Click += (_, _) => ExcluirVariacao(produto, alvo, venda);

            var editarVariacao = NovoItem("Editar variação");
            editarVariacao.Click += (_, _) => EditarVariacao(produto, alvo, linha.Venda);
            menu.Items.Add(editarVariacao);
            menu.Items.Add(alterarVariacao);
            menu.Items.Add(excluirVariacao);
        }

        menu.Items.Add(new ToolStripSeparator());
        var alterarProduto = NovoItem(
            produtoAtivo ? "Inativar produto" : "Ativar produto",
            produtoAtivo ? vermelho : verde);
        alterarProduto.Click += (_, _) => AlterarStatus(produto, produtoAtivo);
        menu.Items.Add(alterarProduto);

        menu.Closed += (_, _) => menu.BeginInvoke(new Action(menu.Dispose));

        var celula = tabela.GetCellDisplayRectangle(indiceColuna, indiceLinha, false);
        menu.Show(tabela, new Point(celula.Left + celula.Width / 2 + 18, celula.Bottom - 8),
            ToolStripDropDownDirection.BelowLeft);
    }

    // Inativar/ativar o produto também inativa/ativa todas as variações dele.
    private void AlterarStatus(Produto produto, bool estavaAtivo)
    {
        string acao = estavaAtivo ? "inativar" : "ativar";
        string complemento = produto.Variacoes.Count == 0 ? "" : estavaAtivo
            ? "\n\nTodas as variações deste produto também serão inativadas."
            : "\n\nTodas as variações deste produto também serão ativadas.";

        var confirma = MessageBox.Show(
            $"Deseja {acao} o produto \"{produto.Nome}\"?{complemento}", "VendeAí",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirma != DialogResult.Yes) return;

        string resultado = produtoService.AlterarStatusProduto(
            produto.CodigoSku, estavaAtivo ? StatusProduto.Inativo : StatusProduto.Ativo);
        MessageBox.Show(resultado, "VendeAí", MessageBoxButtons.OK, MessageBoxIcon.Information);
        AtualizarLista();
    }

    private static string DescricaoVariacao(ProdutoVariacao variacao)
    {
        string descricao = string.Join(" · ",
            new[] { variacao.Cor, variacao.Tamanho }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (!string.IsNullOrWhiteSpace(descricao)) return descricao;
        return string.IsNullOrWhiteSpace(variacao.CodigoSku) ? "variação" : variacao.CodigoSku;
    }

    // Inativar/ativar somente esta variação: o produto e as outras variações não mudam.
    private void AlterarStatusVariacao(Produto produto, ProdutoVariacao variacao, ProdutoModalidade venda, bool estavaAtiva)
    {
        string acao = estavaAtiva ? "inativar" : "ativar";
        var confirma = MessageBox.Show(
            $"Deseja {acao} a variação \"{DescricaoVariacao(variacao)} ({venda.Modalidade})\" de \"{produto.Nome}\"?" +
            "\n\nO produto e as outras variações não serão afetados.",
            "VendeAí", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirma != DialogResult.Yes) return;

        string resultado = produtoService.AlterarStatusModalidade(
            produto.CodigoSku, variacao.Id, venda.Modalidade,
            estavaAtiva ? StatusProduto.Inativo : StatusProduto.Ativo);
        MessageBox.Show(resultado, "VendeAí", MessageBoxButtons.OK, MessageBoxIcon.Information);
        AtualizarLista();
    }

    // Excluir a variação não afeta o produto: ele continua existindo normalmente.
    private void ExcluirVariacao(Produto produto, ProdutoVariacao variacao, ProdutoModalidade venda)
    {
        var confirma = MessageBox.Show(
            $"Deseja excluir a variação \"{DescricaoVariacao(variacao)} ({venda.Modalidade})\" de \"{produto.Nome}\"?" +
            "\n\nSomente a modalidade selecionada será removida. Esta ação não pode ser desfeita.",
            "VendeAí", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirma != DialogResult.Yes) return;

        string resultado = produtoService.ExcluirModalidade(produto.CodigoSku, variacao.Id, venda.Modalidade);
        MessageBox.Show(resultado, "VendeAí", MessageBoxButtons.OK, MessageBoxIcon.Information);
        AtualizarLista();
    }

    // =========================================================
    // EDITAR PRODUTO
    // =========================================================

    private void EditarProduto(Produto produto)
    {
        using var janela = NovaJanelaModal("Editar produto",
            "Atualize as informações básicas do produto.", 536, out Panel raiz);
        const int margem = 32;
        int larguraTotal = raiz.Width - margem * 2;
        int larguraColuna = (larguraTotal - 24) / 2;
        int xColuna2 = margem + larguraColuna + 24;

        var nome = TextoModal(CampoModal(raiz, "Nome do produto *", margem, 106, larguraTotal),
            produto.Nome);
        TextoModal(CampoModal(raiz, "Código (SKU)", margem, 180, larguraColuna),
            produto.CodigoSku, somenteLeitura: true);
        var combo = ComboModal(CampoModal(raiz, "Categoria *", xColuna2, 180, larguraColuna),
            "Roupas", "Cosméticos", "Acessórios");
        combo.SelectedIndex = produto.Categoria switch
        {
            Categoria.Cosmeticos => 1,
            Categoria.Acessorios => 2,
            _ => 0
        };
        var marca = TextoModal(CampoModal(raiz, "Marca", margem, 254, larguraTotal),
            produto.Marca ?? "");
        var descricao = TextoModal(CampoModal(raiz, "Descrição", margem, 328, larguraTotal, 80),
            produto.Descricao ?? "", multilinha: true);

        RodapeModal(raiz, janela, 450, "Salvar alterações", () =>
        {
            if (string.IsNullOrWhiteSpace(nome.Text))
            {
                MessageBox.Show("Informe o nome do produto.", "VendeAí",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var atualizado = new Produto
            {
                Nome = nome.Text.Trim(),
                CodigoSku = produto.CodigoSku,
                Categoria = combo.SelectedIndex switch
                {
                    1 => Categoria.Cosmeticos,
                    2 => Categoria.Acessorios,
                    _ => Categoria.Roupas
                },
                Marca = string.IsNullOrWhiteSpace(marca.Text) ? null : marca.Text.Trim(),
                Descricao = string.IsNullOrWhiteSpace(descricao.Text) ? null : descricao.Text.Trim(),
                Imagem = produto.Imagem,
                Status = produto.Status
            };
            string resultado = produtoService.AtualizarProduto(atualizado);
            bool ok = resultado == "Produto atualizado com sucesso.";
            MessageBox.Show(resultado, "VendeAí", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) janela.DialogResult = DialogResult.OK;
        });

        if (janela.ShowDialog(this) == DialogResult.OK) AtualizarLista();
    }

    // =========================================================
    // CRIAR VARIAÇÃO
    // =========================================================

    private static string ParteSku(string? valor)
    {
        string t = (valor ?? "").Trim().ToUpperInvariant()
            .Normalize(System.Text.NormalizationForm.FormD);
        var letras = t.Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch)
            != UnicodeCategory.NonSpacingMark).ToArray();
        t = new string(letras).Normalize(System.Text.NormalizationForm.FormC);
        return new string(t.Select(ch => char.IsLetterOrDigit(ch) ? ch : '-').ToArray()).Trim('-');
    }

    private void CriarVariacao(Produto produto)
    {
        if (produto.Status == StatusProduto.Inativo)
        {
            MessageBox.Show("Não é possível adicionar variações a um produto inativo.", "VendeAí",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var janela = NovaJanelaModal("Criar variação",
            "Os dados de venda são copiados do produto. Altere somente o necessário.",
            504, out Panel raiz);
        const int margem = 32;
        int larguraTotal = raiz.Width - margem * 2;
        int larguraColuna = (larguraTotal - 24) / 2;
        int xColuna2 = margem + larguraColuna + 24;
        int larguraTerco = (larguraTotal - 32) / 3;

        TextoModal(CampoModal(raiz, "Produto", margem, 106, larguraTotal),
            $"{produto.Nome}  ({produto.CodigoSku})", somenteLeitura: true);
        var cor = TextoModal(CampoModal(raiz, "Cor", margem, 180, larguraColuna));
        var tamanho = TextoModal(CampoModal(raiz, "Tamanho", xColuna2, 180, larguraColuna));
        var sku = TextoModal(CampoModal(raiz, "SKU da variação (automático)", margem, 254, larguraColuna),
            somenteLeitura: true);
        var combo = ComboModal(CampoModal(raiz, "Modalidade", xColuna2, 254, larguraColuna),
            "Varejo", "Atacado", "Pacote (Internacional)", "Lote (Nacional)");

        ProdutoModalidade? modeloVenda = produto.Modalidades.FirstOrDefault();
        combo.SelectedIndex = modeloVenda == null ? 0 : (int)modeloVenda.Modalidade;
        var quantidade = NumeroModal(CampoModal(raiz, "Quantidade mínima", margem, 328, larguraTerco),
            modeloVenda?.QuantidadeMinimaVenda ?? 1, 0, 1, 1000000);
        var precoVenda = NumeroModal(CampoModal(raiz, "Preço de venda (R$)",
            margem + larguraTerco + 16, 328, larguraTerco),
            modeloVenda?.PrecoVenda ?? 0, 2, 0, 1000000);
        var precoCusto = NumeroModal(CampoModal(raiz, "Preço de custo (R$)",
            margem + (larguraTerco + 16) * 2, 328, larguraTerco),
            modeloVenda?.PrecoCusto ?? 0, 2, 0, 1000000);

        ProdutoVariacao? Existente() => produto.Variacoes.FirstOrDefault(v =>
            string.Equals((v.Cor ?? "").Trim(), cor.Text.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals((v.Tamanho ?? "").Trim(), tamanho.Text.Trim(), StringComparison.OrdinalIgnoreCase));

        void AtualizarSku()
        {
            var existente = Existente();
            if (existente != null && !string.IsNullOrWhiteSpace(existente.CodigoSku))
            {
                sku.Text = existente.CodigoSku;
                return;
            }
            string complemento = string.Join("-", new[] { ParteSku(cor.Text), ParteSku(tamanho.Text) }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            if (complemento.Length == 0) { sku.Clear(); return; }

            string candidato = $"{ParteSku(produto.CodigoSku)}-{complemento}";
            string final = candidato;
            int numero = 2;
            while (AppContexto.Produtos.ExisteSku(final)) final = $"{candidato}-{numero++}";
            sku.Text = final;
        }
        cor.TextChanged += (_, _) => AtualizarSku();
        tamanho.TextChanged += (_, _) => AtualizarSku();

        RodapeModal(raiz, janela, 420, "Criar variação", () =>
        {
            if (precoVenda.Value <= 0)
            {
                MessageBox.Show("O preço de venda deve ser maior que zero.", "VendeAí",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(cor.Text) && string.IsNullOrWhiteSpace(tamanho.Text))
            {
                MessageBox.Show("Informe pelo menos a cor ou o tamanho da variação.", "VendeAí",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var modalidadeEscolhida = (Modalidade)combo.SelectedIndex;
            var novaModalidade = new ProdutoModalidade
            {
                Modalidade = modalidadeEscolhida,
                QuantidadeMinimaVenda = (int)quantidade.Value,
                PrecoVenda = precoVenda.Value,
                PrecoCusto = precoCusto.Value > 0 ? precoCusto.Value : null
            };

            string resultado;
            bool ok;
            var existente = Existente();
            if (existente != null)
            {
                if (existente.Modalidades.Any(m => m.Modalidade == modalidadeEscolhida))
                {
                    MessageBox.Show("Essa combinação de cor, tamanho e modalidade já existe.",
                        "VendeAí", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                resultado = produtoService.AdicionarModalidade(produto.CodigoSku, existente.Id, novaModalidade);
                ok = resultado == "Modalidade adicionada à variação existente.";
            }
            else
            {
                var variacao = new ProdutoVariacao
                {
                    CodigoSku = string.IsNullOrWhiteSpace(sku.Text) ? null : sku.Text.Trim(),
                    Cor = string.IsNullOrWhiteSpace(cor.Text) ? null : cor.Text.Trim(),
                    Tamanho = string.IsNullOrWhiteSpace(tamanho.Text) ? null : tamanho.Text.Trim(),
                    Status = StatusProduto.Ativo,
                    Modalidades = new List<ProdutoModalidade> { novaModalidade }
                };
                resultado = produtoService.AdicionarVariacao(produto.CodigoSku, variacao);
                ok = resultado == "Variação adicionada com sucesso.";
            }
            MessageBox.Show(resultado, "VendeAí", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) janela.DialogResult = DialogResult.OK;
        });

        if (janela.ShowDialog(this) == DialogResult.OK) AtualizarLista();
    }

    private void EditarVariacao(Produto produto, ProdutoVariacao variacao, ProdutoModalidade venda)
    {
        using var janela = NovaJanelaModal("Editar variação",
            "Cor e tamanho são compartilhados; preços pertencem à modalidade.",
            504, out Panel raiz);
        const int margem = 32;
        int larguraTotal = raiz.Width - margem * 2;
        int larguraColuna = (larguraTotal - 24) / 2;
        int xColuna2 = margem + larguraColuna + 24;
        int larguraTerco = (larguraTotal - 32) / 3;

        TextoModal(CampoModal(raiz, "Produto", margem, 106, larguraTotal),
            $"{produto.Nome}  ({produto.CodigoSku})", somenteLeitura: true);
        var cor = TextoModal(CampoModal(raiz, "Cor", margem, 180, larguraColuna), variacao.Cor ?? "");
        var tamanho = TextoModal(CampoModal(raiz, "Tamanho", xColuna2, 180, larguraColuna), variacao.Tamanho ?? "");
        var sku = TextoModal(CampoModal(raiz, "SKU da variação (automático)", margem, 254, larguraColuna),
            variacao.CodigoSku ?? "", somenteLeitura: true);
        var combo = ComboModal(CampoModal(raiz, "Modalidade", xColuna2, 254, larguraColuna),
            "Varejo", "Atacado", "Pacote (Internacional)", "Lote (Nacional)");

        ProdutoModalidade modeloVenda = venda;
        combo.SelectedIndex = (int)venda.Modalidade;
        combo.Enabled = false;
        var quantidade = NumeroModal(CampoModal(raiz, "Quantidade mínima", margem, 328, larguraTerco),
            modeloVenda?.QuantidadeMinimaVenda ?? 1, 0, 1, 1000000);
        var precoVenda = NumeroModal(CampoModal(raiz, "Preço de venda (R$)",
            margem + larguraTerco + 16, 328, larguraTerco),
            modeloVenda?.PrecoVenda ?? 0, 2, 0, 1000000);
        var precoCusto = NumeroModal(CampoModal(raiz, "Preço de custo (R$)",
            margem + (larguraTerco + 16) * 2, 328, larguraTerco),
            modeloVenda?.PrecoCusto ?? 0, 2, 0, 1000000);

        RodapeModal(raiz, janela, 420, "Salvar alterações", () =>
        {
            string resultado = produtoService.EditarVariacao(produto.CodigoSku, variacao.Id, venda.Modalidade,
                cor.Text.Trim(), tamanho.Text.Trim(), (int)quantidade.Value, precoVenda.Value,
                precoCusto.Value > 0 ? precoCusto.Value : null);
            bool ok = resultado == "Variação atualizada com sucesso.";
            MessageBox.Show(resultado, "VendeAí", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) janela.DialogResult = DialogResult.OK;
        });
        if (janela.ShowDialog(this) == DialogResult.OK) AtualizarLista();
    }

    // =========================================================
    // COMPONENTES DAS JANELAS (estilo "Editar pedido")
    // =========================================================

    private Form NovaJanelaModal(string titulo, string subtitulo, int altura, out Panel raiz)
    {
        const int largura = 600;
        var janela = new Form
        {
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.None, ShowInTaskbar = false,
            ClientSize = new Size(largura, altura),
            BackColor = Color.FromArgb(170, 182, 210),
            Font = new Font("Segoe UI", 10), KeyPreview = true
        };
        janela.Region = new Region(RetanguloArredondado(new Rectangle(0, 0, largura, altura), 16));
        janela.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape) janela.DialogResult = DialogResult.Cancel;
        };

        var r = new Panel
        {
            Location = new Point(1, 1), Size = new Size(largura - 2, altura - 2),
            BackColor = Color.White
        };
        r.Region = new Region(RetanguloArredondado(new Rectangle(0, 0, r.Width, r.Height), 15));
        janela.Controls.Add(r);

        void Arrastar(object? s, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(janela.Handle, 0xA1, 0x2, 0);
        }
        var lblTitulo = new Label
        {
            Text = titulo, Location = new Point(32, 20), AutoSize = true,
            Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = texto
        };
        var lblSub = new Label
        {
            Text = subtitulo, Location = new Point(34, 58), AutoSize = true,
            Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(96, 108, 133)
        };
        var fechar = new IconPictureBox
        {
            IconChar = IconChar.Xmark, IconColor = Color.FromArgb(140, 152, 178), IconSize = 16,
            Size = new Size(20, 20), Location = new Point(r.Width - 32 - 20, 26),
            BackColor = Color.White, Cursor = Cursors.Hand, TabStop = false
        };
        fechar.Click += (_, _) => janela.DialogResult = DialogResult.Cancel;
        r.Controls.Add(lblTitulo);
        r.Controls.Add(lblSub);
        r.Controls.Add(fechar);
        r.Controls.Add(new Panel
        {
            Location = new Point(0, 90), Size = new Size(r.Width, 1),
            BackColor = Color.FromArgb(225, 230, 240)
        });
        r.MouseDown += Arrastar;
        lblTitulo.MouseDown += Arrastar;
        lblSub.MouseDown += Arrastar;

        raiz = r;
        return janela;
    }

    private Panel CampoModal(Panel raiz, string rotulo, int x, int y, int largura, int altura = 40)
    {
        raiz.Controls.Add(new Label
        {
            Text = rotulo, Location = new Point(x, y), AutoSize = true, ForeColor = texto,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        });
        var caixa = CaixaFiltro(x, y + 22, largura, altura);
        raiz.Controls.Add(caixa);
        return caixa;
    }

    private TextBox TextoModal(Panel caixa, string valor = "", bool somenteLeitura = false,
        bool multilinha = false)
    {
        var t = new TextBox
        {
            BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10),
            Text = valor, Multiline = multilinha, Location = new Point(12, 10),
            Width = caixa.Width - 24, BackColor = Color.White, ForeColor = texto
        };
        if (multilinha) t.Height = caixa.Height - 20;
        if (somenteLeitura)
        {
            t.ReadOnly = true;
            t.TabStop = false;
            t.BackColor = Color.FromArgb(246, 248, 252);
            t.ForeColor = Color.FromArgb(96, 108, 133);
            caixa.BackColor = t.BackColor;
        }
        caixa.Controls.Add(t);
        return t;
    }

    private NumericUpDown NumeroModal(Panel caixa, decimal valor, int casas, decimal minimo,
        decimal maximo)
    {
        var n = new NumericUpDown
        {
            BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 10),
            Location = new Point(12, 10), Width = caixa.Width - 24,
            DecimalPlaces = casas, Minimum = minimo, Maximum = maximo,
            Value = Math.Clamp(valor, minimo, maximo), ThousandsSeparator = true,
            BackColor = Color.White, ForeColor = texto
        };
        caixa.Controls.Add(n);
        return n;
    }

    private ComboBox ComboModal(Panel caixa, params string[] opcoes)
    {
        var c = new ComboBox
        {
            Location = new Point(10, 8), Width = caixa.Width - 20,
            DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat,
            BackColor = Color.White, Font = new Font("Segoe UI", 10),
            DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 22
        };
        c.DrawItem += (_, e) =>
        {
            if (e.Index < 0) return;
            bool naLista = (e.State & DrawItemState.ComboBoxEdit) == 0;
            bool selecionado = naLista && (e.State & DrawItemState.Selected) != 0;
            using var pincel = new SolidBrush(selecionado ? Color.FromArgb(235, 241, 255) : Color.White);
            e.Graphics.FillRectangle(pincel, e.Bounds);
            TextRenderer.DrawText(e.Graphics, c.Items[e.Index]?.ToString() ?? "", c.Font,
                new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height),
                texto, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        };
        c.Items.AddRange(opcoes.Cast<object>().ToArray());
        if (c.Items.Count > 0) c.SelectedIndex = 0;
        caixa.Controls.Add(c);
        return c;
    }

    private void RodapeModal(Panel raiz, Form janela, int y, string textoSalvar, Action salvar)
    {
        raiz.Controls.Add(new Panel
        {
            Location = new Point(0, y), Size = new Size(raiz.Width, 1),
            BackColor = Color.FromArgb(225, 230, 240)
        });
        raiz.Controls.Add(BotaoModal("Cancelar", 32, y + 16, 150, false,
            () => janela.DialogResult = DialogResult.Cancel));
        raiz.Controls.Add(BotaoModal(textoSalvar, raiz.Width - 32 - 200, y + 16, 200, true, salvar));
    }

    private Panel BotaoModal(string rotulo, int x, int y, int largura, bool principal, Action acao)
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
            TextRenderer.DrawText(e.Graphics, rotulo, fonte, r, principal ? Color.White : azul,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };
        b.Click += (_, _) => acao();
        return b;
    }

    // =========================================================
    // COMPONENTES GERAIS (mesmo padrão das outras telas)
    // =========================================================

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
                ArredondarRegiao(botao, 8);
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
            Text = "VendeAí | Produtos", ForeColor = Color.White,
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

    private ComboBox CriarCombo(int x, int y, int largura, params string[] opcoes)
    {
        var caixa = CaixaFiltro(x, y, largura, 44);
        var c = new ComboBox
        {
            Location = new Point(11, 10), Width = largura - 22,
            DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat,
            BackColor = Color.White, Font = new Font("Segoe UI", 9)
        };
        c.Items.AddRange(opcoes.Cast<object>().ToArray());
        c.SelectedIndex = 0;
        caixa.Controls.Add(c);
        card.Controls.Add(caixa);
        return c;
    }

    private Button BotaoPagina(string rotulo)
    {
        var b = new Button
        {
            Text = rotulo, Size = new Size(42, 34), BackColor = Color.White,
            ForeColor = azul, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        b.FlatAppearance.BorderColor = borda;
        return b;
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

    private static void ArredondarRegiao(Control controle, int raio)
    {
        if (controle.Width <= 0 || controle.Height <= 0) return;
        using var caminho = RetanguloArredondado(
            new Rectangle(0, 0, controle.Width, controle.Height), raio);
        var anterior = controle.Region;
        controle.Region = new Region(caminho);
        anterior?.Dispose();
    }

    private void Navegar(string destino)
    {
        switch (destino)
        {
            case "Produtos":
                // Já estamos na tela de Produtos: se o cadastro estiver aberto, volta para a lista.
                if (cadastroAtual != null) MostrarLista();
                return;
            case "Clientes": Navegacao.Abrir(new ManterClientesForm()); return;
            case "Pedidos": Navegacao.Abrir(new ManterPedidosForm()); return;
            case "Compras": Navegacao.Abrir(new ManterComprasForm()); return;
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