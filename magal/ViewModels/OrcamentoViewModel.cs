using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using magal.Models;
using magal.Data.Repositories;
using magal.Services;

namespace magal.ViewModels
{
    /// <summary>
    /// ViewModel responsável pela tela de orçamento: dados do cliente/veículo,
    /// itens (peças e mão de obra), salvamento e geração do PDF.
    /// </summary>
    public class OrcamentoViewModel : BaseModel
    {
        private readonly OrcamentoRepository _repository = new OrcamentoRepository();

        private Orcamento _orcamentoAtual;
        private string _novaDescricaoItem;
        private decimal _novoValorItem;
        private bool _isLoading;
        private bool _processando;

        public Orcamento OrcamentoAtual
        {
            get => _orcamentoAtual;
            set { _orcamentoAtual = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ItemOrcamento> ItensOrcamento { get; } = new ObservableCollection<ItemOrcamento>();

        public string NovaDescricaoItem
        {
            get => _novaDescricaoItem;
            set { _novaDescricaoItem = value; OnPropertyChanged(); }
        }

        public decimal NovoValorItem
        {
            get => _novoValorItem;
            set { _novoValorItem = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool BotaoAtivo => !_processando;

        public RelayCommand AdicionarItemCommand { get; }
        public RelayCommand DeletarItemCommand { get; }
        public RelayCommand SalvarOrcamentoCommand { get; }
        public RelayCommand GerarPdfCommand { get; }

        public OrcamentoViewModel()
        {
            AdicionarItemCommand = new RelayCommand(_ => AdicionarItem());
            DeletarItemCommand = new RelayCommand(param => DeletarItem(param as ItemOrcamento));
            SalvarOrcamentoCommand = new RelayCommand(async _ => await SalvarOrcamentoAsync());
            GerarPdfCommand = new RelayCommand(async _ => await GerarPdfAsync());

            NovoOrcamento();
        }

        private void NovoOrcamento()
        {
            OrcamentoAtual = new Orcamento();
            ItensOrcamento.Clear();
            NovaDescricaoItem = string.Empty;
            NovoValorItem = 0;
        }

        /// <summary>
        /// Carrega um orçamento já existente (vindo do histórico) para visualização, edição e reimpressão.
        /// </summary>
        public void CarregarParaEdicao(Orcamento orcamento, List<ItemOrcamento> itens)
        {
            OrcamentoAtual = orcamento;

            ItensOrcamento.Clear();
            foreach (var item in itens) ItensOrcamento.Add(item);
        }

        private void AdicionarItem()
        {
            if (string.IsNullOrWhiteSpace(NovaDescricaoItem))
            {
                MessageBox.Show("Informe a descrição da peça ou serviço.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NovoValorItem <= 0)
            {
                MessageBox.Show("Informe um valor maior que zero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ItensOrcamento.Add(new ItemOrcamento
            {
                OrcamentoId = OrcamentoAtual.Id,
                Descricao = NovaDescricaoItem.Trim(),
                ValorUnitario = NovoValorItem
            });

            NovaDescricaoItem = string.Empty;
            NovoValorItem = 0;
            AtualizarTotal();
        }

        private void DeletarItem(ItemOrcamento item)
        {
            if (item == null || !ItensOrcamento.Contains(item)) return;

            ItensOrcamento.Remove(item);
            AtualizarTotal();
        }

        private void AtualizarTotal()
        {
            OrcamentoAtual.ValorTotal = ItensOrcamento.Sum(i => i.ValorUnitario);
        }

        private async Task SalvarOrcamentoAsync()
        {
            if (string.IsNullOrWhiteSpace(OrcamentoAtual.ClienteNome))
            {
                MessageBox.Show("Informe o nome do cliente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ItensOrcamento.Any())
            {
                MessageBox.Show("Adicione ao menos uma peça ou serviço ao orçamento.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _processando = true;
                OnPropertyChanged(nameof(BotaoAtivo));

                AtualizarTotal();
                int id = await _repository.SalvarCompletoAsync(OrcamentoAtual, ItensOrcamento.ToList());
                OrcamentoAtual.Id = id;

                MessageBox.Show("Orçamento salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar orçamento: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processando = false;
                OnPropertyChanged(nameof(BotaoAtivo));
            }
        }

        private async Task GerarPdfAsync()
        {
            if (!ItensOrcamento.Any())
            {
                MessageBox.Show("Adicione ao menos uma peça ou serviço antes de gerar o PDF.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "PDF|*.pdf",
                FileName = $"Orcamento_{OrcamentoAtual.ClienteNome}_{OrcamentoAtual.VeiculoPlaca}"
            };

            if (sfd.ShowDialog() != true) return;

            try
            {
                _processando = true;
                OnPropertyChanged(nameof(BotaoAtivo));

                AtualizarTotal();
                var itens = ItensOrcamento.ToList();
                await Task.Run(() => new PdfService().GerarOrcamentoPdf(OrcamentoAtual, itens, sfd.FileName));

                MessageBox.Show("PDF gerado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao gerar PDF: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _processando = false;
                OnPropertyChanged(nameof(BotaoAtivo));
            }
        }
    }
}
