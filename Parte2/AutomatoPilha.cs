using System.Text;

namespace Parte2;

/// <summary>
/// Autômato de Pilha (AP) com reconhecimento por PILHA VAZIA.
///
/// Tradução direta da definição formal exigida no enunciado: a 7-upla
///   M = (Q, Σ, Γ, δ, q0, Z0, ∅)
/// onde
///   Q   -> conjunto finito de estados;
///   Σ   -> alfabeto de entrada;
///   Γ   -> alfabeto da pilha;
///   δ   -> função de transição (parcial, possivelmente NÃO determinística);
///   q0  -> estado inicial;
///   Z0  -> símbolo que inicia a pilha (marca o fundo);
///   ∅   -> o conjunto de estados de aceitação é vazio: aceita-se por PILHA VAZIA.
///
/// Notação das transições segue os slides do professor (FTC05):
///   δ(e, a, b) = [e', z]  significa  "no estado e, lendo a, desempilhando b,
///   vai para e' empilhando z". A palavra vazia (λ) é representada por '\0'.
///   - a = λ ('\0'): transição não consome símbolo da entrada (λ-movimento);
///   - b = λ ('\0'): o topo da pilha é ignorado (não desempilha);
///   - z = λ ('\0'/""): nada é empilhado.
/// </summary>
public sealed class AutomatoPilha
{
    /// <summary>Representação de λ (palavra/símbolo vazio) no código.</summary>
    public const char Lambda = '\0';

    /// <summary>Limite de passos da simulação (evita laços infinitos de λ-movimentos).</summary>
    private const int LimitePassos = 20000;

    public HashSet<string> Estados { get; }              // Q
    public HashSet<char> Alfabeto { get; }               // Σ
    public HashSet<char> AlfabetoPilha { get; }          // Γ
    public string EstadoInicial { get; }                 // q0
    public char SimboloInicialPilha { get; }             // Z0

    /// <summary>
    /// δ — chave: (estado, símbolo de entrada, símbolo do topo); valor: lista de
    /// destinos (novo estado, string a empilhar). A lista permite NÃO determinismo.
    /// </summary>
    public Dictionary<(string estado, char entrada, char topo), List<(string novoEstado, string empilha)>> Transicao { get; }

    public AutomatoPilha(
        IEnumerable<string> estados,
        IEnumerable<char> alfabeto,
        IEnumerable<char> alfabetoPilha,
        Dictionary<(string, char, char), List<(string, string)>> transicao,
        string estadoInicial,
        char simboloInicialPilha)
    {
        Estados = new HashSet<string>(estados);
        Alfabeto = new HashSet<char>(alfabeto);
        AlfabetoPilha = new HashSet<char>(alfabetoPilha);
        Transicao = transicao;
        EstadoInicial = estadoInicial;
        SimboloInicialPilha = simboloInicialPilha;

        if (!Estados.Contains(EstadoInicial))
            throw new ArgumentException($"Estado inicial '{EstadoInicial}' nao pertence a Q.");
        if (!AlfabetoPilha.Contains(SimboloInicialPilha))
            throw new ArgumentException("Simbolo inicial da pilha (Z0) nao pertence a Gamma.");
    }

    /// <summary>
    /// Decide se a cadeia é aceita por pilha vazia. Como o AP pode ser não
    /// determinístico, faz uma busca em profundidade (DFS) sobre as configurações
    /// instantâneas. Em <paramref name="computacao"/> devolve a sequência de
    /// configurações da computação que aceita (ou o ramo explorado, se rejeita).
    /// </summary>
    public bool Aceitar(string cadeia, out List<string> computacao)
    {
        var caminho = new List<string>();   // ramo atual da DFS
        var melhor = new List<string>();    // ramo mais profundo já visto (para exibir em rejeições)
        var visitados = new HashSet<string>();

        // A pilha começa contendo apenas o símbolo inicial Z0.
        var pilha = new Stack<char>();
        pilha.Push(SimboloInicialPilha);

        int passos = 0;
        bool aceita = Buscar(EstadoInicial, cadeia, 0, pilha, caminho, melhor, visitados, ref passos);

        // Se aceitou, devolve a computação aceitadora; senão, o ramo mais longo explorado.
        computacao = aceita ? caminho : melhor;
        return aceita;
    }

    // DFS recursiva sobre configurações (estado, posição na entrada, pilha).
    private bool Buscar(string estado, string cadeia, int pos, Stack<char> pilha,
                        List<string> caminho, List<string> melhor, HashSet<string> visitados, ref int passos)
    {
        string chave = $"{estado}|{pos}|{PilhaTexto(pilha)}";
        if (!visitados.Add(chave)) return false;        // configuração já explorada
        if (++passos > LimitePassos) return false;

        caminho.Add(ConfiguracaoTexto(estado, cadeia, pos, pilha));
        if (caminho.Count > melhor.Count)               // guarda o ramo mais profundo
        {
            melhor.Clear();
            melhor.AddRange(caminho);
        }

        // Aceitação por pilha vazia: toda a entrada consumida E pilha vazia.
        if (pos == cadeia.Length && pilha.Count == 0)
            return true;

        foreach (var (novoEstado, novaPilha, novaPos) in Sucessores(estado, cadeia, pos, pilha))
        {
            if (Buscar(novoEstado, cadeia, novaPos, novaPilha, caminho, melhor, visitados, ref passos))
                return true;
        }

        caminho.RemoveAt(caminho.Count - 1);            // backtrack: este ramo não aceitou
        return false;
    }

    // Gera todas as configurações alcançáveis em um passo a partir da atual.
    private IEnumerable<(string novoEstado, Stack<char> pilha, int pos)> Sucessores(
        string estado, string cadeia, int pos, Stack<char> pilha)
    {
        bool temTopo = pilha.Count > 0;
        char topo = temTopo ? pilha.Peek() : Lambda;

        // Combinações de chave a testar: entrada ∈ {símbolo atual, λ}; topo ∈ {topo real, λ}.
        var chaves = new List<(char entrada, char topoChave, bool consome)>();
        if (pos < cadeia.Length)
        {
            char s = cadeia[pos];
            chaves.Add((s, Lambda, true));               // lê s, ignora topo
            if (temTopo) chaves.Add((s, topo, true));    // lê s, desempilha topo
        }
        chaves.Add((Lambda, Lambda, false));             // λ-movimento, ignora topo
        if (temTopo) chaves.Add((Lambda, topo, false));  // λ-movimento, desempilha topo

        foreach (var (entrada, topoChave, consome) in chaves)
        {
            if (!Transicao.TryGetValue((estado, entrada, topoChave), out var destinos))
                continue;

            foreach (var (novoEstado, empilha) in destinos)
            {
                var nova = ClonarPilha(pilha);
                if (topoChave != Lambda) nova.Pop();     // desempilha b (quando b != λ)
                // Empilha z de modo que z[0] fique no topo (push em ordem reversa).
                for (int k = empilha.Length - 1; k >= 0; k--) nova.Push(empilha[k]);

                yield return (novoEstado, nova, consome ? pos + 1 : pos);
            }
        }
    }

    /// <summary>Imprime as transições no formato dos slides: δ(e, a, b) = [e', z].</summary>
    public void ExibirDiagrama()
    {
        Console.WriteLine("Funcao de transicao (delta):");
        Console.WriteLine($"  Q  = {{ {string.Join(", ", Estados.OrderBy(e => e))} }}");
        Console.WriteLine($"  Sigma = {{ {string.Join(", ", Alfabeto.OrderBy(c => c))} }}");
        Console.WriteLine($"  Gamma = {{ {string.Join(", ", AlfabetoPilha.OrderBy(c => c))} }}");
        Console.WriteLine($"  q0 = {EstadoInicial}   Z0 = {Simbolo(SimboloInicialPilha)}   (aceita por pilha vazia)");
        Console.WriteLine();
        foreach (var ((e, a, b), destinos) in Transicao.OrderBy(t => t.Key.estado).ThenBy(t => t.Key.entrada))
        {
            foreach (var (novoEstado, empilha) in destinos)
            {
                string z = empilha.Length == 0 ? Simbolo(Lambda) : empilha;
                Console.WriteLine($"  delta({e}, {Simbolo(a)}, {Simbolo(b)}) = [{novoEstado}, {z}]");
            }
        }
        Console.WriteLine();
    }

    // ----- auxiliares de formatação -----

    // Configuração instantânea no estilo do professor: [estado, entrada_restante, pilha].
    private static string ConfiguracaoTexto(string estado, string cadeia, int pos, Stack<char> pilha)
    {
        string entrada = pos >= cadeia.Length ? "λ" : cadeia.Substring(pos);
        return $"[{estado}, {entrada}, {PilhaTexto(pilha)}]";
    }

    // Pilha como texto com o TOPO À ESQUERDA (igual à notação ⊢ dos slides).
    private static string PilhaTexto(Stack<char> pilha)
        => pilha.Count == 0 ? "λ" : new string(pilha.ToArray());

    private static string Simbolo(char c) => c == Lambda ? "λ" : c.ToString();

    // Clona preservando a ordem (dois "reverses" da Stack).
    private static Stack<char> ClonarPilha(Stack<char> origem)
        => new Stack<char>(new Stack<char>(origem));
}
