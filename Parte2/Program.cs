using Parte2;

// ============================================================================
//  Parte 2 - Simulador de Autômato de Pilha (AP) com aceitação por PILHA VAZIA
//  L2 = { a^n b^n | n >= 1 }            (principal)
//  L3 = { w em {a,b}* | w = w^R, |w|>=1 } (desafio: palíndromos)
//  Disciplina: Fundamentos Teoricos da Computacao - Faculdade Cotemig
// ============================================================================

const char L = AutomatoPilha.Lambda;   // λ = '\0'

string baseDir = AppContext.BaseDirectory;

Console.WriteLine("================================================================");
Console.WriteLine(" PARTE 2 - AUTOMATO DE PILHA (aceitacao por PILHA VAZIA)");
Console.WriteLine("================================================================\n");

// ----------------------------------------------------------------------------
// AP 1 -> L2 = { a^n b^n | n >= 1 }   (determinístico)
//
//   Empilha um X para cada 'a'; desempilha um X para cada 'b'; ao final,
//   um λ-movimento desempilha Z0, esvaziando a pilha (aceitação).
// ----------------------------------------------------------------------------
var deltaL2 = new Dictionary<(string, char, char), List<(string, string)>>
{
    { ("q0", 'a', L  ), new() { ("q0", "X") } },  // delta(q0, a, λ) = [q0, X]
    { ("q0", 'b', 'X'), new() { ("q1", "" ) } },  // delta(q0, b, X) = [q1, λ]
    { ("q1", 'b', 'X'), new() { ("q1", "" ) } },  // delta(q1, b, X) = [q1, λ]
    { ("q1", L  , 'Z'), new() { ("q1", "" ) } },  // delta(q1, λ, Z) = [q1, λ]  (esvazia)
};

var apL2 = new AutomatoPilha(
    estados: new[] { "q0", "q1" },
    alfabeto: new[] { 'a', 'b' },
    alfabetoPilha: new[] { 'Z', 'X' },
    transicao: deltaL2,
    estadoInicial: "q0",
    simboloInicialPilha: 'Z');

Console.WriteLine(">> AP para L2 = { a^n b^n | n >= 1 }\n");
apL2.ExibirDiagrama();
ProcessarArquivo(apL2, Path.Combine(baseDir, "entradas_ap.txt"));

// ----------------------------------------------------------------------------
// AP 2 -> L3 = palíndromos sobre {a,b}   (NÃO determinístico - desafio)
//
//   Fase 1 (q0): empilha o símbolo lido (A para 'a', B para 'b').
//   Adivinha o meio: por λ-movimento (palavra par) ou consumindo o símbolo
//   central sem empilhar (palavra ímpar) -> vai para q1.
//   Fase 2 (q1): casa cada símbolo lido com o topo e desempilha.
//   Ao final, λ-movimento desempilha Z0 -> pilha vazia -> aceita.
// ----------------------------------------------------------------------------
var deltaL3 = new Dictionary<(string, char, char), List<(string, string)>>
{
    { ("q0", 'a', L  ), new() { ("q0", "A"), ("q1", "") } }, // empilha A  ou  consome meio (ímpar)
    { ("q0", 'b', L  ), new() { ("q0", "B"), ("q1", "") } }, // empilha B  ou  consome meio (ímpar)
    { ("q0", L  , L  ), new() { ("q1", "") } },              // adivinha meio par (sem ler)
    { ("q1", 'a', 'A'), new() { ("q1", "") } },              // casa 'a' com A no topo
    { ("q1", 'b', 'B'), new() { ("q1", "") } },              // casa 'b' com B no topo
    { ("q1", L  , 'Z'), new() { ("q1", "") } },              // esvazia Z0 -> aceita
};

var apL3 = new AutomatoPilha(
    estados: new[] { "q0", "q1" },
    alfabeto: new[] { 'a', 'b' },
    alfabetoPilha: new[] { 'Z', 'A', 'B' },
    transicao: deltaL3,
    estadoInicial: "q0",
    simboloInicialPilha: 'Z');

Console.WriteLine("\n================================================================");
Console.WriteLine(" DESAFIO - AP para L3 = palindromos sobre {a,b} (nao deterministico)");
Console.WriteLine("================================================================\n");
apL3.ExibirDiagrama();
ProcessarArquivo(apL3, Path.Combine(baseDir, "entradas_palindromos.txt"));

// ============================================================================
//  Auxiliar de apresentação
// ============================================================================
static void ProcessarArquivo(AutomatoPilha ap, string caminho)
{
    if (!File.Exists(caminho))
    {
        Console.WriteLine($"[ERRO] Arquivo de entradas nao encontrado: {caminho}");
        return;
    }

    foreach (string linha in File.ReadAllLines(caminho))
    {
        string cadeia = linha.Trim();
        bool aceita = ap.Aceitar(cadeia, out List<string> computacao);

        string exibicao = cadeia.Length == 0 ? "(lambda)" : cadeia;
        Console.WriteLine($"Cadeia : {exibicao}");

        // Exibe a configuracao instantanea a cada passo (estilo |- dos slides).
        for (int i = 0; i < computacao.Count; i++)
            Console.WriteLine(i == 0 ? $"  {computacao[i]}" : $"  |- {computacao[i]}");

        if (!aceita)
            Console.WriteLine("  (nenhuma computacao esvazia a pilha)");

        Console.WriteLine($"Result : {(aceita ? "ACEITA" : "REJEITA")}");
        Console.WriteLine(new string('-', 56));
    }
}
