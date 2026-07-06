namespace magal.Models
{
    public class ItemOrcamento : BaseModel
    {
        private string _descricao;
        private decimal _valorUnitario;

        public int Id { get; set; }
        public int OrcamentoId { get; set; }

        public string Descricao
        {
            get => _descricao;
            set { _descricao = value; OnPropertyChanged(); }
        }

        public decimal ValorUnitario
        {
            get => _valorUnitario;
            set { _valorUnitario = value; OnPropertyChanged(); }
        }
    }
}
