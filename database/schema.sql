/*
    Centro de Treinamento Pokémon
    Script de criação do banco de dados
    Banco: SQL Server
*/

CREATE TABLE Treinadores
(
    Id INT IDENTITY(1, 1) NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Email VARCHAR(254) NOT NULL,
    CidadeOrigem VARCHAR(150) NOT NULL,

    CONSTRAINT PK_Treinadores
        PRIMARY KEY (Id),

    CONSTRAINT UQ_Treinadores_Email
        UNIQUE (Email),

    CONSTRAINT CK_Treinadores_Nome
        CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),

    CONSTRAINT CK_Treinadores_Email
        CHECK (
            LEN(LTRIM(RTRIM(Email))) > 0
            AND Email LIKE '%@%'
        ),

    CONSTRAINT CK_Treinadores_CidadeOrigem
        CHECK (LEN(LTRIM(RTRIM(CidadeOrigem))) > 0)
);
GO

CREATE TABLE Pokemons
(
    Id INT IDENTITY(1, 1) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    Tipo INT NOT NULL,
    Nivel INT NOT NULL,
    TreinadorId INT NOT NULL,

    CONSTRAINT PK_Pokemons
        PRIMARY KEY (Id),

    CONSTRAINT FK_Pokemons_Treinadores
        FOREIGN KEY (TreinadorId)
        REFERENCES Treinadores (Id),

    CONSTRAINT CK_Pokemons_Nome
        CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),

    CONSTRAINT CK_Pokemons_Tipo
        CHECK (Tipo > 0),

    CONSTRAINT CK_Pokemons_Nivel
        CHECK (Nivel BETWEEN 1 AND 100)
);
GO

CREATE TABLE PlanosTreinamento
(
    Id INT IDENTITY(1, 1) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    ValorMensal DECIMAL(18, 2) NOT NULL,
    Descricao VARCHAR(300) NOT NULL,
    NivelPlano INT NOT NULL,

    CONSTRAINT PK_PlanosTreinamento
        PRIMARY KEY (Id),

    CONSTRAINT UQ_PlanosTreinamento_Nome
        UNIQUE (Nome),

    CONSTRAINT UQ_PlanosTreinamento_NivelPlano
        UNIQUE (NivelPlano),

    CONSTRAINT CK_PlanosTreinamento_Nome
        CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),

    CONSTRAINT CK_PlanosTreinamento_ValorMensal
        CHECK (ValorMensal > 0),

    CONSTRAINT CK_PlanosTreinamento_Descricao
        CHECK (LEN(LTRIM(RTRIM(Descricao))) > 0),

    CONSTRAINT CK_PlanosTreinamento_NivelPlano
        CHECK (NivelPlano BETWEEN 1 AND 3)
);
GO

CREATE TABLE Matriculas
(
    Id INT IDENTITY(1, 1) NOT NULL,
    PokemonId INT NOT NULL,
    PlanoTreinamentoId INT NOT NULL,
    DataInicio DATETIME2 NOT NULL,
    DataEncerramento DATETIME2 NULL,
    Status INT NOT NULL,
    ValorMensal DECIMAL(18, 2) NOT NULL,

    CONSTRAINT PK_Matriculas
        PRIMARY KEY (Id),

    CONSTRAINT FK_Matriculas_Pokemons
        FOREIGN KEY (PokemonId)
        REFERENCES Pokemons (Id),

    CONSTRAINT FK_Matriculas_PlanosTreinamento
        FOREIGN KEY (PlanoTreinamentoId)
        REFERENCES PlanosTreinamento (Id),

    CONSTRAINT CK_Matriculas_Status
        CHECK (Status IN (1, 2, 3)),

    CONSTRAINT CK_Matriculas_ValorMensal
        CHECK (ValorMensal > 0),

    CONSTRAINT CK_Matriculas_DataEncerramento
        CHECK (
            DataEncerramento IS NULL
            OR DataEncerramento >= DataInicio
        )
);
GO

CREATE INDEX IX_Pokemons_TreinadorId
    ON Pokemons (TreinadorId);
GO

CREATE INDEX IX_Pokemons_Nome
    ON Pokemons (Nome);
GO

CREATE INDEX IX_Matriculas_PokemonId
    ON Matriculas (PokemonId);
GO

CREATE INDEX IX_Matriculas_PlanoTreinamentoId
    ON Matriculas (PlanoTreinamentoId);
GO

CREATE INDEX IX_Matriculas_Status
    ON Matriculas (Status);
GO

CREATE INDEX IX_Matriculas_DataInicio
    ON Matriculas (DataInicio);
GO

CREATE UNIQUE INDEX UQ_Matriculas_PokemonAtivo
    ON Matriculas (PokemonId)
    WHERE Status = 1;
GO