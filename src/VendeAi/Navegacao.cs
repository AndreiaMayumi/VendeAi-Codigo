
using System;
using System.Windows.Forms;

namespace VendeAi;

public sealed class Navegacao : ApplicationContext
{
    private static Navegacao? instancia;
    private Form? telaAtual;
    private bool navegando;

    public Navegacao(Form telaInicial)
    {
        instancia = this;
        Exibir(telaInicial);
    }

    public static void Abrir(Form novaTela)
    {
        if (instancia == null)
        {
            novaTela.Dispose();

            throw new InvalidOperationException(
                "A navegação ainda não foi inicializada.");
        }

        instancia.Exibir(novaTela);
    }

    private void Exibir(Form novaTela)
    {
        if (navegando)
        {
            novaTela.Dispose();
            return;
        }

        if (novaTela.IsDisposed)
            return;

        navegando = true;

        try
        {
            Form? anterior = telaAtual;

            if (anterior != null && !anterior.IsDisposed)
            {
                novaTela.StartPosition = FormStartPosition.Manual;

                if (anterior.WindowState == FormWindowState.Maximized)
                {
                    novaTela.Bounds = anterior.RestoreBounds;
                    novaTela.WindowState = FormWindowState.Maximized;
                }
                else if (anterior.WindowState == FormWindowState.Minimized)
                {
                    novaTela.Bounds = anterior.RestoreBounds;
                    novaTela.WindowState = FormWindowState.Normal;
                }
                else
                {
                    novaTela.Bounds = anterior.Bounds;
                    novaTela.WindowState = FormWindowState.Normal;
                }
            }

            // Oculta a janela anterior antes da troca.
            if (anterior != null && !anterior.IsDisposed)
            {
                anterior.Hide();
            }

            // Define a nova janela principal.
            telaAtual = novaTela;
            MainForm = novaTela;

            novaTela.FormClosed += (_, _) =>
            {
                if (ReferenceEquals(telaAtual, novaTela))
                {
                    telaAtual = null;
                    ExitThread();
                }
            };

            // Exibe a nova tela.
            novaTela.Show();

            // Encerra a janela anterior.
            if (anterior != null && !anterior.IsDisposed)
            {
                anterior.Close();
            }
        }
        finally
        {
            navegando = false;
        }
    }
}
