-- =============================================================================
-- Seed de dados de teste — Centro de Treinamento Pokémon "Alto Nível"
-- Popula 10 Treinadores, 10 Pokémon (1:1) e 10 Matrículas cobrindo os três
-- planos e os três status (Ativa, Cancelada, Concluída), incluindo uma cadeia
-- de upgrade (R2) pronta pra testar o fluxo fim-a-fim.
-- Idempotente: remove qualquer leva anterior deste seed (pelo domínio de
-- e-mail) antes de inserir de novo, então pode rodar quantas vezes quiser.
-- Pré-requisito: schema.sql já aplicado.
-- =============================================================================
USE PokemonTrainingCenter;
GO

-- Exigido pelo SQL Server para mexer em tabelas com índices filtrados (Matriculas, R1).
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DELETE m FROM Matriculas m
    JOIN Pokemons p ON p.Id = m.PokemonId
    JOIN Treinadores t ON t.Id = p.TreinadorId
    WHERE t.Email LIKE '%@treinocenter.seed';
DELETE p FROM Pokemons p
    JOIN Treinadores t ON t.Id = p.TreinadorId
    WHERE t.Email LIKE '%@treinocenter.seed';
DELETE FROM Treinadores WHERE Email LIKE '%@treinocenter.seed';
GO

DECLARE @planoGinasio INT = (SELECT Id FROM PlanosTreinamento WHERE Nome = N'Ginásio Local');
DECLARE @planoLiga    INT = (SELECT Id FROM PlanosTreinamento WHERE Nome = N'Liga Regional');
DECLARE @planoElite   INT = (SELECT Id FROM PlanosTreinamento WHERE Nome = N'Elite dos 4');

-- ---------------------------------------------------------------------------
-- Treinadores + Pokémon
-- ---------------------------------------------------------------------------
DECLARE @tAsh INT, @tMisty INT, @tBrock INT, @tGary INT, @tMay INT,
        @tDawn INT, @tSerena INT, @tIris INT, @tCynthia INT, @tLance INT;
DECLARE @pCharizard INT, @pStarmie INT, @pOnix INT, @pBlastoise INT, @pBlaziken INT,
        @pEmpoleon INT, @pBraixen INT, @pDragonite INT, @pGarchomp INT, @pGyarados INT;

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Ash Ketchum', N'ash@treinocenter.seed', N'Pallet Town');
SET @tAsh = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Charizard', N'Fogo', 88, @tAsh);
SET @pCharizard = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Misty', N'misty@treinocenter.seed', N'Cerulean City');
SET @tMisty = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Starmie', N'Agua', 45, @tMisty);
SET @pStarmie = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Brock', N'brock@treinocenter.seed', N'Pewter City');
SET @tBrock = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Onix', N'Pedra', 40, @tBrock);
SET @pOnix = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Gary Oak', N'gary@treinocenter.seed', N'Pallet Town');
SET @tGary = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Blastoise', N'Agua', 82, @tGary);
SET @pBlastoise = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'May', N'may@treinocenter.seed', N'Petalburg City');
SET @tMay = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Blaziken', N'Fogo', 65, @tMay);
SET @pBlaziken = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Dawn', N'dawn@treinocenter.seed', N'Twinleaf Town');
SET @tDawn = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Empoleon', N'Agua', 58, @tDawn);
SET @pEmpoleon = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Serena', N'serena@treinocenter.seed', N'Vaniville Town');
SET @tSerena = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Braixen', N'Fogo', 32, @tSerena);
SET @pBraixen = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Iris', N'iris@treinocenter.seed', N'Opelucid City');
SET @tIris = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Dragonite', N'Dragao', 55, @tIris);
SET @pDragonite = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Cynthia', N'cynthia@treinocenter.seed', N'Celestic Town');
SET @tCynthia = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Garchomp', N'Dragao', 78, @tCynthia);
SET @pGarchomp = SCOPE_IDENTITY();

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Lance', N'lance@treinocenter.seed', N'Blackthorn City');
SET @tLance = SCOPE_IDENTITY();
INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Gyarados', N'Agua', 70, @tLance);
SET @pGyarados = SCOPE_IDENTITY();

-- ---------------------------------------------------------------------------
-- Matrículas — 6 Ativas, 2 Canceladas (R4) e 1 cadeia de upgrade (R2:
-- Concluída + Ativa ligadas via MatriculaOrigemId). Braixen fica de propósito
-- sem matrícula, como caso de teste pra criar uma nova.
-- ---------------------------------------------------------------------------
INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, Status, ValorMensal) VALUES
    (@pCharizard, @planoElite,   '2026-07-01', N'Ativa', 300.00),
    (@pStarmie,   @planoGinasio, '2026-07-05', N'Ativa', 50.00),
    (@pBlastoise, @planoElite,   '2026-07-10', N'Ativa', 300.00),
    (@pBlaziken,  @planoLiga,    '2026-07-12', N'Ativa', 120.00),
    (@pDragonite, @planoElite,   '2026-06-20', N'Ativa', 300.00),
    (@pGarchomp,  @planoElite,   '2026-07-15', N'Ativa', 300.00);

INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, DataFim, Status, ValorMensal) VALUES
    (@pOnix,     @planoGinasio, '2026-06-01', '2026-06-18', N'Cancelada', 50.00),
    (@pGyarados, @planoLiga,    '2026-06-10', '2026-06-25', N'Cancelada', 120.00);

DECLARE @matriculaEmpoleonOrigem INT;
INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, DataFim, Status, ValorMensal) VALUES
    (@pEmpoleon, @planoGinasio, '2026-06-01', '2026-06-16', N'Concluida', 50.00);
SET @matriculaEmpoleonOrigem = SCOPE_IDENTITY();

INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, Status, ValorMensal, MatriculaOrigemId) VALUES
    (@pEmpoleon, @planoLiga, '2026-06-16', N'Ativa', 120.00, @matriculaEmpoleonOrigem);
GO

PRINT 'Seed aplicado: 10 Treinadores, 10 Pokémon, 10 Matrículas.';
GO
