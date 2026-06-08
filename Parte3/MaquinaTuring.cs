using System.Text;

namespace Parte3;

/// <summary>
/// Resultado da execucao da MT sobre uma entrada: guarda o rastro de
/// configuracoes instantaneas percorridas, o veredito e os dados finais
/// (estado de parada, conteudo da fita e numero de passos).
/// </summary>
public sealed class ResultadoExecucao
{
    public string Entrada { get; init; } = string.Empty;
    public bool Aceita { get; init; }
    /// <summary>false quando a MT excedeu o limite de passos (nao parou).</summary>
    public bool Parou { get; init; }
    public int Passos { get; init; }
    public string EstadoFinal { get; init; } = string.Empty;
    /// <summary>Conteudo nao-branco da fita ao final (usado pela MT transdutora).</summary>
    public string FitaFinal { get; init; } = string.Empty;
    public List<string> Rastro { get; init; } = new();
}

/// <summary>
/// Maquina de Turing (MT) deterministica de fita unica.
///
/// Traducao direta da definicao formal do enunciado: a 7-upla
///   M = (Q, Sigma, Gamma, transicao, q0, q_accept, q_reject), onde
///   Q        -> conjunto finito de estados;
///   Sigma    -> alfabeto de entrada (o branco nao pertence a Sigma);
///   Gamma    -> alfabeto da fita, Sigma contido em Gamma e branco pertence a Gamma;
///   transicao -> funcao de transicao parcial: Q x Gamma -> Q x Gamma x {L, R};
///   q0       -> estado inicial;
///   q_accept -> estado de aceitacao;
///   q_reject -> estado de rejeicao (q_reject != q_accept).
///
/// Notacao: as direcoes L (left) e R (right) correspondem a "esquerda" (E) e
/// "direita" (D) dos slides do Prof. Julio Cesar; transicao(q, a) = [q', b, d]
/// le-se "no estado q lendo a, escreve b, move o cabecote para d e vai para q'".
/// Como a transicao e parcial, um par (estado, simbolo) indefinido faz a maquina parar em q_reject.
/// </summary>
public sealed class MaquinaTuring
{
    /// <summary>Simbolo branco da fita.</summary>
    public const char Branco = '_';

    /// <summary>Direcoes do cabecote.</summary>
    public const char Esquerda = 'L';
    public const char Direita = 'R';

    /// <summary>Q — conjunto finito de estados.</summary>
    public HashSet<string> Estados { get; }

    /// <summary>Sigma — alfabeto de entrada.</summary>
    public HashSet<char> Alfabeto { get; }

    /// <summary>Gamma — alfabeto da fita (contem Sigma e o branco).</summary>
    public HashSet<char> AlfabetoFita { get; }

    /// <summary>
    /// Funcao de transicao  transicao : Q x Gamma -> Q x Gamma x {L, R}.
    /// Chave: (estado atual, simbolo lido). Valor: (novo estado, simbolo escrito, direcao).
    /// </summary>
    public Dictionary<(string estado, char simbolo), (string novoEstado, char novoSimbolo, char direcao)> Transicao { get; }

    /// <summary>q0 — estado inicial.</summary>
    public string EstadoInicial { get; }

    /// <summary>q_accept — estado de aceitacao.</summary>
    public string EstadoAceitacao { get; }

    /// <summary>q_reject — estado de rejeicao.</summary>
    public string EstadoRejeicao { get; }

    /// <summary>
    /// Limite de passos da simulacao (configuravel por construcao). Evita que a
    /// maquina entre em laco infinito durante os testes: ao atingir o limite, a
    /// execucao e interrompida e marcada como "nao parou".
    /// </summary>
    public int LimitePassos { get; }

    public MaquinaTuring(
        IEnumerable<string> estados,
        IEnumerable<char> alfabeto,
        IEnumerable<char> alfabetoFita,
        Dictionary<(string estado, char simbolo), (string novoEstado, char novoSimbolo, char direcao)> transicao,
        string estadoInicial,
        string estadoAceitacao,
        string estadoRejeicao,
        int limitePassos = 10_000)
    {
        Estados = new HashSet<string>(estados);
        Alfabeto = new HashSet<char>(alfabeto);
        AlfabetoFita = new HashSet<char>(alfabetoFita);
        Transicao = new Dictionary<(string estado, char simbolo), (string novoEstado, char novoSimbolo, char direcao)>(transicao);
        EstadoInicial = estadoInicial;
        EstadoAceitacao = estadoAceitacao;
        EstadoRejeicao = estadoRejeicao;
        LimitePassos = limitePassos;

        ValidarConsistencia();
    }

    /// <summary>
    /// Confere que a 7-upla e coerente com a definicao formal: branco em Gamma mas
    /// fora de Sigma; Sigma contido em Gamma; estados especiais em Q e distintos;
    /// e toda transicao usando estados de Q, simbolos de Gamma e direcao valida.
    /// </summary>
    private void ValidarConsistencia()
    {
        if (Alfabeto.Contains(Branco))
            throw new ArgumentException("O branco nao pode pertencer a Sigma.");
        if (!AlfabetoFita.Contains(Branco))
            throw new ArgumentException("O branco deve pertencer a Gamma.");
        foreach (char s in Alfabeto)
            if (!AlfabetoFita.Contains(s))
                throw new ArgumentException($"Simbolo de entrada '{s}' nao pertence a Gamma.");

        foreach (string q in new[] { EstadoInicial, EstadoAceitacao, EstadoRejeicao })
            if (!Estados.Contains(q))
                throw new ArgumentException($"Estado '{q}' nao pertence a Q.");
        if (EstadoAceitacao == EstadoRejeicao)
            throw new ArgumentException("q_accept e q_reject devem ser distintos.");

        foreach (var ((origem, lido), (destino, escrito, direcao)) in Transicao)
        {
            if (!Estados.Contains(origem) || !Estados.Contains(destino))
                throw new ArgumentException($"Transicao transicao({origem}, {lido}) usa estado fora de Q.");
            if (!AlfabetoFita.Contains(lido) || !AlfabetoFita.Contains(escrito))
                throw new ArgumentException($"Transicao transicao({origem}, {lido}) usa simbolo fora de Gamma.");
            if (direcao != Esquerda && direcao != Direita)
                throw new ArgumentException($"Direcao invalida '{direcao}' em transicao({origem}, {lido}).");
        }
    }

    /// <summary>
    /// Executa a MT sobre <paramref name="entrada"/>, registrando a configuracao
    /// instantanea a cada passo. A fita e uma estrutura DINAMICA
    /// (Dictionary&lt;int,char&gt;, chave = posicao); celulas nao escritas valem branco.
    /// A maquina para ao alcancar q_accept (aceita) ou q_reject (rejeita); uma
    /// transicao indefinida (funcao parcial) leva implicitamente a q_reject.
    /// </summary>
    public ResultadoExecucao Executar(string entrada)
    {
        var fita = new Dictionary<int, char>();
        for (int i = 0; i < entrada.Length; i++)
            fita[i] = entrada[i];

        int cabecote = 0;
        string estado = EstadoInicial;
        int passos = 0;

        var rastro = new List<string> { Configuracao(estado, fita, cabecote) };

        while (estado != EstadoAceitacao && estado != EstadoRejeicao)
        {
            // Limite de passos: protege contra lacos infinitos durante os testes.
            if (passos >= LimitePassos)
            {
                return new ResultadoExecucao
                {
                    Entrada = entrada,
                    Aceita = false,
                    Parou = false,
                    Passos = passos,
                    EstadoFinal = estado,
                    FitaFinal = ConteudoFita(fita),
                    Rastro = rastro
                };
            }

            char lido = fita.TryGetValue(cabecote, out char c) ? c : Branco;

            if (!Transicao.TryGetValue((estado, lido), out var acao))
            {
                // transicao indefinida: por convencao, a maquina para em q_reject.
                estado = EstadoRejeicao;
                rastro.Add(Configuracao(estado, fita, cabecote));
                break;
            }

            var (novoEstado, novoSimbolo, direcao) = acao;
            fita[cabecote] = novoSimbolo;
            cabecote += direcao == Direita ? 1 : -1;
            estado = novoEstado;
            passos++;

            rastro.Add(Configuracao(estado, fita, cabecote));
        }

        bool aceita = estado == EstadoAceitacao;
        return new ResultadoExecucao
        {
            Entrada = entrada,
            Aceita = aceita,
            Parou = true,
            Passos = passos,
            EstadoFinal = estado,
            FitaFinal = ConteudoFita(fita),
            Rastro = rastro
        };
    }

    /// <summary>
    /// Imprime no console a definicao formal (7-upla) e a tabela da funcao de
    /// transicao na notacao dos slides:  transicao(q, a) = [q', b, d].
    /// </summary>
    public void ExibirDefinicao()
    {
        Console.WriteLine("Definicao formal  M = (Q, Sigma, Gamma, transicao, q0, q_accept, q_reject):");
        Console.WriteLine($"  Q        = {{ {string.Join(", ", Estados.OrderBy(e => e))} }}");
        Console.WriteLine($"  Sigma    = {{ {string.Join(", ", Alfabeto.OrderBy(c => c))} }}");
        Console.WriteLine($"  Gamma    = {{ {string.Join(", ", AlfabetoFita.OrderBy(c => c))} }}   (branco = '{Branco}')");
        Console.WriteLine($"  q0       = {EstadoInicial}");
        Console.WriteLine($"  q_accept = {EstadoAceitacao}");
        Console.WriteLine($"  q_reject = {EstadoRejeicao}");
        Console.WriteLine($"  limite   = {LimitePassos} passos");
        Console.WriteLine("  transicao (E = esquerda/L, D = direita/R):");
        foreach (var ((q, a), (q2, b, d)) in Transicao.OrderBy(t => t.Key.estado).ThenBy(t => t.Key.simbolo))
            Console.WriteLine($"    transicao({q}, {a}) = [{q2}, {b}, {d}]");
        Console.WriteLine();
    }

    // ----- auxiliares de formatacao -----

    // Configuracao instantanea: estado + fita (com [ ] ao redor do simbolo sob o
    // cabecote) + posicao do cabecote.
    private string Configuracao(string estado, Dictionary<int, char> fita, int cabecote)
        => $"estado={estado,-8} fita={FitaComCabecote(fita, cabecote),-22} cabecote={cabecote}";

    // Renderiza a fita do menor ao maior indice escrito (incluindo o cabecote),
    // destacando com [ ] a celula sob o cabecote.
    private static string FitaComCabecote(Dictionary<int, char> fita, int cabecote)
    {
        int lo = cabecote, hi = cabecote;
        foreach (int k in fita.Keys)
        {
            if (k < lo) lo = k;
            if (k > hi) hi = k;
        }

        var sb = new StringBuilder();
        for (int i = lo; i <= hi; i++)
        {
            char c = fita.TryGetValue(i, out char v) ? v : Branco;
            sb.Append(i == cabecote ? $"[{c}]" : c.ToString());
        }
        return sb.ToString();
    }

    // Conteudo util da fita (sem brancos nas pontas) — usado para a saida da MT transdutora.
    private static string ConteudoFita(Dictionary<int, char> fita)
    {
        if (fita.Count == 0) return "(vazia)";
        int lo = fita.Keys.Min(), hi = fita.Keys.Max();

        var sb = new StringBuilder();
        for (int i = lo; i <= hi; i++)
            sb.Append(fita.TryGetValue(i, out char v) ? v : Branco);

        string conteudo = sb.ToString().Trim(Branco);
        return conteudo.Length == 0 ? "(branco)" : conteudo;
    }
}
