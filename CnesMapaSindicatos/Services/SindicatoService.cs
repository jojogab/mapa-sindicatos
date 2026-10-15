using ClosedXML.Excel;
using CnesMapaSindicatos.Models;
using System.Globalization;

namespace CnesMapaSindicatos.Services;

public class SindicatoService
{
    private List<Sindicato> _sindicatos = [];
    private readonly IWebHostEnvironment _env;

    public SindicatoService(IWebHostEnvironment env)
    {
        _env = env;
        CarregarDados();
    }

    private void CarregarDados()
    {
        var caminhoPlanilha = Path.Combine(
            _env.ContentRootPath,
            "DataTeste",
            "SINDICATOSBRASIL14042025.xlsx");

        if (!File.Exists(caminhoPlanilha))
        {
            throw new FileNotFoundException(
                "A planilha de sindicatos não foi encontrada.",
                caminhoPlanilha);
        }

        _sindicatos = ImportarPlanilhaXlsx(caminhoPlanilha);
    }

    // Lê as 31 colunas exatas do seu arquivo .xlsx do MTE
    public List<Sindicato> ImportarPlanilhaXlsx(string caminhoArquivo)
    {
        var lista = new List<Sindicato>();
        using var workbook = new XLWorkbook(caminhoArquivo);
        var worksheet = workbook.Worksheets.First();
        var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1); // Pula a linha 1 (cabeçalho)

        if (rows is null) return lista;

        int id = 1;
        foreach (var row in rows)
        {
            lista.Add(new Sindicato
            {
                Id = id++,
                Cnpj = LerTexto(row, 1),
                Cadastro = LerTexto(row, 2),
                Grau = LerTexto(row, 3),
                Denominacao = LerTexto(row, 4),
                CodigoSindicalCompleto = LerTexto(row, 5),
                Federacao = LerTexto(row, 6),
                Confederacao = LerTexto(row, 7),
                CentralSindical = LerTexto(row, 8),
                FuncionamentoDirecao = LerTexto(row, 9),
                NumeroTotalDirigentesEleitos = LerInteiro(row, 10),
                NumeroVotosChapaVencedora = LerInteiro(row, 11),
                FormaEleicao = LerTexto(row, 12),
                NumeroChapas = LerInteiro(row, 13),
                ColegioEleitoral = LerInteiro(row, 14),
                TotalVotantes = LerInteiro(row, 15),
                AreaGeoeconomica = LerTexto(row, 16),
                Grupo = LerTexto(row, 17),
                Classe = LerTexto(row, 18),
                Categoria = LerTexto(row, 19),
                Logradouro = LerTexto(row, 20),
                Complemento = LerTexto(row, 21),
                Numero = LerTexto(row, 22),
                Bairro = LerTexto(row, 23),
                Cep = LerTexto(row, 24),
                LocalidadeSede = LerTexto(row, 25),
                UfSede = LerTexto(row, 26).ToUpperInvariant(),
                Email = LerTexto(row, 27),
                Telefone1 = LerTexto(row, 28),
                InicioMandato = LerData(row, 29),
                FimMandato = LerData(row, 30),
                NomePresidente = LerTexto(row, 31)
            });
        }

        return lista;
    }

    // Método principal de busca e filtragem cruzada
    public IEnumerable<Sindicato> Filtrar(
        string? busca = null,
        string? uf = null,
        string? grau = null,
        string? areaGeoeconomica = null,
        string? grupo = null,
        string? classe = null,
        string? categoria = null,
        string? centralSindical = null,
        string? cadastro = null,
        string? federacao = null,
        string? confederacao = null,
        string? funcionamentoDirecao = null,
        string? formaEleicao = null,
        bool apenasMandatoVigente = false)
    {
        var query = _sindicatos.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(uf))
            query = query.Where(s => s.UfSede.Equals(uf, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(grau))
            query = query.Where(s => s.Grau.Equals(grau, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(areaGeoeconomica))
            query = query.Where(s => s.AreaGeoeconomica.Equals(areaGeoeconomica, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(grupo))
            query = query.Where(s => s.Grupo.Equals(grupo, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(classe))
            query = query.Where(s => s.Classe.Equals(classe, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(s => s.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(centralSindical))
            query = query.Where(s => s.CentralSindical.Equals(centralSindical, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(cadastro))
            query = query.Where(s => s.Cadastro.Equals(cadastro, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(federacao))
            query = query.Where(s => s.Federacao.Equals(federacao, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(confederacao))
            query = query.Where(s => s.Confederacao.Equals(confederacao, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(funcionamentoDirecao))
            query = query.Where(s => s.FuncionamentoDirecao.Equals(funcionamentoDirecao, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(formaEleicao))
            query = query.Where(s => s.FormaEleicao.Equals(formaEleicao, StringComparison.OrdinalIgnoreCase));

        if (apenasMandatoVigente)
            query = query.Where(s => s.MandatoVigente);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(s => CamposPesquisa(s).Any(campo =>
                campo.Contains(termo, StringComparison.OrdinalIgnoreCase)));
        }

        return query.OrderBy(s => s.Denominacao).ToList();
    }

    // Métodos auxiliares para popular os Dropdowns de filtro dinamicamente no Blazor
    public List<string> ObterGraus() => ExtrairUnicos(s => s.Grau);
    public List<string> ObterCadastros() => ExtrairUnicos(s => s.Cadastro);
    public List<string> ObterAreasGeoeconomicas() => ExtrairUnicos(s => s.AreaGeoeconomica);
    public List<string> ObterGrupos() => ExtrairUnicos(s => s.Grupo);
    public List<string> ObterClasses() => ExtrairUnicos(s => s.Classe);
    public List<string> ObterCategorias() => ExtrairUnicos(s => s.Categoria);
    public List<string> ObterCentraisSindicais() => ExtrairUnicos(s => s.CentralSindical);
    public List<string> ObterFederacoes() => ExtrairUnicos(s => s.Federacao);
    public List<string> ObterConfederacoes() => ExtrairUnicos(s => s.Confederacao);
    public List<string> ObterFuncionamentosDirecao() => ExtrairUnicos(s => s.FuncionamentoDirecao);
    public List<string> ObterFormasEleicao() => ExtrairUnicos(s => s.FormaEleicao);
    public List<string> ObterUfs() => ExtrairUnicos(s => s.UfSede);

    // Retorna o total de sindicatos por UF (ótimo para pintar o mapa de calor no Leaflet!)
    public Dictionary<string, int> ObterContagemPorUf() =>
        _sindicatos
            .Where(s => !string.IsNullOrWhiteSpace(s.UfSede))
            .GroupBy(s => s.UfSede)
            .ToDictionary(g => g.Key, g => g.Count());

    private List<string> ExtrairUnicos(Func<Sindicato, string> selector) =>
        _sindicatos
            .Select(selector)
            .Where(val => !string.IsNullOrWhiteSpace(val))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(val => val)
            .ToList();

    private static IEnumerable<string> CamposPesquisa(Sindicato sindicato)
    {
        yield return sindicato.Cnpj;
        yield return sindicato.Cadastro;
        yield return sindicato.Grau;
        yield return sindicato.Denominacao;
        yield return sindicato.CodigoSindicalCompleto;
        yield return sindicato.Federacao;
        yield return sindicato.Confederacao;
        yield return sindicato.CentralSindical;
        yield return sindicato.FuncionamentoDirecao;
        yield return sindicato.FormaEleicao;
        yield return sindicato.AreaGeoeconomica;
        yield return sindicato.Grupo;
        yield return sindicato.Classe;
        yield return sindicato.Categoria;
        yield return sindicato.Logradouro;
        yield return sindicato.Complemento;
        yield return sindicato.Numero;
        yield return sindicato.Bairro;
        yield return sindicato.Cep;
        yield return sindicato.LocalidadeSede;
        yield return sindicato.UfSede;
        yield return sindicato.Email;
        yield return sindicato.Telefone1;
        yield return sindicato.NomePresidente;
        yield return sindicato.NumeroTotalDirigentesEleitos?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        yield return sindicato.NumeroVotosChapaVencedora?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        yield return sindicato.NumeroChapas?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        yield return sindicato.ColegioEleitoral?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        yield return sindicato.TotalVotantes?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        if (sindicato.InicioMandato.HasValue)
        {
            yield return sindicato.InicioMandato.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            yield return sindicato.InicioMandato.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        if (sindicato.FimMandato.HasValue)
        {
            yield return sindicato.FimMandato.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            yield return sindicato.FimMandato.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }

    // Helpers de leitura segura do Excel
    private static string LerTexto(IXLRangeRow row, int col)
    {
        var val = row.Cell(col).GetString()?.Trim() ?? string.Empty;
        return (val == "-" || val.Equals("NÃO INFORMADO", StringComparison.OrdinalIgnoreCase)) ? string.Empty : val;
    }

    private static int? LerInteiro(IXLRangeRow row, int col)
    {
        var cell = row.Cell(col);
        if (cell.IsEmpty()) return null;
        if (cell.TryGetValue<int>(out var val)) return val;
        var txt = cell.GetString().Replace(".", "").Trim();
        return int.TryParse(txt, out var parsed) ? parsed : null;
    }

    private static DateTime? LerData(IXLRangeRow row, int col)
    {
        var cell = row.Cell(col);
        if (cell.IsEmpty()) return null;
        if (cell.TryGetValue<DateTime>(out var dt)) return dt;
        var txt = cell.GetString().Trim();
        return DateTime.TryParseExact(txt, ["dd/MM/yyyy", "yyyy-MM-dd"], new CultureInfo("pt-BR"), DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

}