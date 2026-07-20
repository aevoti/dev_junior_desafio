SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO

IF DB_ID(N'PokemonTrainingDb') IS NULL
BEGIN
    CREATE DATABASE PokemonTrainingDb;
END;
GO

USE PokemonTrainingDb;
GO

IF OBJECT_ID(N'dbo.Treinadores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Treinadores
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Treinadores PRIMARY KEY,
        Nome NVARCHAR(150) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        CidadeOrigem NVARCHAR(150) NOT NULL
    );

END;
GO

IF OBJECT_ID(N'dbo.Pokemons', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pokemons
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pokemons PRIMARY KEY,
        Nome NVARCHAR(120) NOT NULL,
        Tipo NVARCHAR(80) NOT NULL,
        Nivel INT NOT NULL,
        TreinadorId INT NOT NULL,
        CONSTRAINT CK_Pokemons_Nivel CHECK (Nivel >= 1 AND Nivel <= 100),
        CONSTRAINT FK_Pokemons_Treinadores_TreinadorId
            FOREIGN KEY (TreinadorId) REFERENCES dbo.Treinadores (Id) ON DELETE NO ACTION
    );

END;
GO

IF OBJECT_ID(N'dbo.PlanosTreinamento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PlanosTreinamento
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PlanosTreinamento PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        ValorMensal DECIMAL(10,2) NOT NULL,
        Descricao NVARCHAR(300) NOT NULL,
        Ordem INT NOT NULL,
        NivelMinimo INT NOT NULL,
        CONSTRAINT CK_PlanosTreinamento_ValorMensal CHECK (ValorMensal > 0),
        CONSTRAINT CK_PlanosTreinamento_Ordem CHECK (Ordem > 0),
        CONSTRAINT CK_PlanosTreinamento_NivelMinimo
            CHECK (NivelMinimo >= 1 AND NivelMinimo <= 100)
    );

END;
GO

IF OBJECT_ID(N'dbo.Matriculas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Matriculas
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Matriculas PRIMARY KEY,
        PokemonId INT NOT NULL,
        PlanoTreinamentoId INT NOT NULL,
        DataInicio DATETIME2 NOT NULL,
        DataFim DATETIME2 NULL,
        Status VARCHAR(20) NOT NULL,
        ValorMensal DECIMAL(10,2) NOT NULL,
        MotivoEncerramento NVARCHAR(500) NULL,
        CONSTRAINT CK_Matriculas_Status CHECK (Status IN ('Ativa', 'Cancelada', 'Concluida')),
        CONSTRAINT CK_Matriculas_ValorMensal CHECK (ValorMensal > 0),
        CONSTRAINT FK_Matriculas_Pokemons_PokemonId
            FOREIGN KEY (PokemonId) REFERENCES dbo.Pokemons (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Matriculas_PlanosTreinamento_PlanoTreinamentoId
            FOREIGN KEY (PlanoTreinamentoId) REFERENCES dbo.PlanosTreinamento (Id) ON DELETE NO ACTION
    );

END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Treinadores_Email' AND object_id = OBJECT_ID(N'dbo.Treinadores'))
    CREATE UNIQUE INDEX UX_Treinadores_Email ON dbo.Treinadores (Email);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pokemons_TreinadorId' AND object_id = OBJECT_ID(N'dbo.Pokemons'))
    CREATE INDEX IX_Pokemons_TreinadorId ON dbo.Pokemons (TreinadorId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PlanosTreinamento_Nome' AND object_id = OBJECT_ID(N'dbo.PlanosTreinamento'))
    CREATE UNIQUE INDEX UX_PlanosTreinamento_Nome ON dbo.PlanosTreinamento (Nome);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_PlanosTreinamento_Ordem' AND object_id = OBJECT_ID(N'dbo.PlanosTreinamento'))
    CREATE UNIQUE INDEX UX_PlanosTreinamento_Ordem ON dbo.PlanosTreinamento (Ordem);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Matriculas_PokemonId' AND object_id = OBJECT_ID(N'dbo.Matriculas'))
    CREATE INDEX IX_Matriculas_PokemonId ON dbo.Matriculas (PokemonId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Matriculas_PlanoTreinamentoId' AND object_id = OBJECT_ID(N'dbo.Matriculas'))
    CREATE INDEX IX_Matriculas_PlanoTreinamentoId ON dbo.Matriculas (PlanoTreinamentoId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Matriculas_Status' AND object_id = OBJECT_ID(N'dbo.Matriculas'))
    CREATE INDEX IX_Matriculas_Status ON dbo.Matriculas (Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Matriculas_PokemonId_Ativa' AND object_id = OBJECT_ID(N'dbo.Matriculas'))
    CREATE UNIQUE INDEX UX_Matriculas_PokemonId_Ativa
        ON dbo.Matriculas (PokemonId)
        WHERE Status = 'Ativa';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.PlanosTreinamento WHERE Id = 1)
BEGIN
    SET IDENTITY_INSERT dbo.PlanosTreinamento ON;
    INSERT INTO dbo.PlanosTreinamento (Id, Nome, ValorMensal, Descricao, Ordem, NivelMinimo)
    VALUES (1, N'Gin' + NCHAR(225) + N'sio Local', 50.00, N'Treinos b' + NCHAR(225) + N'sicos', 1, 1);
    SET IDENTITY_INSERT dbo.PlanosTreinamento OFF;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.PlanosTreinamento WHERE Id = 2)
BEGIN
    SET IDENTITY_INSERT dbo.PlanosTreinamento ON;
    INSERT INTO dbo.PlanosTreinamento (Id, Nome, ValorMensal, Descricao, Ordem, NivelMinimo)
    VALUES (2, N'Liga Regional', 120.00, N'Treinos intermedi' + NCHAR(225) + N'rios e batalhas simuladas', 2, 1);
    SET IDENTITY_INSERT dbo.PlanosTreinamento OFF;
END;

IF NOT EXISTS (SELECT 1 FROM dbo.PlanosTreinamento WHERE Id = 3)
BEGIN
    SET IDENTITY_INSERT dbo.PlanosTreinamento ON;
    INSERT INTO dbo.PlanosTreinamento (Id, Nome, ValorMensal, Descricao, Ordem, NivelMinimo)
    VALUES (3, N'Elite dos 4', 300.00, N'Prepara' + NCHAR(231) + NCHAR(227) + N'o completa para a Liga', 3, 50);
    SET IDENTITY_INSERT dbo.PlanosTreinamento OFF;
END;
GO
