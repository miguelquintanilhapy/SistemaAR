using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using magal.Models;
using magal.Data.Repositories;

namespace magal.ViewModels
{
    /// <summary>
    /// ViewModel responsável pela tela de histórico: listagem, busca por cliente/placa
    /// e reabertura de orçamentos já salvos.
    /// </summary>
    public class HistoricoViewModel : BaseModel
    {
        private readonly OrcamentoRepository _repository = new OrcamentoRepository();
        private string _filtroTexto;
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string FiltroTexto
        {
            get => _filtroTexto;
            set
            {
                _filtroTexto = value;
                OnPropertyChanged();
                OrcamentosView.Refresh();
            }
        }

        public ObservableCollection<Orcamento> Orcamentos { get; } = new ObservableCollection<Orcamento>();
        public ICollectionView OrcamentosView { get; }

        public RelayCommand ReabrirCommand { get; }
        public RelayCommand AtualizarCommand { get; }

        public HistoricoViewModel()
        {
            OrcamentosView = CollectionViewSource.GetDefaultView(Orcamentos);
            OrcamentosView.Filter = FiltrarOrcamento;

            ReabrirCommand = new RelayCommand(p => ExecutarReabertura(p as Orcamento));
            AtualizarCommand = new RelayCommand(async _ => await CarregarHistoricoAsync());
        }

        public async Task CarregarHistoricoAsync()
        {
            try
            {
                IsLoading = true;

                var lista = await _repository.ObterTodosParaHistoricoAsync();

                Orcamentos.Clear();
                foreach (var o in lista) Orcamentos.Add(o);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar histórico: " + ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool FiltrarOrcamento(object obj)
        {
            if (string.IsNullOrWhiteSpace(FiltroTexto)) return true;
            if (obj is not Orcamento orcamento) return false;

            var busca = FiltroTexto.Trim();

            return (orcamento.ClienteNome?.Contains(busca, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (orcamento.VeiculoPlaca?.Contains(busca, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private async void ExecutarReabertura(Orcamento orcamento)
        {
            if (orcamento == null) return;

            try
            {
                IsLoading = true;

                var (completo, itens) = await _repository.ObterPorIdAsync(orcamento.Id);

                if (completo == null)
                {
                    MessageBox.Show("Não foi possível localizar este orçamento.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var mainWindow = Application.Current.Windows.OfType<magal.MainWindow>().FirstOrDefault();
                mainWindow?.AbrirOrcamentoParaEdicao(completo, itens);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao reabrir orçamento: " + ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
