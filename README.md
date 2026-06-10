# ftc-20261-final

Trabalho Final da disciplina **Fundamentos Teóricos da Computação** — Faculdade Cotemig (2026.1).
Implementação, em **C# (.NET 6+)**, de três máquinas abstratas fundamentais da Teoria da Computação:

1. **Autômato Finito Determinístico (AFD)** — reconhecedor de linguagens regulares;
2. **Autômato de Pilha (AP)** com reconhecimento por pilha vazia — linguagens livres de contexto;
3. **Máquina de Turing (MT)** — modelo universal de computação.

> Professor: Júlio César da Silva.

## Integrantes da equipe

| Nome completo | Matrícula | Parte responsável |
|---|---|---|
| Bernardo Santos Magalhães | 72400188 | Parte 1 — AFD |
| João Victor Amaro de Araújo | 72400110 | Parte 2 — Autômato de Pilha |
| Lucas Filipe França Nunes | 72401362 | Parte 3 — Máquina de Turing |

## Estrutura do repositório

```
ftc-20261-final/
├── Parte1/   # AFD  — L1 = { w em {a,b}* | w termina com "ab" }
├── Parte2/   # AP   — L2 = { a^n b^n | n >= 1 }  (+ desafio: palíndromos L3)
├── Parte3/   # MT   — L4 = { a^n b^n c^n | n >= 1 }  (+ desafio: f(n) = n+1)
├── docs/     # relatório técnico (relatorio.pdf) e materiais de apoio
└── README.md
```

## Descrição de cada parte

- **Parte 1 — AFD:** simulador genérico de AFD representado como a 5-upla formal
  `M = (Q, Σ, δ, q0, F)`. Lê cadeias de `entradas.txt`, exibe o rastro de estados e o
  veredito (ACEITA/REJEITA), imprime a tabela de transições e carrega qualquer AFD a
  partir de `afd.json` (desafio obrigatório).
- **Parte 2 — Autômato de Pilha:** _(a implementar pelo integrante responsável)._
- **Parte 3 — Máquina de Turing:** simulador de MT representado como a 7-upla formal
  `M = (Q, Σ, Γ, δ, q0, q_accept, q_reject)`, com fita dinâmica (`Dictionary<int,char>`) e
  branco `'_'`. Reconhece `L4 = { aⁿbⁿcⁿ | n ≥ 1 }` por estratégia de marcação, exibindo a
  cada passo o estado, a fita (com `[ ]` ao redor do símbolo sob o cabeçote) e a posição do
  cabeçote, além de contador e limite configurável de passos. Inclui a MT transdutora que
  computa `f(n) = n + 1` em unário (desafio obrigatório).

## Como compilar e executar

Pré-requisito: **.NET SDK 6.0 ou superior** ([download](https://dotnet.microsoft.com/download)).
Verifique com `dotnet --version`.

```bash
# Parte 1 — AFD
cd Parte1
dotnet run
```

Cada parte é um projeto independente; repita `dotnet run` dentro de `Parte2/` e `Parte3/`.

## Vídeo de defesa

🎥 _Link do YouTube (não listado): https://youtu.be/frcq_r53SjU

---

_Entrega: 10/06/2026 — via Google Sala de Aula (apenas o link deste repositório)._
