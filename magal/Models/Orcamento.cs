using System;

namespace magal.Models
{
    public class Orcamento : BaseModel
    {
        private string _clienteNome;
        private string _clienteTelefone;
        private string _veiculoMarca;
        private string _veiculoModelo;
        private string _veiculoPlaca;
        private string _veiculoCor;
        private int _veiculoAno;
        private int _garantiaDias = 90;
        private string _observacoes;
        private decimal _valorTotal;

        public int Id { get; set; }

        public string ClienteNome
        {
            get => _clienteNome;
            set { _clienteNome = value; OnPropertyChanged(); }
        }

        public string ClienteTelefone
        {
            get => _clienteTelefone;
            set { _clienteTelefone = value; OnPropertyChanged(); }
        }

        public string VeiculoMarca
        {
            get => _veiculoMarca;
            set { _veiculoMarca = value; OnPropertyChanged(); }
        }

        public string VeiculoModelo
        {
            get => _veiculoModelo;
            set { _veiculoModelo = value; OnPropertyChanged(); }
        }

        public string VeiculoPlaca
        {
            get => _veiculoPlaca;
            set { _veiculoPlaca = value; OnPropertyChanged(); }
        }

        public string VeiculoCor
        {
            get => _veiculoCor;
            set { _veiculoCor = value; OnPropertyChanged(); }
        }

        public int VeiculoAno
        {
            get => _veiculoAno;
            set { _veiculoAno = value; OnPropertyChanged(); }
        }

        public int GarantiaDias
        {
            get => _garantiaDias;
            set { _garantiaDias = value; OnPropertyChanged(); }
        }

        public string Observacoes
        {
            get => _observacoes;
            set { _observacoes = value; OnPropertyChanged(); }
        }

        public decimal ValorTotal
        {
            get => _valorTotal;
            set { _valorTotal = value; OnPropertyChanged(); }
        }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
