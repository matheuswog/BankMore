# BankMore - Sistema de Banco Digital

Sistema de banco digital baseado em microsserviços desenvolvido para o desafio técnico da vaga de DEV .NET.

## Arquitetura

O sistema foi desenvolvido seguindo os padrões:
- **Domain-Driven Design (DDD)** - para manter a lógica de negócio organizada
- **CQRS (Command Query Responsibility Segregation)** - separando comandos de consultas
- **Arquitetura de Microsserviços** - cada funcionalidade em seu próprio serviço

## Funcionalidades Implementadas

### API ContaCorrente (Porta 7001)
- ✅ Cadastro de conta corrente com validação de CPF
- ✅ Login com geração de token JWT
- ✅ Inativação de conta
- ✅ Movimentações (crédito/débito) com idempotência
- ✅ Consulta de saldo
- ✅ Validação de conta (endpoint interno)

### API Transferencia (Porta 7002)
- ✅ Transferência entre contas da mesma instituição
- ✅ Validação de contas origem e destino
- ✅ Estorno automático em caso de falha
- ✅ Idempotência

### API Tarifa (Porta 7003)
- ✅ Processamento de tarifas de transferência
- ✅ Configuração de valor da tarifa via appsettings
- ✅ Idempotência

## Tecnologias Utilizadas

- **.NET 9.0** - framework principal
- **Dapper** - ORM leve para acesso a dados
- **SQLite** - banco de dados para desenvolvimento (fácil de configurar)
- **MediatR** - implementação do padrão CQRS
- **JWT** - autenticação baseada em tokens
- **Swagger** - documentação automática da API
- **Docker** - containerização para facilitar deploy
- **Kafka** - comunicação assíncrona entre serviços (configurado)

## Como Executar

### Opção 1: Docker Compose (Recomendado)

```bash
# Clonar o repositório
git clone <url-do-repositorio>
cd BankMore

# Executar com Docker Compose (mais fácil)
docker-compose up --build
```

### Opção 2: Execução Local

```bash
# Restaurar dependências
dotnet restore

# Executar as APIs (em terminais separados)
dotnet run --project src/BankMore.ContaCorrente.API
dotnet run --project src/BankMore.Transferencia.API
dotnet run --project src/BankMore.Tarifa.API
```

## Endpoints Principais

### ContaCorrente API (https://localhost:7001)

- `POST /api/conta-corrente/cadastrar` - Cadastrar nova conta
- `POST /api/conta-corrente/login` - Realizar login
- `POST /api/conta-corrente/inativar` - Inativar conta (requer autenticação)
- `POST /api/conta-corrente/movimentar` - Realizar movimentação (requer autenticação)
- `GET /api/conta-corrente/saldo` - Consultar saldo (requer autenticação)
- `GET /api/conta-corrente/validar/{contaId}` - Validar conta (requer autenticação)

### Transferencia API (https://localhost:7002)

- `POST /api/transferencia/efetuar` - Efetuar transferência (requer autenticação)

### Tarifa API (https://localhost:7003)

- `POST /api/tarifa/processar` - Processar tarifa (requer autenticação)

## Documentação Swagger

Após executar as APIs, acesse:
- ContaCorrente: https://localhost:7001/swagger
- Transferencia: https://localhost:7002/swagger
- Tarifa: https://localhost:7003/swagger

## Exemplo de Uso

### 1. Cadastrar Conta
```bash
curl -X POST "https://localhost:7001/api/conta-corrente/cadastrar" \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "12345678901",
    "senha": "123456",
    "nomeTitular": "João Silva"
  }'
```

### 2. Realizar Login
```bash
curl -X POST "https://localhost:7001/api/conta-corrente/login" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacao": "12345678901",
    "senha": "123456"
  }'
```

### 3. Consultar Saldo
```bash
curl -X GET "https://localhost:7001/api/conta-corrente/saldo" \
  -H "Authorization: Bearer <token-jwt>"
```

### 4. Realizar Transferência
```bash
curl -X POST "https://localhost:7002/api/transferencia/efetuar" \
  -H "Authorization: Bearer <token-jwt>" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacaoRequisicao": "TXN-001",
    "contaDestinoId": 2,
    "valor": 100.00
  }'
```

## Segurança

- Todas as APIs são protegidas com autenticação JWT
- Senhas são criptografadas com SHA256 (em produção, seria melhor usar bcrypt)
- Validação de CPF implementada com algoritmo completo
- Dados sensíveis não transitam entre microsserviços

## Idempotência

- Todas as operações são idempotentes através da `IdentificacaoRequisicao`
- Permite retry seguro em caso de falhas de rede
- Evita processamento duplicado de transferências

## Testes

```bash
# Executar testes unitários
dotnet test tests/BankMore.ContaCorrente.Tests
dotnet test tests/BankMore.Transferencia.Tests

# Executar todos os testes
dotnet test
```

## Estrutura do Projeto

```
BankMore/
├── src/
│   ├── BankMore.ContaCorrente.API/
│   ├── BankMore.ContaCorrente.Domain/
│   ├── BankMore.ContaCorrente.Infrastructure/
│   ├── BankMore.Transferencia.API/
│   ├── BankMore.Transferencia.Domain/
│   ├── BankMore.Transferencia.Infrastructure/
│   ├── BankMore.Tarifa.API/
│   ├── BankMore.Tarifa.Domain/
│   └── BankMore.Tarifa.Infrastructure/
├── tests/
│   ├── BankMore.ContaCorrente.Tests/
│   └── BankMore.Transferencia.Tests/
├── docker-compose.yaml
└── README.md
```

## Requisitos Atendidos

✅ **Funcionalidades Principais**
- Cadastro e autenticação de usuários
- Movimentações na conta corrente (depósitos e saques)
- Transferências entre contas
- Consulta de saldo

✅ **Padrões Arquiteturais**
- DDD (Domain-Driven Design) - lógica de negócio bem organizada
- CQRS (Command Query Responsibility Segregation) - separação clara de responsabilidades

✅ **Segurança**
- Autenticação JWT em todas as APIs
- Criptografia de senhas
- Validação de CPF

✅ **Qualidade**
- Testes automatizados
- Documentação Swagger completa

✅ **Infraestrutura**
- Docker Compose para execução
- SQLite como banco de dados
- Kafka configurado para comunicação assíncrona

✅ **Resiliência**
- Idempotência em todas as operações
- Estorno automático em transferências

✅ **Diferenciais**
- Cache implementado (via configuração)
- Testes de integração
- Comunicação assíncrona com Kafka
