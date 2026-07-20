/*
    Centro de Treinamento Pokémon
    Script de criação do banco de dados
    Banco: SQL Server
*/

CREATE TABLE Treinador
(
    Id INT IDENTITY(1, 1) NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Email VARCHAR(254) NOT NULL,
    CidadeOrigem VARCHAR(150) NOT NULL,

    CONSTRAINT PK_Treinador
        PRIMARY KEY (Id),

    CONSTRAINT UQ_Treinador_Email
        UNIQUE (Email),

    CONSTRAINT CK_Treinador_Nome
        CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),

    CONSTRAINT CK_Treinador_Email
        CHECK
        (
            LEN(LTRIM(RTRIM(Email))) > 0
            AND Email LIKE '%@%'
        ),

    CONSTRAINT CK_Treinador_CidadeOrigem
        CHECK (LEN(LTRIM(RTRIM(CidadeOrigem))) > 0)
);
GO

CREATE TABLE Pokemon
(
    Id INT IDENTITY(1, 1) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    Tipo INT NOT NULL,
    Nivel INT NOT NULL,
    TreinadorId INT NOT NULL,

    CONSTRAINT PK_Pokemon
        PRIMARY KEY (Id),

    CONSTRAINT FK_Pokemon_Treinador
        FOREIGN KEY (TreinadorId)
        REFERENCES Treinador (Id),

    CONSTRAINT CK_Pokemon_Nome
        CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),

    CONSTRAINT CK_Pokemon_Tipo
        CHECK (Tipo > 0),

    CONSTRAINT CK_Pokemon_Nivel
        CHECK (Nivel BETWEEN 1 AND 100)
);
GO

CREATE TABLE PlanoTreinamento
(
    Id INT IDENTITY(1, 1) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    ValorMensal DECIMAL(18, 2) NOT NULL,
    Descricao VARCHAR(300) NOT NULL,
    NivelPlano INT NOT NULL,

    CONSTRAINT PK_PlanoTreinamento
        PRIMARY KEY (Id),

    CONSTRAINT UQ_PlanoTreinamento_Nome
        UNIQUE (Nome),

    CONSTRAINT UQ_PlanoTreinamento_NivelPlano
        UNIQUE (NivelPlano),

    CONSTRAINT CK_PlanoTreinamento_Nome
        CHECK (LEN(LTRIM(RTRIM(Nome))) > 0),

    CONSTRAINT CK_PlanoTreinamento_ValorMensal
        CHECK (ValorMensal > 0),

    CONSTRAINT CK_PlanoTreinamento_Descricao
        CHECK (LEN(LTRIM(RTRIM(Descricao))) > 0),

    CONSTRAINT CK_PlanoTreinamento_NivelPlano
        CHECK (NivelPlano BETWEEN 1 AND 3)
);
GO

CREATE TABLE Matricula
(
    Id INT IDENTITY(1, 1) NOT NULL,
    PokemonId INT NOT NULL,
    PlanoTreinamentoId INT NOT NULL,
    DataInicio DATETIME2 NOT NULL,
    DataEncerramento DATETIME2 NULL,
    Status INT NOT NULL,
    ValorMensal DECIMAL(18, 2) NOT NULL,

    CONSTRAINT PK_Matricula
        PRIMARY KEY (Id),

    CONSTRAINT FK_Matricula_Pokemon
        FOREIGN KEY (PokemonId)
        REFERENCES Pokemon (Id),

    CONSTRAINT FK_Matricula_PlanoTreinamento
        FOREIGN KEY (PlanoTreinamentoId)
        REFERENCES PlanoTreinamento (Id),

    CONSTRAINT CK_Matricula_Status
        CHECK (Status IN (1, 2, 3)),

    CONSTRAINT CK_Matricula_ValorMensal
        CHECK (ValorMensal > 0),

    CONSTRAINT CK_Matricula_DataEncerramento
        CHECK
        (
            DataEncerramento IS NULL
            OR DataEncerramento >= DataInicio
        )
);
GO

CREATE INDEX IX_Pokemon_TreinadorId
    ON Pokemon (TreinadorId);
GO

CREATE INDEX IX_Pokemon_Nome
    ON Pokemon (Nome);
GO

CREATE INDEX IX_Matricula_PokemonId
    ON Matricula (PokemonId);
GO

CREATE INDEX IX_Matricula_PlanoTreinamentoId
    ON Matricula (PlanoTreinamentoId);
GO

CREATE INDEX IX_Matricula_Status
    ON Matricula (Status);
GO

CREATE INDEX IX_Matricula_DataInicio
    ON Matricula (DataInicio);
GO

CREATE UNIQUE INDEX UQ_Matricula_PokemonAtivo
    ON Matricula (PokemonId)
    WHERE Status = 1;
GO