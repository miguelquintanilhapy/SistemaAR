using System;

namespace magal.Models
{
    public class Orcamento : BaseModel
    {
        public int Id { get; set; }
        public string ClienteNome { get; set; }
        public string ClienteTelefone { get; set; }
        public string VeiculoMarca { get; set; }
        public string VeiculoModelo { get; set; }
        public string VeiculoPlaca { get; set; }
        public string VeiculoCor { get; set; }
        public int VeiculoAno { get; set; }
        public int GarantiaDias { get; set; } = 90;
        public string Observacoes { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}