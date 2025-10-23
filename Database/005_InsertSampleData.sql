-- Insert sample data for testing
-- Database: InfgestMVC_TESTES

USE [InfgestMVC_TESTES];
GO

-- Insert sample approval hub records
INSERT INTO aprov.approval_hub (
    tipo_fatura, documento_a_criar, comprador, aprovador,
    fornecedor_bc_id, fornecedor_nome, local_livre, observacoes,
    valor_total, num_fatura, data_documento, empresa_nome,
    estado, criado_por
)
VALUES
(
    'Fatura',
    'Ordem de Pagamento',
    'João Silva',
    'Maria Santos',
    'FOR001',
    'Fornecedor Exemplo Lda',
    'Lisboa',
    'Fatura de teste para material de escritório',
    1500.00,
    'FT 2024/001',
    '2024-01-15',
    'Empresa Teste',
    'Pendente',
    'SYSTEM'
),
(
    'Nota de Crédito',
    'Nota de Lançamento',
    'Ana Costa',
    'Pedro Oliveira',
    'FOR002',
    'Fornecedor Premium SA',
    'Porto',
    'Nota de crédito relativa a devolução',
    750.50,
    'NC 2024/005',
    '2024-01-20',
    'Empresa Teste',
    'Aprovado',
    'SYSTEM'
),
(
    'Fatura',
    'Fatura Fornecedor',
    'Carlos Mendes',
    'Sofia Rodrigues',
    'FOR003',
    'Fornecedor Global Lda',
    'Faro',
    'Serviços de manutenção',
    2300.00,
    'FT 2024/012',
    '2024-01-25',
    'Empresa Teste',
    'Em Análise',
    'SYSTEM'
);
GO

-- Get the IDs of the inserted records
DECLARE @id1 INT = (SELECT id_approval_hub FROM aprov.approval_hub WHERE num_fatura = 'FT 2024/001');
DECLARE @id2 INT = (SELECT id_approval_hub FROM aprov.approval_hub WHERE num_fatura = 'NC 2024/005');
DECLARE @id3 INT = (SELECT id_approval_hub FROM aprov.approval_hub WHERE num_fatura = 'FT 2024/012');

-- Insert sample dimensões for first approval
IF @id1 IS NOT NULL
BEGIN
    INSERT INTO aprov.ids_dimensoes (id_approval_hub, Linha, tipo, cod_dimensao, nome_dimensao, percentagem, valor)
    VALUES
    (@id1, 1, 'CentroCusto', 'CC001', 'Administração', 60.00, 900.00),
    (@id1, 2, 'CentroCusto', 'CC002', 'Vendas', 40.00, 600.00),
    (@id1, 3, 'Projeto', 'PRJ001', 'Projeto Alpha', 100.00, 1500.00);
END

-- Insert sample dimensões for second approval
IF @id2 IS NOT NULL
BEGIN
    INSERT INTO aprov.ids_dimensoes (id_approval_hub, Linha, tipo, cod_dimensao, nome_dimensao, percentagem, valor)
    VALUES
    (@id2, 1, 'Cliente', 'CLI001', 'Cliente XYZ', 100.00, 750.50);
END

-- Insert sample dimensões for third approval
IF @id3 IS NOT NULL
BEGIN
    INSERT INTO aprov.ids_dimensoes (id_approval_hub, Linha, tipo, cod_dimensao, nome_dimensao, percentagem, valor)
    VALUES
    (@id3, 1, 'CentroCusto', 'CC003', 'Produção', 50.00, 1150.00),
    (@id3, 2, 'Marca', 'MRC001', 'Marca Premium', 50.00, 1150.00);
END

PRINT 'Sample data inserted successfully';
PRINT 'Total approval hub records: ' + CAST((SELECT COUNT(*) FROM aprov.approval_hub) AS NVARCHAR(10));
PRINT 'Total dimensões records: ' + CAST((SELECT COUNT(*) FROM aprov.ids_dimensoes) AS NVARCHAR(10));
GO
