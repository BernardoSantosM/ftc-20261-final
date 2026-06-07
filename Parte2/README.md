# Parte 2 — Autômato de Pilha (AP) com reconhecimento por pilha vazia

> **Responsável:** _<integrante 2>_ — esta pasta é o ponto de partida para o seu projeto C#.

## Linguagem-alvo

`L2 = { a^n b^n | n >= 1 }`

## Exigências do enunciado (Seção 3.2)

- [ ] Representar o AP como a **7-upla** formal `M = (Q, Σ, Γ, δ, q0, Z0, ∅)`.
- [ ] Pilha implementada **manualmente** como `Stack<char>` (proibido `LinkedList` ou arrays).
- [ ] Aceitação **exclusivamente por pilha vazia** (não verificar estado final).
- [ ] Transições com **λ-movimentos** representados como `'\0'` no código.
- [ ] Exibir, a cada passo, a **configuração instantânea**: estado atual, entrada restante e conteúdo da pilha.
- [ ] Ler as cadeias de `entradas_ap.txt` e exibir os resultados.
- [ ] **Desafio obrigatório:** segundo AP que reconhece os palíndromos
      `L3 = { w em {a,b}* | w = w^R, |w| >= 1 }`, justificando a diferença de complexidade entre L2 e L3.

## Casos de teste obrigatórios (L2)

| Cadeia | Esperado |
|---|---|
| `ab` | ACEITA |
| `aabb` | ACEITA |
| `aaabbb` | ACEITA |
| `aab` | REJEITA |
| `abb` | REJEITA |
| `ba` | REJEITA |
| `λ` (vazia) | REJEITA |
| `abab` | REJEITA |

## Referência das aulas

Seguir a notação dos slides **FTC05** (Autômato de Pilha) do Prof. Júlio César.

## Como rodar (depois de implementado)

```bash
cd Parte2
dotnet run
```
