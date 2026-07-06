using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using magal.Data.Repositories;
using magal.Models;
using magal.ViewModels;
using magal.Views;

namespace magal
{
    public partial class MainWindow : Window
    {
        private HistoricoView _historicoView;
        private OrcamentoView _orcamentoView;


        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            AbrirOrcamento();
        }

       

        // --- NAVEGAÇÃO ---
        private void BtnOrcamentos_Click(object sender, RoutedEventArgs e) => AbrirOrcamento();
        private void BtnHistorico_Click(object sender, RoutedEventArgs e) => AbrirHistorico();

        private void AtualizarBotaoAtivo(Button botaoAtivo)
        {
            // Lista com todos os seus botões da sidebar
            var botoes = new[] { BtnOrcamentos, BtnHistorico, };

            foreach (var btn in botoes)
            {
                if (btn == null) continue;

                if (btn == botaoAtivo)
                {
                    // Define uma Tag que avisa ao XAML para fixar o estilo ativo
                    btn.Tag = "Ativo";
                }
                else
                {
                    btn.Tag = null;
                }
            }
        }

      

        public void AbrirOrcamento()
        {
            _orcamentoView = new OrcamentoView();
            MainContent.Content = _orcamentoView;
            AtualizarBotaoAtivo(BtnOrcamentos); 
        }

        public void AbrirHistorico()
        {
            if (_historicoView == null) _historicoView = new HistoricoView();
            MainContent.Content = _historicoView;
            AtualizarBotaoAtivo(BtnHistorico);
        }

       



        public async void IrParaEdicao(Projeto projetoSimplificado)
        {
            var repo = new ProjetoRepository();
            Projeto projetoCompleto = await repo.CarregarProjetoCompleto(projetoSimplificado.id_projeto);

            var viewModel = new OrcamentoViewModel();
            viewModel.CarregarProjetoParaEdicao(projetoCompleto);

            var view = new OrcamentoView();
            view.DataContext = viewModel;
            MainContent.Content = view;

            AtualizarBotaoAtivo(BtnOrcamentos); 
        }

       

     

        public ContentControl MainContentControl => MainContent;
    }
}
