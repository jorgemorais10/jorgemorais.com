-- Create anexo table (attachments)
-- Database: InfgestMVC_TESTES

USE [InfgestMVC_TESTES];
GO

-- Drop table if exists (for development)
IF OBJECT_ID('aprov.anexo', 'U') IS NOT NULL
BEGIN
    DROP TABLE aprov.anexo;
END
GO

-- Create anexo table (no system versioning for binary data)
CREATE TABLE aprov.anexo
(
    id_anexo INT IDENTITY(1,1) NOT NULL,
    id_approval_hub INT NOT NULL,
    nome_ficheiro NVARCHAR(255) NULL,
    content_type NVARCHAR(100) NULL,
    tamanho_bytes BIGINT NOT NULL DEFAULT 0,
    conteudo VARBINARY(MAX) NULL,
    carregado_por NVARCHAR(100) NULL,
    carregado_em DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_anexo PRIMARY KEY CLUSTERED (id_anexo),
    CONSTRAINT FK_anexo_approval_hub
        FOREIGN KEY (id_approval_hub)
        REFERENCES aprov.approval_hub(id_approval_hub)
        ON DELETE CASCADE
);
GO

-- Create indexes
CREATE NONCLUSTERED INDEX IX_anexo_approval_hub
ON aprov.anexo(id_approval_hub);
GO

CREATE NONCLUSTERED INDEX IX_anexo_carregado_em
ON aprov.anexo(carregado_em DESC);
GO

PRINT 'Table aprov.anexo created';
GO
