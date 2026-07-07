using magal.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Globalization;

namespace magal.Services
{
    /// <summary>
    /// Serviço responsável por gerar o PDF do orçamento no modelo utilizado pela Ice Car.
    /// </summary>
    public class PdfService
    {
        private static readonly CultureInfo _ptBR = new CultureInfo("pt-BR");

        private const string EmpresaNome = "Ice Car";
        private const string EmpresaResponsavel = "58.385.817 YAGO LOPES FERNANDES";
        private const string EmpresaCnpj = "CNPJ: 58.385.817/0001-05";
        private const string EmpresaEndereco1 = "Rua Aldo Moreira dos Santos, 498, Ice Car Ar Condicionado Automotivo";
        private const string EmpresaEndereco2 = "Jardim Santa Júlia, São José dos Campos-SP - CEP 12228-305";
        private const string EmpresaEmail = "icecarsjc@gmail.com";
        private const string EmpresaTelefone = "+55 (12) 98893-0176";
        private const string EmpresaRedesSociais = "Instagram: icecar_sjc   |   Facebook: IceCar";
        private const string MeiosPagamento = "Dinheiro, cartão de crédito, cartão de débito ou pix.";
        private const string ValidadeOrcamento = "Orçamentos válidos por 10 dias.";
        private const string CorPrimaria = "#14607D";

        static PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void GerarOrcamentoPdf(Orcamento orcamento, List<ItemOrcamento> itens, string caminhoArquivo)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0.8f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col => ConstruirCabecalho(col, orcamento));
                    page.Content().PaddingTop(16).Column(col => ConstruirConteudo(col, orcamento, itens));
                    page.Footer().BorderTop(1).BorderColor("#CBD5E1").PaddingTop(8).Row(row => ConstruirRodape(row));
                });
            }).GeneratePdf(caminhoArquivo);
        }

        private void ConstruirCabecalho(ColumnDescriptor col, Orcamento orcamento)
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(EmpresaNome).FontSize(18).Bold().FontColor(CorPrimaria);
                    c.Item().Text(EmpresaResponsavel).FontSize(9).FontColor("#555555");
                    c.Item().Text(EmpresaCnpj).FontSize(9).FontColor("#555555");
                    c.Item().Text(EmpresaEndereco1).FontSize(9).FontColor("#555555");
                    c.Item().Text(EmpresaEndereco2).FontSize(9).FontColor("#555555");
                });

                row.ConstantItem(200).Column(c =>
                {
                    c.Item().AlignRight().Text(EmpresaEmail).FontSize(9).FontColor("#555555");
                    c.Item().AlignRight().Text(EmpresaTelefone).FontSize(9).FontColor("#555555");
                    c.Item().PaddingTop(4).AlignRight().Text(EmpresaRedesSociais).FontSize(8).FontColor("#999999");
                    c.Item().PaddingTop(4).AlignRight().Text($"Data: {orcamento.DataCriacao:dd/MM/yyyy}").FontSize(9).Bold().FontColor(CorPrimaria);
                });
            });

            col.Item().PaddingTop(12).Background(CorPrimaria).Padding(10)
                .Text($"Orçamento {orcamento.Id}-{orcamento.DataCriacao.Year}").FontSize(14).Bold().FontColor(Colors.White);

            col.Item().PaddingTop(10).Column(c =>
            {
                c.Item().Text($"Cliente: {orcamento.ClienteNome}").FontSize(11).Bold();
                if (!string.IsNullOrWhiteSpace(orcamento.ClienteTelefone))
                    c.Item().Text(orcamento.ClienteTelefone).FontSize(9).FontColor("#555555");
            });
        }

        private void ConstruirConteudo(ColumnDescriptor col, Orcamento orcamento, List<ItemOrcamento> itens)
        {
            col.Item().PaddingTop(6).Background(CorPrimaria).Padding(6).Text("INFORMAÇÕES BÁSICAS").FontColor(Colors.White).Bold().FontSize(10);

            col.Item().PaddingTop(8).PaddingBottom(14).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                });

                void Campo(string rotulo, string valor)
                {
                    table.Cell().Column(cc =>
                    {
                        cc.Item().Text(rotulo).FontSize(8).Bold().FontColor("#999999");
                        cc.Item().Text(string.IsNullOrWhiteSpace(valor) ? "-" : valor).FontSize(11).Bold();
                    });
                }

                Campo("MARCA", orcamento.VeiculoMarca);
                Campo("MODELO", orcamento.VeiculoModelo);
                Campo("PLACA", orcamento.VeiculoPlaca);
                Campo("COR", orcamento.VeiculoCor);
                Campo("ANO", orcamento.VeiculoAno > 0 ? orcamento.VeiculoAno.ToString() : null);
            });

            col.Item().Background(CorPrimaria).Padding(6).Text("SERVIÇOS").FontColor(Colors.White).Bold().FontSize(10);

            col.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(4); c.RelativeColumn(1); });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderColor("#CBD5E1").Padding(6).Text("DESCRIÇÃO").Bold().FontSize(9).FontColor("#555555");
                    header.Cell().BorderBottom(1).BorderColor("#CBD5E1").Padding(6).AlignRight().Text("PREÇO").Bold().FontSize(9).FontColor("#555555");
                });

                foreach (var item in itens)
                {
                    table.Cell().BorderBottom(1).BorderColor("#E8EDF2").Padding(6).Text(item.Descricao).FontSize(10);
                    table.Cell().BorderBottom(1).BorderColor("#E8EDF2").Padding(6).AlignRight().Text(item.ValorUnitario.ToString("C2", _ptBR)).FontSize(10).Bold();
                }
            });

            col.Item().PaddingTop(6).Background(CorPrimaria).Padding(8).Row(row =>
            {
                row.RelativeItem().Text("Total").FontColor(Colors.White).Bold().FontSize(11);
                row.ConstantItem(120).AlignRight().Text(orcamento.ValorTotal.ToString("C2", _ptBR)).FontColor(Colors.White).Bold().FontSize(11);
            });

            col.Item().PaddingTop(18).Background(CorPrimaria).Padding(6).Text("PAGAMENTO").FontColor(Colors.White).Bold().FontSize(10);
            col.Item().PaddingTop(6).Text(c =>
            {
                c.Span("Meios de pagamento: ").Bold().FontSize(9);
                c.Span(MeiosPagamento).FontSize(9);
            });

            col.Item().PaddingTop(14).Background(CorPrimaria).Padding(6).Text("GARANTIA").FontColor(Colors.White).Bold().FontSize(10);
            col.Item().PaddingTop(6).Text(c =>
            {
                c.Span("Período de garantia: ").Bold().FontSize(9);
                c.Span($"{orcamento.GarantiaDias} dias").FontSize(9);
            });

            col.Item().PaddingTop(14).Background(CorPrimaria).Padding(6).Text("INFORMAÇÕES ADICIONAIS").FontColor(Colors.White).Bold().FontSize(10);
            col.Item().PaddingTop(6).Column(c =>
            {
                if (!string.IsNullOrWhiteSpace(orcamento.Observacoes))
                    c.Item().Text(orcamento.Observacoes).FontSize(9);

                c.Item().PaddingTop(4).Text(ValidadeOrcamento).FontSize(9);
            });

            col.Item().PaddingTop(60).Row(row =>
            {
                row.RelativeItem();
                row.ConstantItem(260).Column(c =>
                {
                    c.Item().BorderBottom(1).BorderColor("#A0AEC0").PaddingBottom(2);
                    c.Item().PaddingTop(4).AlignCenter().Text(EmpresaNome).FontSize(9).Bold();
                });
            });
        }

        private void ConstruirRodape(RowDescriptor row)
        {
            row.RelativeItem().Text($"{EmpresaResponsavel} - {EmpresaCnpj}").FontSize(7).FontColor("#999999");
            row.ConstantItem(80).AlignRight().Text(x =>
            {
                x.Span("Página ").FontSize(7);
                x.CurrentPageNumber().FontSize(7);
                x.Span(" de ").FontSize(7);
                x.TotalPages().FontSize(7);
            });
        }
    }
}
