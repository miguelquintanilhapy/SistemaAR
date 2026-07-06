namespace magal.Models
{
    public class ItemOrcamento : BaseModel
    {
        public int Id { get; set; }
        public int OrcamentoId { get; set; }
        public string Descricao { get; set; }
        public decimal ValorUnitario { get; set; }
    }
}