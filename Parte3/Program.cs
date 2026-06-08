using Parte3;

// ============================================================================
//  Parte 3 - Simulador de Maquina de Turing (MT)
//  Linguagem-alvo:  L4 = { a^n b^n c^n | n >= 1 }
//  Disciplina: Fundamentos Teoricos da Computacao - Faculdade Cotemig
// ============================================================================

const char L = MaquinaTuring.Esquerda;   // 'L'
const char R = MaquinaTuring.Direita;    // 'R'

string baseDir = AppContext.BaseDirectory;

Console.WriteLine("================================================================");
Console.WriteLine(" PARTE 3 - MAQUINA DE TURING");
Console.WriteLine(" L4 = { a^n b^n c^n | n >= 1 }");
Console.WriteLine("================================================================\n");

// ----------------------------------------------------------------------------
// MT reconhecedora de L4 = { a^n b^n c^n | n >= 1 }  (estrategia de MARCACAO).
//
//   A cada passada a maquina marca um 'a' (-> X), o primeiro 'b' (-> Y) e o
//   primeiro 'c' (-> Z), garantindo que as tres quantidades caiam juntas.
//   Esgotados os 'a', percorre a fita confirmando o padrao X* Y* Z* e aceita.
//
//   Estados:
//     q0 -> inicio da passada: sobre o 'a' mais a esquerda ainda nao marcado;
//     q1 -> procura o primeiro 'b' a direita (pula a's e Y's ja marcados);
//     q2 -> procura o primeiro 'c' a direita (pula b's e Z's ja marcados);
//     q3 -> retorna a esquerda ate o ultimo X e reinicia a passada;
//     q4 -> verificacao final: percorre os Y's;
//     q5 -> verificacao final: percorre os Z's ate o branco;
//     qaccept / qreject -> estados de parada.
// ----------------------------------------------------------------------------
var deltaL4 = new Dictionary<(string, char), (string, char, char)>
{
    // q0: marca um 'a' como X e parte para achar um 'b'.
    { ("q0", 'a'), ("q1", 'X', R) },  // delta(q0, a) = [q1, X, R]
    { ("q0", 'Y'), ("q4", 'Y', R) },  // sem 'a' restante: inicia verificacao

    // q1: anda a direita ate o primeiro 'b', pulando a's e Y's; marca 'b' como Y.
    { ("q1", 'a'), ("q1", 'a', R) },
    { ("q1", 'Y'), ("q1", 'Y', R) },
    { ("q1", 'b'), ("q2", 'Y', R) },

    // q2: anda a direita ate o primeiro 'c', pulando b's e Z's; marca 'c' como Z e volta.
    { ("q2", 'b'), ("q2", 'b', R) },
    { ("q2", 'Z'), ("q2", 'Z', R) },
    { ("q2", 'c'), ("q3", 'Z', L) },

    // q3: retorna a esquerda ate o X; entao avanca para o proximo 'a' e reinicia.
    { ("q3", 'a'), ("q3", 'a', L) },
    { ("q3", 'b'), ("q3", 'b', L) },
    { ("q3", 'Y'), ("q3", 'Y', L) },
    { ("q3", 'Z'), ("q3", 'Z', L) },
    { ("q3", 'X'), ("q0", 'X', R) },

    // q4: verificacao final - todos os Y's seguidos dos Z's.
    { ("q4", 'Y'), ("q4", 'Y', R) },
    { ("q4", 'Z'), ("q5", 'Z', R) },

    // q5: percorre os Z's; branco ao final => cadeia bem formada => aceita.
    { ("q5", 'Z'), ("q5", 'Z', R) },
    { ("q5", '_'), ("qaccept", '_', R) },
};

var mtL4 = new MaquinaTuring(
    estados: new[] { "q0", "q1", "q2", "q3", "q4", "q5", "qaccept", "qreject" },
    alfabeto: new[] { 'a', 'b', 'c' },
    alfabetoFita: new[] { 'a', 'b', 'c', 'X', 'Y', 'Z', '_' },
    transicao: deltaL4,
    estadoInicial: "q0",
    estadoAceitacao: "qaccept",
    estadoRejeicao: "qreject");

Console.WriteLine(">> MT para L4 = { a^n b^n c^n | n >= 1 }\n");
mtL4.ExibirDefinicao();
ProcessarReconhecedor(mtL4, Path.Combine(baseDir, "entradas_mt.txt"));

// ============================================================================
//  Funcoes auxiliares de apresentacao
// ============================================================================

// Le cada linha do arquivo e exibe cadeia + rastro de configuracoes + veredito.
static void ProcessarReconhecedor(MaquinaTuring mt, string caminho)
{
    if (!File.Exists(caminho))
    {
        Console.WriteLine($"[ERRO] Arquivo de entradas nao encontrado: {caminho}");
        return;
    }

    foreach (string linha in File.ReadAllLines(caminho))
    {
        string cadeia = linha.Trim();
        ResultadoExecucao r = mt.Executar(cadeia);

        string exibicao = cadeia.Length == 0 ? "(lambda)" : cadeia;
        Console.WriteLine($"Cadeia : {exibicao}");
        ImprimirRastro(r.Rastro);

        if (!r.Parou)
            Console.WriteLine($"  [!] limite de {mt.LimitePassos} passos atingido (possivel laco)");

        Console.WriteLine($"Passos : {r.Passos}");
        Console.WriteLine($"Result : {(r.Aceita ? "ACEITA" : "REJEITA")}  (parou em {r.EstadoFinal})");
        Console.WriteLine(new string('-', 60));
    }
}

// Imprime, passo a passo, as configuracoes instantaneas da computacao.
static void ImprimirRastro(List<string> rastro)
{
    for (int i = 0; i < rastro.Count; i++)
        Console.WriteLine($"  passo {i,3}: {rastro[i]}");
}
