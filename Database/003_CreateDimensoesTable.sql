-- Create ids_dimensoes table with System Versioning
-- Database: InfgestMVC_TESTES

USE [InfgestMVC_TESTES];
GO

-- Drop table if exists (for development)
IF OBJECT_ID('aprov.ids_dimensoes', 'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT * FROM sys.tables
        WHERE name = 'ids_dimensoes'
        AND schema_id = SCHEMA_ID('aprov')
        AND temporal_type = 2
    )
    BEGIN
        ALTER TABLE aprov.ids_dimensoes SET (SYSTEM_VERSIONING = OFF);
        DROP TABLE IF EXISTS aprov.ids_dimensoes_history;
    END
    DROP TABLE aprov.ids_dimensoes;
END
GO

-- Create dimensões table
CREATE TABLE aprov.ids_dimensoes
(
    id INT IDENTITY(1,1) NOT NULL,
    id_approval_hub INT NOT NULL,
    Linha INT NOT NULL,
    tipo NVARCHAR(50) NULL,  -- CentroCusto, Cliente, Produto, Marca, Mercado, Projeto
    cod_dimensao NVARCHAR(50) NULL,
    nome_dimensao NVARCHAR(200) NULL,
    percentagem DECIMAL(5,2) NULL,  -- 0-100
    valor DECIMAL(18,2) NULL,

    -- System Versioning columns
    valido_de DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    valido_ate DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,

    PERIOD FOR SYSTEM_TIME (valido_de, valido_ate),

    CONSTRAINT PK_ids_dimensoes PRIMARY KEY CLUSTERED (id),
    CONSTRAINT FK_ids_dimensoes_approval_hub
        FOREIGN KEY (id_approval_hub)
        REFERENCES aprov.approval_hub(id_approval_hub)
        ON DELETE CASCADE,
    CONSTRAINT CHK_percentagem CHECK (percentagem >= 0 AND percentagem <= 100)
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

CREATE NONCLUSTERED INDEX IX_ids_dimensoes_tipo
ON aprov.ids_dimensoes(tipo);
GO

PRINT 'Table aprov.ids_dimensoes created with System Versioning';
GO
