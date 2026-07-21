-- =========================================
-- Centro de Treinamento Pokémon "Alto Nível"
-- Script de criação das tabelas
-- =========================================

CREATE TABLE Treinadores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(150) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    CidadeOrigem NVARCHAR(100) NULL,
    CONSTRAINT UQ_Treinadores_Email UNIQUE (Email)
);

CREATE TABLE Planos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(50) NOT NULL,
    ValorMensal DECIMAL(10,2) NOT NULL,
    NivelMinimoRequerido INT NOT NULL DEFAULT 1
);

CREATE TABLE Pokemons (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(100) NOT NULL,
    Tipo NVARCHAR(20) NOT NULL,
    Nivel INT NOT NULL,
    TreinadorId INT NOT NULL,
    CONSTRAINT FK_Pokemons_Treinadores FOREIGN KEY (TreinadorId) REFERENCES Treinadores(Id),
    CONSTRAINT CK_Pokemons_Nivel CHECK (Nivel BETWEEN 1 AND 100)
);

CREATE TABLE Matriculas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PokemonId INT NOT NULL,
    PlanoId INT NOT NULL,
    DataInicio DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL, 
    ValorMensal DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_Matriculas_Pokemons FOREIGN KEY (PokemonId) REFERENCES Pokemons(Id),
    CONSTRAINT FK_Matriculas_Planos FOREIGN KEY (PlanoId) REFERENCES Planos(Id),
    CONSTRAINT CK_Matriculas_Status CHECK (Status IN ('Ativa', 'Cancelada', 'Concluida'))
);
GO

CREATE UNIQUE INDEX UX_Matriculas_PokemonAtivo
    ON Matriculas (PokemonId)
    WHERE Status = 'Ativa';
GO

INSERT INTO Planos (Nome, ValorMensal, NivelMinimoRequerido) VALUES
    ('Ginásio Local', 50.00, 1),
    ('Liga Regional', 120.00, 1),
    ('Elite dos 4', 300.00, 50);
GO