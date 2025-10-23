# Database Scripts - Approval Hub System

Este diretório contém os scripts SQL para criar e configurar as tabelas do sistema de Approval Hub.

## Ordem de Execução

Execute os scripts na seguinte ordem:

1. **001_CreateSchema.sql** - Cria o schema `aprov`
2. **002_CreateApprovalHubTable.sql** - Cria a tabela principal `approval_hub` com System Versioning
3. **003_CreateDimensoesTable.sql** - Cria a tabela `ids_dimensoes` com System Versioning
4. **004_CreateAnexoTable.sql** - Cria a tabela `anexo` para ficheiros
5. **005_InsertSampleData.sql** - (Opcional) Insere dados de exemplo para testes

## Estrutura das Tabelas

### aprov.approval_hub
Tabela principal que armazena os pedidos de aprovação de faturas.

**Campos principais:**
- `id_approval_hub` - Chave primária
- `tipo_fatura` - Tipo de fatura (Fatura, Nota de Crédito, etc.)
- `fornecedor_nome` - Nome do fornecedor
- `valor_total` - Valor total da fatura
- `estado` - Estado do pedido (Pendente, Aprovado, Rejeitado, Em Análise)
- System Versioning habilitado para histórico de alterações

### aprov.ids_dimensoes
Armazena as dimensões associadas a cada pedido de aprovação.

**Tipos de dimensões:**
- Centro de Custo
- Cliente
- Produto
- Marca
- Mercado
- Projeto

**Campos principais:**
- `id` - Chave primária
- `id_approval_hub` - FK para approval_hub
- `tipo` - Tipo de dimensão
- `percentagem` - Percentagem do valor total (0-100)
- `valor` - Valor calculado
- System Versioning habilitado

### aprov.anexo
Armazena ficheiros anexados aos pedidos.

**Campos principais:**
- `id_anexo` - Chave primária
- `id_approval_hub` - FK para approval_hub
- `nome_ficheiro` - Nome do ficheiro
- `conteudo` - Conteúdo binário do ficheiro
- `tamanho_bytes` - Tamanho do ficheiro

## System Versioning

As tabelas `approval_hub` e `ids_dimensoes` têm System Versioning habilitado, permitindo:
- Consultar o histórico de alterações
- Auditoria completa de todas as modificações
- Recuperação de versões anteriores

### Consultar Histórico

```sql
-- Ver todas as versões de um pedido
SELECT *
FROM aprov.approval_hub
FOR SYSTEM_TIME ALL
WHERE id_approval_hub = 1;

-- Ver estado num momento específico
SELECT *
FROM aprov.approval_hub
FOR SYSTEM_TIME AS OF '2024-01-01'
WHERE id_approval_hub = 1;

-- Ver alterações entre datas
SELECT *
FROM aprov.approval_hub
FOR SYSTEM_TIME BETWEEN '2024-01-01' AND '2024-12-31'
WHERE id_approval_hub = 1;
```

## Notas Importantes

1. A tabela `anexo` não tem System Versioning devido ao armazenamento de dados binários grandes
2. Todas as tabelas têm índices otimizados para as queries mais comuns
3. As chaves estrangeiras têm `ON DELETE CASCADE` para manter integridade referencial
4. Os campos de data usam `DATETIME2` para maior precisão
5. O campo `row_version` em `approval_hub` é útil para controlo de concorrência otimista

## Configuração da Connection String

No ficheiro `appsettings.json`, configure:

```json
{
  "ConnectionStrings": {
    "InfgestMVC_TESTES": "Server=your_server;Database=InfgestMVC_TESTES;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Backup e Manutenção

Recomenda-se:
- Backups regulares da base de dados
- Monitorização do crescimento das tabelas de histórico
- Limpeza periódica do histórico antigo se necessário

```sql
-- Limpar histórico anterior a uma data (cuidado!)
DELETE FROM aprov.approval_hub_history
WHERE valido_ate < '2023-01-01';
```
