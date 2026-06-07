using Parte1;

// ============================================================================
//  Parte 1 - Simulador Generico de AFD
//  Linguagem-alvo:  L1 = { w em {a, b}* | w termina com "ab" }
//  Disciplina: Fundamentos Teoricos da Computacao - Faculdade Cotemig
// ============================================================================

// Localiza os arquivos de dados na pasta de saida (copiados pelo .csproj).
string baseDir = AppContext.BaseDirectory;
string caminhoEntradas = Path.Combine(baseDir, "entradas.txt");
string caminhoJson = Path.Combine(baseDir, "afd.json");

Console.WriteLine("========================================================");
Console.WriteLine(" PARTE 1 - AUTOMATO FINITO DETERMINISTICO (AFD)");
Console.WriteLine(" L1 = { w em {a,b}* | w termina com 'ab' }");
Console.WriteLine("========================================================\n");

// ----------------------------------------------------------------------------
// 1) AFD de L1 construido em codigo (traducao direta da 5-upla formal).
//
//    Q  = { q0, q1, q2 }      Sigma = { a, b }      q0 inicial      F = { q2 }
//
//    Significado dos estados:
//      q0 -> ainda nao ha um 'a' iniciando o sufixo "ab" (recomeco apos 'b');
//      q1 -> o simbolo anterior foi 'a' (candidato a formar "ab");
//      q2 -> acabei de ler "ab" (unico estado de aceitacao).
// ----------------------------------------------------------------------------
var delta = new Dictionary<(string, char), string>
{
    { ("q0", 'a'), "q1" },  // viu 'a': passa a esperar um 'b'
    { ("q0", 'b'), "q0" },  // 'b' sozinho nao inicia "ab": permanece
    { ("q1", 'a'), "q1" },  // outro 'a': continua sendo candidato
    { ("q1", 'b'), "q2" },  // 'a' seguido de 'b' => leu "ab": aceita
    { ("q2", 'a'), "q1" },  // depois de "ab" veio 'a': novo candidato
    { ("q2", 'b'), "q0" },  // depois de "ab" veio 'b': quebra o sufixo
};

var afd = new AFD(
    estados: new[] { "q0", "q1", "q2" },
    alfabeto: new[] { 'a', 'b' },
    transicao: delta,
    estadoInicial: "q0",
    estadosAceitacao: new[] { "q2" });

Console.WriteLine(">> AFD de L1 (definido em codigo)\n");
afd.ExibirDiagrama();

// ----------------------------------------------------------------------------
// 2) Le as cadeias de entradas.txt (uma por linha) e exibe, para cada uma,
//    a cadeia, o rastro de estados percorridos e o resultado (ACEITA/REJEITA).
// ----------------------------------------------------------------------------
Console.WriteLine(">> Processando cadeias de 'entradas.txt'\n");
ProcessarArquivo(afd, caminhoEntradas);

// ----------------------------------------------------------------------------
// 3) Desafio obrigatorio: recarrega o MESMO AFD a partir de 'afd.json' e
//    reprocessa as cadeias, mostrando que o comportamento e identico.
// ----------------------------------------------------------------------------
Console.WriteLine("\n========================================================");
Console.WriteLine(" DESAFIO - Carga dinamica de AFD a partir de 'afd.json'");
Console.WriteLine("========================================================\n");

try
{
    AFD afdJson = CarregadorJson.Carregar(caminhoJson);
    Console.WriteLine(">> AFD carregado de 'afd.json'\n");
    afdJson.ExibirDiagrama();

    Console.WriteLine(">> Processando as mesmas cadeias com o AFD do JSON\n");
    ProcessarArquivo(afdJson, caminhoEntradas);
}
catch (Exception ex)
{
    Console.WriteLine($"[ERRO] Nao foi possivel carregar o afd.json: {ex.Message}");
}

// ============================================================================
//  Funcoes auxiliares de apresentacao
// ============================================================================

// Le cada linha do arquivo e imprime cadeia + rastro + veredito.
static void ProcessarArquivo(AFD afd, string caminho)
{
    if (!File.Exists(caminho))
    {
        Console.WriteLine($"[ERRO] Arquivo de entradas nao encontrado: {caminho}");
        return;
    }

    // Mantem linhas vazias: uma linha em branco representa a cadeia vazia (lambda).
    string[] linhas = File.ReadAllLines(caminho);

    foreach (string linha in linhas)
    {
        string cadeia = linha.Trim();
        ResultadoExecucao r = afd.Processar(cadeia);

        string exibicao = cadeia.Length == 0 ? "(lambda)" : cadeia;
        string veredito = r.Aceita ? "ACEITA" : "REJEITA";

        Console.WriteLine($"Cadeia : {exibicao}");
        Console.WriteLine($"Rastro : {string.Join(" -> ", r.Rastro)}");
        Console.WriteLine($"Result : {veredito}"
            + (r.Observacao is null ? "" : $"  ({r.Observacao})"));
        Console.WriteLine(new string('-', 48));
    }
}
