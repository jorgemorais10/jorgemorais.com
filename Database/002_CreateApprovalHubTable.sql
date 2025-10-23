-- Create approval_hub table with System Versioning
-- Database: InfgestMVC_TESTES

USE [InfgestMVC_TESTES];
GO

-- Drop table if exists (for development)
IF OBJECT_ID('aprov.approval_hub', 'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT * FROM sys.tables
        WHERE name = 'approval_hub'
        AND schema_id = SCHEMA_ID('aprov')
        AND temporal_type = 2
    )
    BEGIN
        ALTER TABLE aprov.approval_hub SET (SYSTEM_VERSIONING = OFF);
        DROP TABLE IF EXISTS aprov.approval_hub_history;
    END
    DROP TABLE aprov.approval_hub;
END
GO

-- Create main table
CREATE TABLE aprov.approval_hub
(
    id_approval_hub INT IDENTITY(1,1) NOT NULL,
    tipo_fatura NVARCHAR(50) NULL,
    documento_a_criar NVARCHAR(100) NULL,
    comprador NVARCHAR(100) NULL,
    aprovador NVARCHAR(100) NULL,
    fornecedor_bc_id NVARCHAR(50) NULL,
    fornecedor_nome NVARCHAR(200) NULL,
    local_livre NVARCHAR(200) NULL,
    observacoes NVARCHAR(MAX) NULL,
    valor_total DECIMAL(18,2) NOT NULL DEFAULT 0,
    num_fatura NVARCHAR(50) NULL,
    data_documento DATE NULL,
    empresa_nome NVARCHAR(200) NULL,
    estado NVARCHAR(50) NULL DEFAULT 'Pendente',
    criado_por NVARCHAR(100) NULL,
    criado_em DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    atualizado_em DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    -- System Versioning columns
    row_version ROWVERSION NOT NULL,
    valido_de DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    valido_ate DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,

    PERIOD FOR SYSTEM_TIME (valido_de, valido_ate),

    CONSTRAINT PK_approval_hub PRIMARY KEY CLUSTERED (id_approval_hub)
)
WITH
(
    SYSTEM_VERSIONING = ON (HISTORY_TABLE = aprov.approval_hub_history)
);
GO

-- Create indexes
CREATE NONCLUSTERED INDEX IX_approval_hub_fornecedor
ON aprov.approval_hub(fornecedor_nome);
GO

CREATE NONCLUSTERED INDEX IX_approval_hub_estado
ON aprov.approval_hub(estado);
GO

CREATE NONCLUSTERED INDEX IX_approval_hub_data_documento
ON aprov.approval_hub(data_documento);
GO

CREATE NONCLUSTERED INDEX IX_approval_hub_criado_em
ON aprov.approval_hub(criado_em DESC);
GO

PRINT 'Table aprov.approval_hub created with System Versioning';
GO
