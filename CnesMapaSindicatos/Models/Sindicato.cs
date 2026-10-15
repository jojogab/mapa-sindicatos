namespace CnesMapaSindicatos.Models;

public class Sindicato
{
    public int Id { get; set; }

    // 1. Identificação Oficial (Colunas 1 a 5)
    public string Cnpj { get; set; } = string.Empty;
    public string Cadastro { get; set; } = string.Empty;
    public string Grau { get; set; } = string.Empty; // Sindicato, Federação, Confederação
    public string Denominacao { get; set; } = string.Empty; // Nome oficial no MTE
    public string CodigoSindicalCompleto { get; set; } = string.Empty;

    // 2. Hierarquia e Filiação (Colunas 6 a 8)
    public string Federacao { get; set; } = string.Empty;
    public string Confederacao { get; set; } = string.Empty;
    public string CentralSindical { get; set; } = string.Empty;

    // 3. Dados Eleitorais e Direção (Colunas 9 a 15)
    public string FuncionamentoDirecao { get; set; } = string.Empty;
    public int? NumeroTotalDirigentesEleitos { get; set; }
    public int? NumeroVotosChapaVencedora { get; set; }
    public string FormaEleicao { get; set; } = string.Empty;
    public int? NumeroChapas { get; set; }
    public int? ColegioEleitoral { get; set; }
    public int? TotalVotantes { get; set; }

    // 4. Enquadramento Sindical (Colunas 16 a 19)
    public string AreaGeoeconomica { get; set; } = string.Empty; // Urbana ou Rural
    public string Grupo { get; set; } = string.Empty;            // Empregados, Empregadores, Prof. Liberais...
    public string Classe { get; set; } = string.Empty;           // Indústria, Comércio, Serviços...
    public string Categoria { get; set; } = string.Empty;        // Categoria específica (ex: TI, Metalúrgicos, Educação)

    // 5. Endereço e Contato da Sede (Colunas 20 a 28)
    public string Logradouro { get; set; } = string.Empty;
    public string Complemento { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string LocalidadeSede { get; set; } = string.Empty;   // Município
    public string UfSede { get; set; } = string.Empty;           // Estado (SP, SE, BA, RJ...)
    public string Email { get; set; } = string.Empty;
    public string Telefone1 { get; set; } = string.Empty;

    // 6. Vigência do Mandato e Presidência (Colunas 29 a 31)
    public DateTime? InicioMandato { get; set; }
    public DateTime? FimMandato { get; set; }
    public string NomePresidente { get; set; } = string.Empty;

    // =========================================================================
    // PROPRIEDADES CALCULADAS (Facilitam a exibição nos Cards e no Modal Blazor)
    // =========================================================================

    public bool MandatoVigente => FimMandato.HasValue && FimMandato.Value.Date >= DateTime.Today;

    public string StatusMandato => !FimMandato.HasValue 
        ? "Não Informado" 
        : (MandatoVigente ? "Vigente" : "Vencido");

    public string EnderecoFormatado
    {
        get
        {
            var rua = string.Join(" ", new[] { Logradouro, Numero }
                .Where(parte => !string.IsNullOrWhiteSpace(parte)));
            var cidadeUf = string.Join("/", new[] { LocalidadeSede, UfSede }
                .Where(parte => !string.IsNullOrWhiteSpace(parte)));
            var bairroCidade = string.Join(", ", new[] { Bairro, cidadeUf }
                .Where(parte => !string.IsNullOrWhiteSpace(parte)));
            var endereco = string.Join(", ", new[] { rua, bairroCidade }
                .Where(parte => !string.IsNullOrWhiteSpace(parte)));

            if (!string.IsNullOrWhiteSpace(Complemento))
                endereco = string.Join(", ", new[] { endereco, Complemento }
                    .Where(parte => !string.IsNullOrWhiteSpace(parte)));

            if (!string.IsNullOrWhiteSpace(Cep))
                endereco = string.IsNullOrWhiteSpace(endereco)
                    ? $"CEP: {Cep}"
                    : $"{endereco} - CEP: {Cep}";

            return endereco;
        }
    }

    public double? TaxaParticipacaoEleitoral =>
        (ColegioEleitoral is > 0 && TotalVotantes.HasValue)
            ? Math.Round((double)TotalVotantes.Value / ColegioEleitoral.Value * 100, 1)
            : null;
}