-- ============================================================
-- Centro de Treinamento Pokémon "Alto Nível"
-- Schema SQL Server
-- ============================================================

CREATE DATABASE PokemonTrainingCenter;
GO

USE PokemonTrainingCenter;
GO

-- ------------------------------------------------------------
-- Treinadores
-- ------------------------------------------------------------
CREATE TABLE Treinadores (
    Id            INT             IDENTITY(1,1) PRIMARY KEY,
    Nome          NVARCHAR(100)   NOT NULL,
    Email         NVARCHAR(150)   NOT NULL,
    CidadeOrigem  NVARCHAR(100)   NOT NULL,
    CONSTRAINT UQ_Treinadores_Email UNIQUE (Email)
);

-- ------------------------------------------------------------
-- Pokemons
-- ------------------------------------------------------------
CREATE TABLE Pokemons (
    Id          INT             IDENTITY(1,1) PRIMARY KEY,
    Nome        NVARCHAR(100)   NOT NULL,
    Tipo        NVARCHAR(50)    NOT NULL,
    Nivel       INT             NOT NULL CHECK (Nivel BETWEEN 1 AND 100),
    TreinadorId INT             NOT NULL,
    CONSTRAINT FK_Pokemons_Treinadores FOREIGN KEY (TreinadorId)
        REFERENCES Treinadores(Id)
);

-- ------------------------------------------------------------
-- Matriculas
-- ------------------------------------------------------------
CREATE TABLE Matriculas (
    Id          INT             IDENTITY(1,1) PRIMARY KEY,
    PokemonId   INT             NOT NULL,
    Plano       NVARCHAR(20)    NOT NULL CHECK (Plano IN ('GinasioLocal','LigaRegional','EliteDos4')),
    DataInicio  DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    Status      NVARCHAR(15)    NOT NULL CHECK (Status IN ('Ativa','Cancelada','Concluida')),
    ValorMensal DECIMAL(10,2)   NOT NULL,
    CONSTRAINT FK_Matriculas_Pokemons FOREIGN KEY (PokemonId)
        REFERENCES Pokemons(Id)
);

-- Garante que um Pokémon só tenha uma matrícula ativa por vez (R1)
CREATE UNIQUE INDEX UX_Matriculas_Pokemon_Ativa
    ON Matriculas (PokemonId)
    WHERE Status = 'Ativa';

GO
