CREATE DATABASE PokemonCenter;
GO
USE PokemonCenter;
GO

SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE Planos (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    nome            VARCHAR(50)     NOT NULL,
    descricao       VARCHAR(200)    NOT NULL,
    valorMensal     DECIMAL(10,2)   NOT NULL
);

CREATE TABLE Treinador (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    nome            VARCHAR(100)    NOT NULL,
    email           VARCHAR(150)    NOT NULL UNIQUE,
    cidadeDeOrigem  VARCHAR(100)    NOT NULL
);

CREATE TABLE Pokemon (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    nome            VARCHAR(100)    NOT NULL,
    tipo            VARCHAR(50)     NOT NULL,
    nivel           INT             NOT NULL,
    treinadorId     INT             NOT NULL,

    CONSTRAINT FK_Pokemon_Treinador FOREIGN KEY (treinadorId)
        REFERENCES Treinador(id),

    CONSTRAINT CK_Pokemon_Nivel CHECK (nivel BETWEEN 1 AND 100)
);

CREATE TABLE Matricula (
    id                      INT IDENTITY(1,1) PRIMARY KEY,
    pokemonId               INT             NOT NULL,
    planoId                 INT             NOT NULL,
    dataInicio              DATE            NOT NULL,
    dataFim                 DATE            NULL,
    status                  VARCHAR(20)     NOT NULL,
    valorPrimeiraCobranca   DECIMAL(10,2)   NOT NULL,

    CONSTRAINT FK_Matricula_Pokemon FOREIGN KEY (pokemonId)
        REFERENCES Pokemon(id),

    CONSTRAINT FK_Matricula_Plano FOREIGN KEY (planoId)
        REFERENCES Planos(id),

    CONSTRAINT CK_Matricula_Status CHECK (status IN ('Ativa', 'Cancelada', 'Concluida'))
);

CREATE UNIQUE INDEX UX_Matricula_PokemonAtiva
    ON Matricula(pokemonId)
    WHERE status = 'Ativa';

INSERT INTO Planos (nome, descricao, valorMensal) VALUES
    ('Ginásio Local',  'Treinos básicos',                              50.00),
    ('Liga Regional',  'Treinos intermediários + batalhas simuladas', 120.00),
    ('Elite dos 4',    'Preparação completa para a Liga',             300.00);
