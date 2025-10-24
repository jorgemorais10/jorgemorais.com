-- Migrate ids_dimensoes table to support complete dimension lines
-- Each line now represents a product with all its dimensions
-- Database: InfgestMVC_TESTES

USE [InfgestMVC_TESTES];
GO

-- Disable system versioning temporarily
IF EXISTS (
    SELECT * FROM sys.tables
    WHERE name = 'ids_dimensoes'
    AND schema_id = SCHEMA_ID('aprov')
    AND temporal_type = 2
)
BEGIN
    ALTER TABLE aprov.ids_dimensoes SET (SYSTEM_VERSIONING = OFF);
    PRINT 'System versioning disabled';
END
GO

-- Drop old table and history
DROP TABLE IF EXISTS aprov.ids_dimensoes_history;
DROP TABLE IF EXISTS aprov.ids_dimensoes;
GO

-- Create new complete dimension lines table
CREATE TABLE aprov.ids_dimensoes
(
    id INT IDENTITY(1,1) NOT NULL,
    id_approval_hub INT NOT NULL,
    linha INT NOT NULL,

    -- Produto (chave principal)
    produto_cod NVARCHAR(50) NULL,
    produto_nome NVARCHAR(200) NULL,
    produto_percentagem DECIMAL(5,2) NULL,
    produto_valor DECIMAL(18,2) NULL,

    -- Centro de Custo
    centro_custo_cod NVARCHAR(50) NULL,
    centro_custo_nome NVARCHAR(200) NULL,
    centro_custo_percentagem DECIMAL(5,2) NULL,
    centro_custo_valor DECIMAL(18,2) NULL,

    -- Cliente
    cliente_cod NVARCHAR(50) NULL,
    cliente_nome NVARCHAR(200) NULL,
    cliente_percentagem DECIMAL(5,2) NULL,
    cliente_valor DECIMAL(18,2) NULL,

    -- Marca
    marca_cod NVARCHAR(50) NULL,
    marca_nome NVARCHAR(200) NULL,
    marca_percentagem DECIMAL(5,2) NULL,
    marca_valor DECIMAL(18,2) NULL,

    -- Mercado
    mercado_cod NVARCHAR(50) NULL,
    mercado_nome NVARCHAR(200) NULL,
    mercado_percentagem DECIMAL(5,2) NULL,
    mercado_valor DECIMAL(18,2) NULL,

    -- Projeto
    projeto_cod NVARCHAR(50) NULL,
    projeto_nome NVARCHAR(200) NULL,
    projeto_percentagem DECIMAL(5,2) NULL,
    projeto_valor DECIMAL(18,2) NULL,

    -- System Versioning columns
    valido_de DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    valido_ate DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,

    PERIOD FOR SYSTEM_TIME (valido_de, valido_ate),

    CONSTRAINT PK_ids_dimensoes PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_ids_dimensoes_approval_hub
        FOREIGN KEY (id_approval_hub)
        REFERENCES aprov.approval_hub(id_approval_hub)
        ON DELETE CASCADE,
    CONSTRAINT CHK_produto_percentagem CHECK (produto_percentagem IS NULL OR (produto_percentagem >= 0 AND produto_percentagem <= 100)),
    CONSTRAINT CHK_centro_custo_percentagem CHECK (centro_custo_percentagem IS NULL OR (centro_custo_percentagem >= 0 AND centro_custo_percentagem <= 100)),
    CONSTRAINT CHK_cliente_percentagem CHECK (cliente_percentagem IS NULL OR (cliente_percentagem >= 0 AND cliente_percentagem <= 100)),
    CONSTRAINT CHK_marca_percentagem CHECK (marca_percentagem IS NULL OR (marca_percentagem >= 0 AND marca_percentagem <= 100)),
    CONSTRAINT CHK_mercado_percentagem CHECK (mercado_percentagem IS NULL OR (mercado_percentagem >= 0 AND mercado_percentagem <= 100)),
    CONSTRAINT CHK_projeto_percentagem CHECK (projeto_percentagem IS NULL OR (projeto_percentagem >= 0 AND projeto_percentagem <= 100))
)
WITH
(
    SYSTEM_VERSIONING = ON (HISTORY_TABLE = aprov.ids_dimensoes_history)
);
GO

-- Create indexes
CREATE NONCLUSTERED INDEX IX_ids_dimensoes_approval_hub
ON aprov.ids_dimensoes(id_approval_hub);
GO

CREATE NONCLUSTERED INDEX IX_ids_dimensoes_produto
ON aprov.ids_dimensoes(produto_cod);
GO

PRINT 'Table aprov.ids_dimensoes migrated to complete dimension lines structure with System Versioning';
GO
