# Exemplo de Uso das APIs BankMore

Este documento apresenta exemplos práticos de como usar as APIs do sistema BankMore.

## Pré-requisitos

1. Execute o sistema usando Docker Compose:
```bash
docker-compose up --build
```

2. As APIs estarão disponíveis em:
   - ContaCorrente: https://localhost:7001
   - Transferencia: https://localhost:7002
   - Tarifa: https://localhost:7003

## Fluxo Completo de Uso

### 1. Cadastrar Primeira Conta

```bash
curl -X POST "https://localhost:7001/api/conta-corrente/cadastrar" \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "12345678901",
    "senha": "123456",
    "nomeTitular": "João Silva"
  }'
```

**Resposta esperada:**
```json
"123456"
```

### 2. Cadastrar Segunda Conta

```bash
curl -X POST "https://localhost:7001/api/conta-corrente/cadastrar" \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "98765432100",
    "senha": "654321",
    "nomeTitular": "Maria Santos"
  }'
```

**Resposta esperada:**
```json
"789012"
```

### 3. Realizar Login na Primeira Conta

```bash
curl -X POST "https://localhost:7001/api/conta-corrente/login" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacao": "12345678901",
    "senha": "123456"
  }'
```

**Resposta esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "contaCorrenteId": 1,
  "numeroConta": "123456"
}
```

### 4. Realizar Depósito na Primeira Conta

```bash
curl -X POST "https://localhost:7001/api/conta-corrente/movimentar" \
  -H "Authorization: Bearer <token-da-resposta-anterior>" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacaoRequisicao": "DEP-001",
    "valor": 1000.00,
    "tipoMovimento": "C"
  }'
```

**Resposta esperada:**
```
HTTP 204 No Content
```

### 5. Consultar Saldo da Primeira Conta

```bash
curl -X GET "https://localhost:7001/api/conta-corrente/saldo" \
  -H "Authorization: Bearer <token-da-resposta-anterior>"
```

**Resposta esperada:**
```json
{
  "numeroConta": "123456",
  "nomeTitular": "João Silva",
  "dataHoraConsulta": "2024-01-15T10:30:00Z",
  "saldo": 1000.00
}
```

### 6. Realizar Login na Segunda Conta

```bash
curl -X POST "https://localhost:7001/api/conta-corrente/login" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacao": "98765432100",
    "senha": "654321"
  }'
```

### 7. Realizar Transferência da Primeira para Segunda Conta

```bash
curl -X POST "https://localhost:7002/api/transferencia/efetuar" \
  -H "Authorization: Bearer <token-da-primeira-conta>" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacaoRequisicao": "TXN-001",
    "contaDestinoId": 2,
    "valor": 500.00
  }'
```

**Resposta esperada:**
```
HTTP 204 No Content
```

### 8. Consultar Saldo da Primeira Conta Após Transferência

```bash
curl -X GET "https://localhost:7001/api/conta-corrente/saldo" \
  -H "Authorization: Bearer <token-da-primeira-conta>"
```

**Resposta esperada:**
```json
{
  "numeroConta": "123456",
  "nomeTitular": "João Silva",
  "dataHoraConsulta": "2024-01-15T10:35:00Z",
  "saldo": 500.00
}
```

### 9. Consultar Saldo da Segunda Conta

```bash
curl -X GET "https://localhost:7001/api/conta-corrente/saldo" \
  -H "Authorization: Bearer <token-da-segunda-conta>"
```

**Resposta esperada:**
```json
{
  "numeroConta": "789012",
  "nomeTitular": "Maria Santos",
  "dataHoraConsulta": "2024-01-15T10:35:00Z",
  "saldo": 500.00
}
```

## Testando Idempotência

### Repetir a Mesma Transferência

```bash
curl -X POST "https://localhost:7002/api/transferencia/efetuar" \
  -H "Authorization: Bearer <token-da-primeira-conta>" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacaoRequisicao": "TXN-001",
    "contaDestinoId": 2,
    "valor": 500.00
  }'
```

**Resposta esperada:**
```
HTTP 204 No Content
```

A transferência não será processada novamente devido à idempotência.

## Testando Validações

### Tentar Transferir Valor Negativo

```bash
curl -X POST "https://localhost:7002/api/transferencia/efetuar" \
  -H "Authorization: Bearer <token-da-primeira-conta>" \
  -H "Content-Type: application/json" \
  -d '{
    "identificacaoRequisicao": "TXN-002",
    "contaDestinoId": 2,
    "valor": -100.00
  }'
```

**Resposta esperada:**
```json
{
  "mensagem": "Valor deve ser maior que zero",
  "tipoFalha": "INVALID_VALUE"
}
```

### Tentar Acessar Endpoint Sem Autenticação

```bash
curl -X GET "https://localhost:7001/api/conta-corrente/saldo"
```

**Resposta esperada:**
```
HTTP 401 Unauthorized
```

## Processamento de Tarifas

As tarifas são processadas automaticamente quando uma transferência é realizada. O sistema:

1. Envia um evento para o tópico Kafka "transferencias-realizadas"
2. A API de Tarifa consome este evento
3. Processa a tarifa (R$ 2,00 por transferência)
4. Envia um evento para o tópico "tarifas-realizadas"
5. A API de ContaCorrente consome este evento e debita a tarifa

## Documentação Swagger

Para uma interface mais amigável, acesse:

- **ContaCorrente API**: https://localhost:7001/swagger
- **Transferencia API**: https://localhost:7002/swagger
- **Tarifa API**: https://localhost:7003/swagger

## Logs do Sistema

Para acompanhar o processamento das tarifas e eventos Kafka, monitore os logs dos containers:

```bash
docker-compose logs -f tarifa-api
docker-compose logs -f conta-corrente-api
```

## Troubleshooting

### Erro de Conexão com Kafka

Se houver problemas com o Kafka, verifique se o container está rodando:

```bash
docker-compose ps kafka
```

### Erro de Banco de Dados

Os bancos SQLite são criados automaticamente na pasta `./data/`. Se houver problemas, delete os arquivos e reinicie:

```bash
rm -rf data/*
docker-compose up --build
```

### Problemas de Autenticação

Certifique-se de usar o token JWT correto e que ele não tenha expirado (válido por 1 hora).
