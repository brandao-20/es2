using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using WebAPI.Entities;
using WebAPI.ExportStrategies;

namespace WebAPI.UnitTests.Services
{
    [TestFixture]
    public class ReportExportServiceTests
    {
        private ReportExportService _svc;

        [SetUp]
        public void SetUp() => _svc = new ReportExportService();

        private List<Relatorio> SampleData() => new()
        {
            new Relatorio
            {
                NomeProduto="X", NomeLoja="Y",
                Preco=9.99m, Data=DateTime.Parse("2025-06-10"),
                NomeCategoria="Cat", ProdutoId=1, LojaId=1, CategoriaId=2, Produto=null, Loja=null, Categoria=null
            }
        };

        [Test]
        public void ExportReport_Csv_Works()
        {
            var csv = _svc.ExportReport(SampleData(), "csv");
            var text = Encoding.UTF8.GetString(csv);
            text.Should().Contain("NomeProduto,NomeLoja,Preco,Data,NomeCategoria");
            text.Should().Contain("X,Y,9.99,2025-06-10,Cat");
        }

        [Test]
        public void ExportReport_Pdf_NotEmpty()
        {
            var pdf = _svc.ExportReport(SampleData(), "pdf");
            pdf.Length.Should().BeGreaterThan(100);
        }

        [Test]
        public void ExportReport_Unknown_Throws()
        {
            Action a = () => _svc.ExportReport(SampleData(), "docx");
            a.Should().Throw<ArgumentException>();
        }
    }
}
