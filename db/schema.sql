-- =============================================================================
-- Centro de Treinamento Pokémon "Alto Nível" - Schema inicial (SQL Server)
-- =============================================================================
-- Escopo desta entrega: estrutura das tabelas e constraints estruturais.
-- A carga de dados de teste e eventuais migrações incrementais ficam para a
-- próxima etapa (implementação das regras R1-R5 no backend).
-- =============================================================================

IF DB_ID('PokemonTrainingCenter') IS NULL
BEGIN
    CREATE DATABASE PokemonTrainingCenter;
END
GO

USE PokemonTrainingCenter;
GO

-- Exigido pelo SQL Server para criar índices filtrados (usado em UQ_Matriculas_PokemonAtiva, R1).
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- -----------------------------------------------------------------------------
-- Treinadores
-- -----------------------------------------------------------------------------
CREATE TABLE Treinadores (
    Id              INT IDENTITY(1,1)   PRIMARY KEY,
    Nome            NVARCHAR(150)       NOT NULL,
    Email           NVARCHAR(200)       NOT NULL,
    CidadeOrigem    NVARCHAR(150)       NOT NULL,
    CONSTRAINT UQ_Treinadores_Email UNIQUE (Email)
);
GO

-- -----------------------------------------------------------------------------
-- Pokemons
-- -----------------------------------------------------------------------------
CREATE TABLE Pokemons (
    Id              INT IDENTITY(1,1)   PRIMARY KEY,
    Nome            NVARCHAR(150)       NOT NULL,
    Tipo            NVARCHAR(30)        NOT NULL,
    Nivel           INT                 NOT NULL,
    TreinadorId     INT                 NOT NULL,
    CONSTRAINT FK_Pokemons_Treinadores FOREIGN KEY (TreinadorId) REFERENCES Treinadores(Id),
    CONSTRAINT CK_Pokemons_Nivel CHECK (Nivel BETWEEN 1 AND 100)
);
GO

-- -----------------------------------------------------------------------------
-- PlanosTreinamento
-- -----------------------------------------------------------------------------
CREATE TABLE PlanosTreinamento (
    Id                  INT IDENTITY(1,1)   PRIMARY KEY,
    Nome                NVARCHAR(100)       NOT NULL,
    Descricao           NVARCHAR(300)       NOT NULL,
    ValorMensal         DECIMAL(10,2)       NOT NULL,
    Nivel               INT                 NOT NULL,  -- hierarquia p/ validar upgrade/downgrade (R2)
    NivelMinimoPokemon  INT                 NOT NULL DEFAULT 1,  -- R3: Elite dos 4 exige 50+
    CONSTRAINT UQ_PlanosTreinamento_Nome UNIQUE (Nome)
);
GO

INSERT INTO PlanosTreinamento (Nome, Descricao, ValorMensal, Nivel, NivelMinimoPokemon) VALUES
    (N'Ginásio Local', N'Treinos básicos', 50.00, 1, 1),
    (N'Liga Regional', N'Treinos intermediários + batalhas simuladas', 120.00, 2, 1),
    (N'Elite dos 4', N'Preparação completa para a Liga', 300.00, 3, 50);
GO

-- -----------------------------------------------------------------------------
-- Matriculas
-- -----------------------------------------------------------------------------
CREATE TABLE Matriculas (
    Id                  INT IDENTITY(1,1)   PRIMARY KEY,
    PokemonId           INT                 NOT NULL,
    PlanoTreinamentoId  INT                 NOT NULL,
    DataInicio          DATE                NOT NULL,
    DataFim             DATE                NULL,
    Status              NVARCHAR(20)        NOT NULL,  -- 'Ativa' | 'Cancelada' | 'Concluida'
    ValorMensal         DECIMAL(10,2)       NOT NULL,  -- valor congelado no momento da matrícula/upgrade
    MatriculaOrigemId   INT                 NULL,      -- aponta para a matrícula encerrada em um upgrade (R2)
    CONSTRAINT FK_Matriculas_Pokemons FOREIGN KEY (PokemonId) REFERENCES Pokemons(Id),
    CONSTRAINT FK_Matriculas_PlanosTreinamento FOREIGN KEY (PlanoTreinamentoId) REFERENCES PlanosTreinamento(Id),
    CONSTRAINT FK_Matriculas_MatriculaOrigem FOREIGN KEY (MatriculaOrigemId) REFERENCES Matriculas(Id),
    CONSTRAINT CK_Matriculas_Status CHECK (Status IN (N'Ativa', N'Cancelada', N'Concluida'))
);
GO

-- R1 — um Pokémon não pode ter duas matrículas Ativas ao mesmo tempo.
-- Reforçado a nível de banco via índice único filtrado (defesa em profundidade,
-- além da validação feita na camada de aplicação).
CREATE UNIQUE INDEX UQ_Matriculas_PokemonAtiva
    ON Matriculas (PokemonId)
    WHERE Status = N'Ativa';
GO
