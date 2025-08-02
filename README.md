# alanlviana - Rinha 2025

## Resumo

Colocando em prática uma forma de processar pagamentos com recursos limitados e recebendo uma grande carga de requisições para pouco recurso disponível, criando um backend resiliente.

Nesse projeto todas as boas práticas foram descartadas!

# Abordagem - v1
Mantém todos os dados em memória em estruturas de dados como ConcurrentQueue e os resultados foram sumarizados em um SortedDictionary.Para sincronizar o total de requisições e status de healthcheck os servidores realizam requisições entre si.

# Abordagem - v2
TBD

## Especificação técnica

- .NET Core
- Nginx
- Docker
- Docker Compose
