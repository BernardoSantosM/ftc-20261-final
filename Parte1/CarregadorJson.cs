using System.Text.Json;
using System.Text.Json.Serialization;

namespace Parte1;

/// <summary>
/// Espelho do esquema do arquivo afd.json descrito no enunciado:
///   { estados, alfabeto, estadoInicial, estadosAceitacao,
///     transicoes: [ { origem, simbolo, destino }, ... ] }
/// Usado apenas para desserializar a configuracao com System.Text.Json (biblioteca
/// nativa do .NET — nenhuma biblioteca externa de automatos e utilizada).
/// </summary>
internal sealed class AfdConfig
{
    [JsonPropertyName("estados")]
    public List<string> Estados { get; set; } = new();

    [JsonPropertyName("alfabeto")]
    public List<string> Alfabeto { get; set; } = new();

    [JsonPropertyName("estadoInicial")]
    public string EstadoInicial { get; set; } = string.Empty;

    [JsonPropertyName("estadosAceitacao")]
    public List<string> EstadosAceitacao { get; set; } = new();

    [JsonPropertyName("transicoes")]
    public List<TransicaoConfig> Transicoes { get; set; } = new();
}

internal sealed class TransicaoConfig
{
    [JsonPropertyName("origem")]
    public string Origem { get; set; } = string.Empty;

    [JsonPropertyName("simbolo")]
    public string Simbolo { get; set; } = string.Empty;

    [JsonPropertyName("destino")]
    public string Destino { get; set; } = string.Empty;
}

/// <summary>
/// Desafio obrigatorio (item f): carregar QUALQUER AFD a partir de um arquivo de
/// configuracao afd.json e construir o objeto <see cref="AFD"/> correspondente.
/// </summary>
public static class CarregadorJson
{
    public static AFD Carregar(string caminho)
    {
        if (!File.Exists(caminho))
            throw new FileNotFoundException($"Arquivo de configuracao nao encontrado: {caminho}");

        string conteudo = File.ReadAllText(caminho);

        var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        AfdConfig? config = JsonSerializer.Deserialize<AfdConfig>(conteudo, opcoes);

        if (config is null)
            throw new InvalidDataException("Falha ao interpretar o afd.json (conteudo nulo).");

        // Cada simbolo do alfabeto deve ser um unico caractere.
        var alfabeto = new List<char>();
        foreach (string s in config.Alfabeto)
        {
            if (s.Length != 1)
                throw new InvalidDataException($"Simbolo de alfabeto invalido no JSON: '{s}' (esperado 1 caractere).");
            alfabeto.Add(s[0]);
        }

        // Monta delta a partir da lista de transicoes.
        var transicao = new Dictionary<(string, char), string>();
        foreach (TransicaoConfig t in config.Transicoes)
        {
            if (t.Simbolo.Length != 1)
                throw new InvalidDataException($"Simbolo de transicao invalido no JSON: '{t.Simbolo}'.");

            var chave = (t.Origem, t.Simbolo[0]);
            if (transicao.ContainsKey(chave))
                throw new InvalidDataException(
                    $"AFD nao deterministico no JSON: ha duas transicoes para (delta({t.Origem}, {t.Simbolo})).");

            transicao[chave] = t.Destino;
        }

        // O construtor de AFD valida a coerencia da 5-upla resultante.
        return new AFD(
            config.Estados,
            alfabeto,
            transicao,
            config.EstadoInicial,
            config.EstadosAceitacao);
    }
}
