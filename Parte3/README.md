# Parte 3 — Máquina de Turing (MT)

> **Responsável:** _<integrante 3>_ — esta pasta é o ponto de partida para o seu projeto C#.

## Linguagem-alvo

`L4 = { a^n b^n c^n | n >= 1 }`

## Exigências do enunciado (Seção 3.3)

- [ ] Fita como estrutura **dinâmica** (`Dictionary<int,char>`, chave = posição); branco = `'_'`.
- [ ] Representar a MT como a **7-upla** `M = (Q, Σ, Γ, δ, q0, q_accept, q_reject)`.
- [ ] δ como `Dictionary<(string estado, char simbolo), (string novoEstado, char novoSimbolo, char direcao)>`.
- [ ] Exibir, a cada passo: estado atual, fita completa com `[ ]` ao redor do símbolo sob o cabeçote, e a posição.
- [ ] Estados explícitos para `q_accept` e `q_reject`.
- [ ] **Contador de passos** + **limite configurável** para evitar loops infinitos.
- [ ] **Desafio obrigatório:** segunda MT que **computa** `f(n) = n + 1` em unário
      (entrada `n` ocorrências de `1` → saída `n+1` ocorrências de `1`; ex.: `111` → `1111`).

## Casos de teste obrigatórios (L4)

| Cadeia | Esperado |
|---|---|
| `abc` | ACEITA |
| `aabbcc` | ACEITA |
| `aaabbbccc` | ACEITA |
| `aabbc` | REJEITA |
| `ab` | REJEITA |
| `abc abc` | REJEITA |
| `λ` (vazia) | REJEITA |

## Casos do desafio f(n) = n + 1 (unário)

| Fita de entrada | Fita de saída |
|---|---|
| `1` | `11` |
| `111` | `1111` |
| `11111` | `111111` |

## Referência das aulas

Seguir a notação dos slides **FTC06** (Máquina de Turing) e **FTC07** (MT Universal) do Prof. Júlio César.

## Como rodar (depois de implementado)

```bash
cd Parte3
dotnet run
```
