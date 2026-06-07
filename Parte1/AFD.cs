using System.Text;

namespace Parte1;

/// <summary>
/// Resultado da execucao do AFD sobre uma cadeia de entrada.
/// Guarda o rastro de estados percorridos (configuracoes), o veredito
/// (ACEITA/REJEITA) e, se houver, o motivo de uma rejeicao por erro
/// (por exemplo, um simbolo fora do alfabeto).
/// </summary>
public sealed class ResultadoExecucao
{
    public string Cadeia { get; init; } = string.Empty;
    public List<string> Rastro { get; init; } = new();
    public bool Aceita { get; init; }
    public string? Observacao { get; init; }
}

/// <summary>
/// Automato Finito Deterministico (AFD).
///
/// Traducao direta da definicao formal vista em aula (Slides 3 - Prof. Julio Cesar):
/// um AFD e a 5-upla  M = (Q, Sigma, delta, q0, F), onde
///   Q     -> conjunto finito de estados;
///   Sigma -> alfabeto de entrada (simbolos permitidos);
///   delta -> funcao de transicao  delta : Q x Sigma -> Q;
///   q0    -> estado inicial, q0 pertence a Q;
///   F     -> conjunto de estados finais (de aceitacao), F contido em Q.
///
/// Cada campo abaixo recebe o nome exato do elemento matematico que representa.
/// </summary>
public sealed class AFD
{
    /// <summary>Q — conjunto finito de estados.</summary>
    public HashSet<string> Estados { get; }

    /// <summary>Sigma — alfabeto de entrada.</summary>
    public HashSet<char> Alfabeto { get; }

    /// <summary>
    /// delta — funcao de transicao  delta : Q x Sigma -> Q.
    /// A chave (estado, simbolo) representa o par de entrada; o valor, o estado de destino.
    /// </summary>
    public Dictionary<(string estado, char simbolo), string> Transicao { get; }

    /// <summary>q0 — estado inicial.</summary>
    public string EstadoInicial { get; }

    /// <summary>F — conjunto de estados de aceitacao.</summary>
    public HashSet<string> EstadosAceitacao { get; }

    public AFD(
        IEnumerable<string> estados,
        IEnumerable<char> alfabeto,
        Dictionary<(string, char), string> transicao,
        string estadoInicial,
        IEnumerable<string> estadosAceitacao)
    {
        Estados = new HashSet<string>(estados);
        Alfabeto = new HashSet<char>(alfabeto);
        Transicao = new Dictionary<(string, char), string>(transicao);
        EstadoInicial = estadoInicial;
        EstadosAceitacao = new HashSet<string>(estadosAceitacao);

        ValidarConsistencia();
    }

    /// <summary>
    /// Confere que a 5-upla e coerente: estado inicial e finais existem em Q
    /// e toda transicao usa estados de Q e simbolos de Sigma.
    /// </summary>
    private void ValidarConsistencia()
    {
        if (!Estados.Contains(EstadoInicial))
            throw new ArgumentException($"Estado inicial '{EstadoInicial}' nao pertence a Q.");

        foreach (string f in EstadosAceitacao)
            if (!Estados.Contains(f))
                throw new ArgumentException($"Estado de aceitacao '{f}' nao pertence a Q.");

        foreach (var ((origem, simbolo), destino) in Transicao)
        {
            if (!Estados.Contains(origem))
                throw new ArgumentException($"Transicao com origem '{origem}' fora de Q.");
            if (!Estados.Contains(destino))
                throw new ArgumentException($"Transicao com destino '{destino}' fora de Q.");
            if (!Alfabeto.Contains(simbolo))
                throw new ArgumentException($"Transicao com simbolo '{simbolo}' fora de Sigma.");
        }
    }

    /// <summary>
    /// Simula a leitura da cadeia simbolo a simbolo e devolve o resultado completo
    /// (rastro de estados + veredito). Implementa, de forma iterativa, a funcao de
    /// transicao estendida  delta^(q0, w):
    ///     delta^(q, lambda) = q
    ///     delta^(q, w.a)    = delta(delta^(q, w), a)
    ///
    /// Como delta nao e total, uma transicao indefinida (ou um simbolo fora de Sigma)
    /// leva a um "estado de poco" implicito: a cadeia e imediatamente REJEITADA.
    /// </summary>
    public ResultadoExecucao Processar(string cadeia)
    {
        string estadoAtual = EstadoInicial;
        var rastro = new List<string> { estadoAtual };

        for (int i = 0; i < cadeia.Length; i++)
        {
            char simbolo = cadeia[i];

            // Simbolo fora do alfabeto: entrada invalida -> rejeita.
            if (!Alfabeto.Contains(simbolo))
            {
                return new ResultadoExecucao
                {
                    Cadeia = cadeia,
                    Rastro = rastro,
                    Aceita = false,
                    Observacao = $"simbolo '{simbolo}' nao pertence ao alfabeto Sigma"
                };
            }

            // Transicao indefinida (delta parcial): cai no estado de poco -> rejeita.
            if (!Transicao.TryGetValue((estadoAtual, simbolo), out string? proximo))
            {
                return new ResultadoExecucao
                {
                    Cadeia = cadeia,
                    Rastro = rastro,
                    Aceita = false,
                    Observacao = $"nao ha transicao definida para (delta({estadoAtual}, {simbolo}))"
                };
            }

            estadoAtual = proximo;
            rastro.Add(estadoAtual);
        }

        // Aceita se, ao consumir toda a cadeia, parou em um estado final (delta^(q0, w) pertence a F).
        bool aceita = EstadosAceitacao.Contains(estadoAtual);
        return new ResultadoExecucao
        {
            Cadeia = cadeia,
            Rastro = rastro,
            Aceita = aceita
        };
    }

    /// <summary>
    /// Decide se a cadeia pertence a linguagem L(M) = { w em Sigma* | delta^(q0, w) pertence a F }.
    /// Exigencia (c) do enunciado: metodo  bool Aceitar(string cadeia).
    /// </summary>
    public bool Aceitar(string cadeia) => Processar(cadeia).Aceita;

    /// <summary>
    /// Exigencia (e) do enunciado: imprime no console uma representacao textual
    /// da tabela de transicoes do AFD (a "funcao programa como tabela" dos slides).
    /// O estado inicial e marcado com "->" e os estados finais com "*".
    /// </summary>
    public void ExibirDiagrama()
    {
        var alfabetoOrdenado = Alfabeto.OrderBy(c => c).ToList();
        var estadosOrdenados = Estados.OrderBy(e => e).ToList();

        Console.WriteLine("Tabela de transicoes (delta):");
        Console.WriteLine($"  Q     = {{ {string.Join(", ", estadosOrdenados)} }}");
        Console.WriteLine($"  Sigma = {{ {string.Join(", ", alfabetoOrdenado)} }}");
        Console.WriteLine($"  q0    = {EstadoInicial}");
        Console.WriteLine($"  F     = {{ {string.Join(", ", EstadosAceitacao.OrderBy(e => e))} }}");
        Console.WriteLine();

        // Cabecalho da tabela.
        var cabecalho = new StringBuilder("        | estado | ");
        foreach (char simbolo in alfabetoOrdenado)
            cabecalho.Append($"   {simbolo}    | ");
        Console.WriteLine(cabecalho.ToString());
        Console.WriteLine("        " + new string('-', 9 + alfabetoOrdenado.Count * 9));

        // Uma linha por estado. Prefixos: "->" inicial, "*" final.
        foreach (string estado in estadosOrdenados)
        {
            string marca = (estado == EstadoInicial ? "->" : "  ")
                         + (EstadosAceitacao.Contains(estado) ? "*" : " ");
            var linha = new StringBuilder($"   {marca}  | {estado,-6} | ");
            foreach (char simbolo in alfabetoOrdenado)
            {
                string destino = Transicao.TryGetValue((estado, simbolo), out string? d) ? d : "-";
                linha.Append($" {destino,-6} | ");
            }
            Console.WriteLine(linha.ToString());
        }
        Console.WriteLine();
    }
}
