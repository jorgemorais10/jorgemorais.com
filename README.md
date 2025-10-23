# Approval Hub System

Sistema de gestão de pedidos de aprovação de faturas desenvolvido em .NET 9 com Bootstrap 5.3.

## Características

- **CRUD Completo** - Criação, leitura, atualização e eliminação de pedidos de aprovação
- **Dimensões Dinâmicas** - Suporte para múltiplas dimensões (Centro de Custo, Cliente, Produto, Marca, Mercado, Projeto)
- **Gestão de Anexos** - Upload e download de ficheiros associados aos pedidos
- **System Versioning** - Histórico completo de alterações com SQL Server Temporal Tables
- **Interface Moderna** - UI responsiva com Bootstrap 5.3
- **Pesquisa Avançada** - Filtros por fornecedor, estado e período

## Tecnologias

- **.NET 9** - Framework principal
- **ASP.NET Core MVC** - Arquitetura web
- **SQL Server** - Base de dados com System Versioning
- **Bootstrap 5.3** - Interface do utilizador
- **Bootstrap Icons** - Iconografia
- **ADO.NET** - Acesso direto à base de dados com queries SQL

## Estrutura do Projeto

```
/
├── Controllers/          # Controladores MVC
│   └── ApprovalHubController.cs
├── Models/              # Modelos de dados
│   ├── ApprovalHub.cs
│   ├── IdsDimensao.cs
│   ├── Anexo.cs
│   └── ConfigAplicacao.cs
├── Services/            # Serviços de negócio
│   ├── IApprovalHubService.cs
│   ├── ApprovalHubService.cs
│   ├── IConfigAplicacaoProvider.cs
│   └── ConfigAplicacaoProvider.cs
├── Helpers/             # Classes auxiliares
│   └── ConfigurationHelper.cs
├── Views/               # Views Razor
│   ├── ApprovalHub/
│   │   ├── Index.cshtml
│   │   ├── Details.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _ValidationScriptsPartial.cshtml
├── wwwroot/            # Ficheiros estáticos
│   ├── css/
│   │   └── site.css
│   └── js/
│       ├── site.js
│       └── approval-form.js
├── Database/           # Scripts SQL
│   ├── 001_CreateSchema.sql
│   ├── 002_CreateApprovalHubTable.sql
│   ├── 003_CreateDimensoesTable.sql
│   ├── 004_CreateAnexoTable.sql
│   ├── 005_InsertSampleData.sql
│   └── README.md
├── appsettings.json    # Configuração
└── Program.cs          # Ponto de entrada
```

## Configuração

### 1. Base de Dados

Execute os scripts SQL na pasta `/Database` pela ordem indicada:

```bash
# Na base de dados InfgestMVC_TESTES
001_CreateSchema.sql
002_CreateApprovalHubTable.sql
003_CreateDimensoesTable.sql
004_CreateAnexoTable.sql
005_InsertSampleData.sql  # Opcional - dados de exemplo
```

### 2. Connection String

Edite o ficheiro `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "InfgestMVC_TESTES": "Server=your_server;Database=InfgestMVC_TESTES;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Executar a Aplicação

```bash
dotnet restore
dotnet build
dotnet run
```

A aplicação estará disponível em `https://localhost:5001` ou `http://localhost:5000`.

## Funcionalidades Principais

### Gestão de Pedidos

- **Lista de Pedidos** - Visualização de todos os pedidos com estados coloridos
- **Criar Pedido** - Formulário completo com validações
- **Editar Pedido** - Atualização de dados e dimensões
- **Eliminar Pedido** - Remoção com confirmação
- **Detalhes** - Visualização completa incluindo dimensões e anexos

### Dimensões

Cada pedido pode ter múltiplas dimensões com:
- Tipo (Centro de Custo, Cliente, Produto, Marca, Mercado, Projeto)
- Código e Nome
- Percentagem do valor total
- Valor calculado automaticamente

### Anexos

- Upload de múltiplos ficheiros
- Download de anexos
- Eliminação de anexos existentes
- Informação sobre tamanho e tipo de ficheiro

### Pesquisa

Filtros disponíveis:
- Nome do fornecedor (pesquisa parcial)
- Estado (Pendente, Aprovado, Rejeitado, Em Análise)
- Período (data início e data fim)

## Tabelas da Base de Dados

### aprov.approval_hub

Tabela principal com System Versioning que armazena os pedidos de aprovação.

**Campos principais:**
- `id_approval_hub` - ID único
- `tipo_fatura` - Tipo de documento
- `fornecedor_nome` - Nome do fornecedor
- `valor_total` - Valor total da fatura
- `estado` - Estado atual
- `row_version` - Controlo de concorrência
- `valido_de` / `valido_ate` - Período de validade (System Versioning)

### aprov.ids_dimensoes

Armazena as dimensões associadas a cada pedido com System Versioning.

**Campos principais:**
- `id` - ID único
- `id_approval_hub` - FK para approval_hub
- `tipo` - Tipo de dimensão
- `percentagem` - Percentagem (0-100)
- `valor` - Valor calculado

### aprov.anexo

Armazena ficheiros binários anexados aos pedidos.

**Campos principais:**
- `id_anexo` - ID único
- `id_approval_hub` - FK para approval_hub
- `nome_ficheiro` - Nome do ficheiro
- `conteudo` - Dados binários
- `tamanho_bytes` - Tamanho do ficheiro

## System Versioning

As tabelas `approval_hub` e `ids_dimensoes` mantêm histórico completo de alterações:

```sql
-- Ver histórico de um pedido
SELECT *
FROM aprov.approval_hub
FOR SYSTEM_TIME ALL
WHERE id_approval_hub = 1;

-- Ver estado num momento específico
SELECT *
FROM aprov.approval_hub
FOR SYSTEM_TIME AS OF '2024-01-01'
WHERE id_approval_hub = 1;
```

## Segurança

- Proteção CSRF com tokens anti-falsificação
- Validação de dados no cliente e servidor
- Queries parametrizadas para prevenir SQL Injection
- Controlo de concorrência otimista com `row_version`

## Interface do Utilizador

- **Design Responsivo** - Funciona em desktop, tablet e mobile
- **Bootstrap 5.3** - Interface moderna e profissional
- **Bootstrap Icons** - Iconografia consistente
- **Alertas e Toasts** - Feedback visual das operações
- **Validação em Tempo Real** - Feedback imediato ao utilizador

## Estados dos Pedidos

- **Pendente** - Aguarda aprovação (amarelo)
- **Em Análise** - A ser analisado (azul)
- **Aprovado** - Aprovado para processamento (verde)
- **Rejeitado** - Rejeitado (vermelho)

## API Endpoints

### Principais Rotas

- `GET /ApprovalHub` - Lista de pedidos
- `GET /ApprovalHub/Create` - Formulário de criação
- `POST /ApprovalHub/Create` - Criar novo pedido
- `GET /ApprovalHub/Details/{id}` - Ver detalhes
- `GET /ApprovalHub/Edit/{id}` - Formulário de edição
- `POST /ApprovalHub/Edit/{id}` - Atualizar pedido
- `GET /ApprovalHub/Delete/{id}` - Confirmação de eliminação
- `POST /ApprovalHub/Delete/{id}` - Eliminar pedido
- `GET /ApprovalHub/Search` - Pesquisar pedidos
- `GET /ApprovalHub/DownloadAnexo/{id}` - Download de anexo
- `POST /ApprovalHub/DeleteAnexo/{id}` - Eliminar anexo (AJAX)
- `POST /ApprovalHub/UpdateEstado` - Atualizar estado (AJAX)

## Desenvolvimento

### Pré-requisitos

- .NET 9 SDK
- SQL Server 2016+ (para System Versioning)
- Visual Studio 2022+ ou VS Code

### Debug

```bash
dotnet run --environment Development
```

### Publicação

```bash
dotnet publish -c Release -o ./publish
```

## Contribuir

1. Fork o projeto
2. Crie uma branch para a feature (`git checkout -b feature/NovaFuncionalidade`)
3. Commit as alterações (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/NovaFuncionalidade`)
5. Abra um Pull Request

## Licença

Este projeto é privado e propriedade da empresa.

## Suporte

Para questões ou problemas, contacte a equipa de desenvolvimento.

---

**Desenvolvido com .NET 9 e Bootstrap 5.3**
