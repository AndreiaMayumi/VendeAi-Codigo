using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using VendeAi.Models;
using VendeAi.Repositories;
using VendeAi.Controllers;

namespace VendeAi;

public partial class CadastroProdutoForm : Form
{
    // ============================
    // CORES DO VENDEAÍ
    // ============================

    private readonly Color corPrimaria = Color.FromArgb(37, 74, 235);
    private readonly Color corPrimariaHover = Color.FromArgb(29, 60, 200);
    private readonly Color corMenu = Color.FromArgb(15, 30, 60);
    private readonly Color corMenuHover = Color.FromArgb(25, 45, 82);
    private readonly Color corFundo = Color.FromArgb(245, 247, 251);
    private readonly Color corTexto = Color.FromArgb(20, 30, 50);
    private readonly Color corTextoSecundario = Color.FromArgb(100, 110, 130);
    private readonly Color corBorda = Color.FromArgb(220, 225, 235);

    // ============================
    // CAMPOS
    // ============================

    private TextBox txtNome = null!;
    private TextBox txtSku = null!;
    private TextBox txtDescricao = null!;
    private TextBox txtMarca = null!;

    private ComboBox cmbCategoria = null!;
    private ComboBox cmbModalidade = null!;

    private NumericUpDown nudQuantidadeMinima = null!;
    private NumericUpDown nudPrecoVenda = null!;
    private NumericUpDown nudPrecoCusto = null!;

    private PictureBox picProduto = null!;

    private Button btnSelecionarImagem = null!;
    private Button btnSalvar = null!;
    private Button btnCancelar = null!;

    private string? caminhoImagem;

    private readonly ProdutoRepository produtoRepository;
    private readonly ProdutoController produtoController;


    // ============================
    // CONSTRUTOR
    // ============================

    public CadastroProdutoForm()
    {
        InitializeComponent();

        produtoRepository = new ProdutoRepository();
        produtoController = new ProdutoController(produtoRepository);

        CriarInterface();
    }


    // ============================
    // INTERFACE PRINCIPAL
    // ============================

    private void CriarInterface()
    {
        SuspendLayout();

        // Configuração da janela
        Text = "Cadastrar Produto - VendeAí";

        BackColor = corFundo;

        WindowState = FormWindowState.Maximized;

        MinimumSize = new Size(1100, 700);

        Font = new Font("Segoe UI", 10);

        AutoScroll = false;


        // Remove os controles antigos criados anteriormente
        Controls.Clear();


        // ============================
        // MENU LATERAL
        // ============================

        Panel menu = new Panel
        {
            Dock = DockStyle.Left,
            Width = 230,
            BackColor = corMenu
        };

        Controls.Add(menu);


        // LOGO
        Label logo = new Label
        {
            Text = "VendeAí",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, 35)
        };

        menu.Controls.Add(logo);


        Label logoDescricao = new Label
        {
            Text = "Gestão de vendas",
            ForeColor = Color.FromArgb(160, 175, 200),
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(32, 75)
        };

        menu.Controls.Add(logoDescricao);


        // ============================
        // ITENS DO MENU
        // ============================

        int menuY = 135;

        CriarBotaoMenu(menu, "⌂", "Painel", menuY, false);
        menuY += 55;

        CriarBotaoMenu(menu, "▣", "Produtos", menuY, true);
        menuY += 55;

        CriarBotaoMenu(menu, "♙", "Clientes", menuY, false);
        menuY += 55;

        CriarBotaoMenu(menu, "▤", "Pedidos", menuY, false);
        menuY += 55;

        CriarBotaoMenu(menu, "▥", "Relatórios", menuY, false);
        menuY += 55;

        CriarBotaoMenu(menu, "⚿", "Permissões", menuY, false);


        Button btnConfiguracoes = CriarBotaoMenu(
            menu,
            "⚙",
            "Configurações",
            0,
            false
        );

        btnConfiguracoes.Dock = DockStyle.Bottom;
        btnConfiguracoes.Height = 60;


        // ============================
        // ÁREA PRINCIPAL
        // ============================

        Panel principal = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = corFundo,
            AutoScroll = true
        };

        Controls.Add(principal);

        principal.BringToFront();


        // ============================
        // TOPO
        // ============================

        Panel topo = new Panel
        {
            Dock = DockStyle.Top,
            Height = 75,
            BackColor = Color.White
        };

        principal.Controls.Add(topo);


        Label breadcrumb = new Label
        {
            Text = "Produtos   ›   Cadastrar produto",
            ForeColor = corTextoSecundario,
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(45, 29)
        };

        topo.Controls.Add(breadcrumb);


        Label usuario = new Label
        {
            Text = "Mayumi",
            ForeColor = corTexto,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        topo.Controls.Add(usuario);

        topo.Resize += (s, e) =>
        {
            usuario.Location = new Point(
                topo.ClientSize.Width - usuario.Width - 45,
                28
            );
        };


        // ============================
        // CONTEÚDO
        // ============================

        Panel conteudo = new Panel
        {
            Location = new Point(45, 110),
            Width = 1050,
            Height = 760,
            BackColor = corFundo
        };

        principal.Controls.Add(conteudo);


        // TÍTULO
        Label titulo = new Label
        {
            Text = "Cadastrar produto",
            ForeColor = corTexto,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };

        conteudo.Controls.Add(titulo);


        Label subtitulo = new Label
        {
            Text = "Preencha as informações do produto para adicioná-lo ao sistema.",
            ForeColor = corTextoSecundario,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(3, 48)
        };

        conteudo.Controls.Add(subtitulo);


        // ============================
        // CARD PRINCIPAL
        // ============================

        Panel card = CriarCard(
            new Point(0, 90),
            new Size(710, 590)
        );

        conteudo.Controls.Add(card);


        // ============================
        // CARD IMAGEM
        // ============================

        Panel cardImagem = CriarCard(
            new Point(735, 90),
            new Size(300, 350)
        );

        conteudo.Controls.Add(cardImagem);


        CriarCamposProduto(card);

        CriarAreaImagem(cardImagem);


        // ============================
        // BOTÕES INFERIORES
        // ============================

        btnCancelar = new Button
        {
            Text = "Cancelar",
            Size = new Size(140, 45),
            Location = new Point(0, 705),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.DarkRed,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        btnCancelar.FlatAppearance.BorderColor = corBorda;
        btnCancelar.FlatAppearance.BorderSize = 1;

        btnCancelar.Click += (s, e) => Close();

        conteudo.Controls.Add(btnCancelar);


        btnSalvar = new Button
        {
            Text = "Salvar produto",
            Size = new Size(190, 45),
            Location = new Point(845, 705),
            FlatStyle = FlatStyle.Flat,
            BackColor = corPrimaria,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        btnSalvar.FlatAppearance.BorderSize = 0;

        btnSalvar.MouseEnter += (s, e) =>
        {
            btnSalvar.BackColor = corPrimariaHover;
        };

        btnSalvar.MouseLeave += (s, e) =>
        {
            btnSalvar.BackColor = corPrimaria;
        };

        btnSalvar.Click += SalvarProduto;

        conteudo.Controls.Add(btnSalvar);


        ArredondarControle(btnSalvar, 7);
        ArredondarControle(btnCancelar, 7);

        ResumeLayout();
    }


    // ============================
    // CAMPOS DO PRODUTO
    // ============================

    private void CriarCamposProduto(Panel card)
    {
        Label tituloBasico = CriarTituloSecao(
            "Informações básicas",
            30,
            25
        );

        card.Controls.Add(tituloBasico);


        // NOME
        CriarLabel(card, "Nome do produto *", 30, 75);

        txtNome = CriarTextBox(
            card,
            30,
            100,
            390
        );


        // SKU
        CriarLabel(card, "Código (SKU) *", 445, 75);

        txtSku = CriarTextBox(
            card,
            445,
            100,
            225
        );


        // DESCRIÇÃO
        CriarLabel(card, "Descrição", 30, 155);

        txtDescricao = CriarTextBoxMultiline(
            card,
            30,
            180,
            640,
            65
        );


        // CATEGORIA
        CriarLabel(card, "Categoria *", 30, 265);

        cmbCategoria = CriarComboBox(
            card,
            30,
            290,
            300
        );

        cmbCategoria.Items.AddRange(
            new object[]
            {
                "Roupas",
                "Cosméticos",
                "Acessórios",
                "Eletrônicos",
                "Alimentos",
                "Outros"
            }
        );


        // MARCA
        CriarLabel(card, "Marca", 355, 265);

        txtMarca = CriarTextBox(
            card,
            355,
            290,
            315
        );


        // ============================
        // INFORMAÇÕES DE VENDA
        // ============================

        Label tituloVenda = CriarTituloSecao(
            "Informações de venda",
            30,
            355
        );

        card.Controls.Add(tituloVenda);


        // MODALIDADE
        CriarLabel(
            card,
            "Modalidade de venda *",
            30,
            405
        );

        cmbModalidade = CriarComboBox(
            card,
            30,
            430,
            300
        );

        cmbModalidade.Items.AddRange(
            new object[]
            {
                "Varejo",
                "Atacado",
                "Pacote Internacional",
                "Lote Nacional"
            }
        );


        // QUANTIDADE MÍNIMA
        CriarLabel(
            card,
            "Quantidade mínima *",
            355,
            405
        );

        nudQuantidadeMinima = CriarNumeric(
            card,
            355,
            430,
            150
        );

        nudQuantidadeMinima.Minimum = 1;
        nudQuantidadeMinima.Value = 1;


        // PREÇO VENDA
        CriarLabel(
            card,
            "Preço de venda (R$) *",
            30,
            495
        );

        nudPrecoVenda = CriarNumeric(
            card,
            30,
            520,
            300
        );

        nudPrecoVenda.DecimalPlaces = 2;
        nudPrecoVenda.Maximum = 1000000;
        nudPrecoVenda.ThousandsSeparator = true;


        // PREÇO CUSTO
        CriarLabel(
            card,
            "Preço de custo (R$)",
            355,
            495
        );

        nudPrecoCusto = CriarNumeric(
            card,
            355,
            520,
            315
        );

        nudPrecoCusto.DecimalPlaces = 2;
        nudPrecoCusto.Maximum = 1000000;
        nudPrecoCusto.ThousandsSeparator = true;
    }


    // ============================
    // IMAGEM DO PRODUTO
    // ============================

    private void CriarAreaImagem(Panel card)
    {
        Label titulo = CriarTituloSecao(
            "Imagem do produto",
            25,
            25
        );

        card.Controls.Add(titulo);


        Label descricao = new Label
        {
            Text = "Adicione uma imagem para facilitar\n" +
                   "a identificação do produto.",
            ForeColor = corTextoSecundario,
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(25, 60)
        };

        card.Controls.Add(descricao);


        picProduto = new PictureBox
        {
            Location = new Point(25, 110),
            Size = new Size(250, 150),
            BackColor = Color.FromArgb(245, 247, 251),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        card.Controls.Add(picProduto);


        Label textoImagem = new Label
        {
            Text = "Nenhuma imagem selecionada",
            ForeColor = corTextoSecundario,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(250, 25),
            Location = new Point(25, 170)
        };

        picProduto.Controls.Add(textoImagem);


        btnSelecionarImagem = new Button
        {
            Text = "Selecionar imagem",
            Location = new Point(25, 280),
            Size = new Size(250, 42),
            BackColor = Color.White,
            ForeColor = corPrimaria,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        btnSelecionarImagem.FlatAppearance.BorderColor = corPrimaria;

        btnSelecionarImagem.Click += SelecionarImagem;

        card.Controls.Add(btnSelecionarImagem);

        ArredondarControle(btnSelecionarImagem, 6);
    }


    // ============================
    // SELECIONAR IMAGEM
    // ============================

    private void SelecionarImagem(object? sender, EventArgs e)
    {
        using OpenFileDialog dialog = new OpenFileDialog();

        dialog.Title = "Selecionar imagem do produto";

        dialog.Filter =
            "Imagens|*.jpg;*.jpeg;*.png;*.bmp";


        if (dialog.ShowDialog() == DialogResult.OK)
        {
            caminhoImagem = dialog.FileName;

            try
            {
                using Image imagemOriginal =
                    Image.FromFile(caminhoImagem);

                picProduto.Image =
                    new Bitmap(imagemOriginal);
            }
            catch
            {
                MessageBox.Show(
                    "Não foi possível carregar a imagem.",
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }


    // ============================
    // SALVAR PRODUTO
    // ============================

    private void SalvarProduto(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            MessageBox.Show(
                "Informe o nome do produto.",
                "Campo obrigatório",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            txtNome.Focus();
            return;
        }


        if (string.IsNullOrWhiteSpace(txtSku.Text))
        {
            MessageBox.Show(
                "Informe o código SKU.",
                "Campo obrigatório",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            txtSku.Focus();
            return;
        }


        if (cmbCategoria.SelectedIndex == -1)
        {
            MessageBox.Show(
                "Selecione uma categoria.",
                "Campo obrigatório",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            cmbCategoria.Focus();
            return;
        }


        if (cmbModalidade.SelectedIndex == -1)
        {
            MessageBox.Show(
                "Selecione a modalidade de venda.",
                "Campo obrigatório",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            cmbModalidade.Focus();
            return;
        }


        if (nudPrecoVenda.Value <= 0)
        {
            MessageBox.Show(
                "O preço de venda deve ser maior que zero.",
                "Valor inválido",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );

            nudPrecoVenda.Focus();
            return;
        }


        // Aqui vamos ligar os campos ao ProdutoController.
        //
        // Por enquanto mantemos esta parte separada porque
        // os nomes exatos das propriedades precisam ser os
        // mesmos existentes no seu Produto.cs.


        MessageBox.Show(
            "Dados do produto validados com sucesso!",
            "VendeAí",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }


    // ============================
    // CRIAR BOTÃO DO MENU
    // ============================

    private Button CriarBotaoMenu(
        Panel menu,
        string icone,
        string texto,
        int y,
        bool selecionado)
    {
        Button botao = new Button
        {
            Text = $"   {icone}     {texto}",
            TextAlign = ContentAlignment.MiddleLeft,

            Location = new Point(15, y),

            Size = new Size(200, 45),

            FlatStyle = FlatStyle.Flat,

            BackColor = selecionado
                ? corPrimaria
                : corMenu,

            ForeColor = Color.White,

            Font = new Font(
                "Segoe UI",
                10,
                selecionado
                    ? FontStyle.Bold
                    : FontStyle.Regular
            ),

            Cursor = Cursors.Hand
        };

        botao.FlatAppearance.BorderSize = 0;

        if (!selecionado)
        {
            botao.MouseEnter += (s, e) =>
            {
                botao.BackColor = corMenuHover;
            };

            botao.MouseLeave += (s, e) =>
            {
                botao.BackColor = corMenu;
            };
        }

        menu.Controls.Add(botao);

        ArredondarControle(botao, 6);

        return botao;
    }


    // ============================
    // CARD
    // ============================

    private Panel CriarCard(
        Point localizacao,
        Size tamanho)
    {
        Panel painel = new Panel
        {
            Location = localizacao,
            Size = tamanho,
            BackColor = Color.White
        };

        ArredondarControle(painel, 10);

        return painel;
    }


    // ============================
    // LABEL
    // ============================

    private void CriarLabel(
        Control pai,
        string texto,
        int x,
        int y)
    {
        // Verifica automaticamente se o campo
        // foi marcado como obrigatório com *
        bool obrigatorio =
            texto.TrimEnd().EndsWith("*");

        // Remove o * do texto principal
        // para podermos desenhá-lo separadamente
        if (obrigatorio)
        {
            texto = texto.TrimEnd();

            texto = texto
                .Substring(0, texto.Length - 1)
                .TrimEnd();
        }


        // Texto normal do campo
        Label label = new Label
        {
            Text = texto,

            ForeColor = corTexto,

            Font = new Font(
                "Segoe UI",
                9,
                FontStyle.Bold
            ),

            AutoSize = true,

            Location = new Point(x, y)
        };

        pai.Controls.Add(label);


        // Se for obrigatório, adiciona
        // um segundo Label somente para o *
        if (obrigatorio)
        {
            Label asterisco = new Label
            {
                Text = "*",

                ForeColor =
                    Color.FromArgb(220, 38, 38),

                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold
                ),

                AutoSize = true
            };

            pai.Controls.Add(asterisco);

            asterisco.Location = new Point(
                label.Right + 3,
                y
            );
        }
    }


    // ============================
    // TÍTULO DE SEÇÃO
    // ============================

    private Label CriarTituloSecao(
        string texto,
        int x,
        int y)
    {
        return new Label
        {
            Text = texto,

            ForeColor = corTexto,

            Font = new Font(
                "Segoe UI",
                12,
                FontStyle.Bold
            ),

            AutoSize = true,

            Location = new Point(x, y)
        };
    }


    // ============================
    // TEXTBOX NORMAL
    // ============================

    private TextBox CriarTextBox(
        Control pai,
        int x,
        int y,
        int largura)
    {
        /*
         * O Panel é usado somente para desenhar
         * a borda ao redor do TextBox.
         *
         * A diferença agora é que o campo fica
         * praticamente encostado no Panel,
         * deixando a borda bem fina.
         */

        Panel borda = new Panel
        {
            Location = new Point(x, y),

            // Altura pequena para evitar
            // borda grossa em cima e embaixo
            Size = new Size(largura, 29),

            BackColor = corBorda
        };

        ArredondarControle(borda, 6);


        TextBox caixa = new TextBox
        {
            BorderStyle = BorderStyle.None,

            Font = new Font(
                "Segoe UI",
                9.5f
            ),

            BackColor = Color.White,

            ForeColor = Color.FromArgb(
                25,
                35,
                55
            ),

            // 2 pixels de cada lado
            Width = largura - 4
        };


        /*
         * Em vez de escolher manualmente a
         * posição vertical, calculamos o centro.
         *
         * Isso evita ficar:
         *
         * borda fina em cima
         * borda enorme embaixo
         *
         * ou vice-versa.
         */

        caixa.Location = new Point(
            2,
            Math.Max(
                1,
                (borda.Height - caixa.Height) / 2
            )
        );


        borda.Controls.Add(caixa);

        pai.Controls.Add(borda);


        // ============================
        // FOCO DO CAMPO
        // ============================

        caixa.Enter += (s, e) =>
        {
            // Ao clicar, a borda fica azul
            borda.BackColor = corPrimaria;
        };


        caixa.Leave += (s, e) =>
        {
            // Ao sair, volta para cinza
            borda.BackColor = corBorda;
        };


        return caixa;
    }


    // ============================
    // TEXTBOX MULTILINE
    // ============================

    private TextBox CriarTextBoxMultiline(
        Control pai,
        int x,
        int y,
        int largura,
        int altura)
    {
        /*
         * A descrição precisa de um método
         * separado porque o TextBox é maior.
         *
         * Antes ele era criado como TextBox
         * normal e só depois recebia Multiline.
         *
         * Como agora temos um Panel funcionando
         * como borda, precisamos criar os dois
         * já com a altura correta.
         */

        Panel borda = new Panel
        {
            Location = new Point(x, y),

            Size = new Size(
                largura,
                altura
            ),

            BackColor = corBorda
        };


        ArredondarControle(
            borda,
            6
        );


        TextBox caixa = new TextBox
        {
            BorderStyle =
                BorderStyle.None,

            Multiline = true,

            Font = new Font(
                "Segoe UI",
                9.5f
            ),

            BackColor = Color.White,

            ForeColor = Color.FromArgb(
                25,
                35,
                55
            ),

            // Apenas 2 px para a borda
            Location = new Point(
                2,
                2
            ),

            Size = new Size(
                largura - 4,
                altura - 4
            ),

            ScrollBars =
                ScrollBars.Vertical
        };


        borda.Controls.Add(caixa);

        pai.Controls.Add(borda);


        // Ao selecionar a descrição,
        // a borda também fica azul.
        caixa.Enter += (s, e) =>
        {
            borda.BackColor =
                corPrimaria;
        };


        caixa.Leave += (s, e) =>
        {
            borda.BackColor =
                corBorda;
        };


        return caixa;
    }


    // ============================
    // COMBOBOX
    // ============================

    private ComboBox CriarComboBox(
        Control pai,
        int x,
        int y,
        int largura)
    {
        ComboBox combo = new ComboBox
        {
            Location = new Point(x, y),

            Size = new Size(largura, 35),

            Font = new Font("Segoe UI", 10),

            DropDownStyle =
                ComboBoxStyle.DropDownList,

            FlatStyle = FlatStyle.Flat,

            BackColor = Color.White,

            ForeColor = corTexto
        };

        pai.Controls.Add(combo);

        return combo;
    }


    // ============================
    // NUMERIC UP DOWN
    // ============================

    private NumericUpDown CriarNumeric(
        Control pai,
        int x,
        int y,
        int largura)
    {
        NumericUpDown numeric =
            new NumericUpDown
            {
                Location =
                    new Point(x, y),

                Size =
                    new Size(largura, 35),

                Font = new Font(
                    "Segoe UI",
                    10
                ),

                BackColor =
                    Color.White,

                ForeColor =
                    corTexto,

                BorderStyle =
                    BorderStyle.FixedSingle
            };

        pai.Controls.Add(numeric);

        return numeric;
    }


    // ============================
    // BORDAS ARREDONDADAS
    // ============================

    private void ArredondarControle(
        Control controle,
        int raio)
    {
        if (controle.Width <= 0 ||
            controle.Height <= 0)
        {
            return;
        }


        GraphicsPath path =
            new GraphicsPath();


        int diametro =
            raio * 2;


        // Canto superior esquerdo
        path.AddArc(
            0,
            0,
            diametro,
            diametro,
            180,
            90
        );


        // Canto superior direito
        path.AddArc(
            controle.Width - diametro,
            0,
            diametro,
            diametro,
            270,
            90
        );


        // Canto inferior direito
        path.AddArc(
            controle.Width - diametro,
            controle.Height - diametro,
            diametro,
            diametro,
            0,
            90
        );


        // Canto inferior esquerdo
        path.AddArc(
            0,
            controle.Height - diametro,
            diametro,
            diametro,
            90,
            90
        );


        path.CloseFigure();


        controle.Region =
            new Region(path);
    }
}