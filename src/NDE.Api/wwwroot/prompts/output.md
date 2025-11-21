## Critérios para reportar problemas

**SEMPRE reporte quando encontrar:**
- Bugs reais ou potenciais (null reference, race conditions, overflow, etc)
- Vulnerabilidades de segurança (SQL injection, XSS, secrets hardcoded, etc)
- Problemas de performance críticos (N+1 queries, memory leaks, loops O(n²))
- Violações de padrões arquiteturais do projeto
- Código não testável ou com lógica complexa sem testes
- Erros de lógica de negócio
- Violações de princípios SOLID evidentes
- Problemas de concorrência (deadlocks, data races)

**NÃO reporte:**
- Preferências estéticas de formatação
- Sugestões de refatoração sem impacto claro
- Elogios ou comentários gerais
- Problemas triviais já cobertos por linters

## Formato da resposta

**Se houver 1 ou mais problemas:**
Liste TODOS os problemas encontrados usando o formato acima, um por vez.

**Se NÃO houver problemas relevantes:**
Responda APENAS e EXATAMENTE: