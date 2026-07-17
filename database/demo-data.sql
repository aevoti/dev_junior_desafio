-- Optional demonstration data for local environments.
-- This script does not replace schema.sql and does not delete existing records.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM Treinadores WHERE Email = 'ash@example.com')
    INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Ash Ketchum', 'ash@example.com', N'Pallet');
IF NOT EXISTS (SELECT 1 FROM Treinadores WHERE Email = 'misty@example.com')
    INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Misty', 'misty@example.com', N'Cerulean');
IF NOT EXISTS (SELECT 1 FROM Treinadores WHERE Email = 'brock@example.com')
    INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Brock', 'brock@example.com', N'Pewter');
IF NOT EXISTS (SELECT 1 FROM Treinadores WHERE Email = 'cynthia@example.com')
    INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES (N'Cynthia', 'cynthia@example.com', N'Celestic');

DECLARE @AshId int = (SELECT Id FROM Treinadores WHERE Email = 'ash@example.com');
DECLARE @MistyId int = (SELECT Id FROM Treinadores WHERE Email = 'misty@example.com');
DECLARE @BrockId int = (SELECT Id FROM Treinadores WHERE Email = 'brock@example.com');
DECLARE @CynthiaId int = (SELECT Id FROM Treinadores WHERE Email = 'cynthia@example.com');

IF NOT EXISTS (SELECT 1 FROM Pokemons WHERE Nome = N'Pikachu' AND TreinadorId = @AshId)
    INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Pikachu', N'Elétrico', 60, @AshId);
IF NOT EXISTS (SELECT 1 FROM Pokemons WHERE Nome = N'Charmander' AND TreinadorId = @AshId)
    INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Charmander', N'Fogo', 49, @AshId);
IF NOT EXISTS (SELECT 1 FROM Pokemons WHERE Nome = N'Starmie' AND TreinadorId = @MistyId)
    INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Starmie', N'Água/Psíquico', 55, @MistyId);
IF NOT EXISTS (SELECT 1 FROM Pokemons WHERE Nome = N'Onix' AND TreinadorId = @BrockId)
    INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Onix', N'Pedra/Terrestre', 45, @BrockId);
IF NOT EXISTS (SELECT 1 FROM Pokemons WHERE Nome = N'Garchomp' AND TreinadorId = @CynthiaId)
    INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES (N'Garchomp', N'Dragão/Terrestre', 80, @CynthiaId);

DECLARE @PikachuId int = (SELECT Id FROM Pokemons WHERE Nome = N'Pikachu' AND TreinadorId = @AshId);
DECLARE @CharmanderId int = (SELECT Id FROM Pokemons WHERE Nome = N'Charmander' AND TreinadorId = @AshId);
DECLARE @StarmieId int = (SELECT Id FROM Pokemons WHERE Nome = N'Starmie' AND TreinadorId = @MistyId);
DECLARE @OnixId int = (SELECT Id FROM Pokemons WHERE Nome = N'Onix' AND TreinadorId = @BrockId);
DECLARE @GarchompId int = (SELECT Id FROM Pokemons WHERE Nome = N'Garchomp' AND TreinadorId = @CynthiaId);
DECLARE @LocalId int = (SELECT Id FROM PlanosTreinamento WHERE Ordem = 1);
DECLARE @RegionalId int = (SELECT Id FROM PlanosTreinamento WHERE Ordem = 2);
DECLARE @EliteId int = (SELECT Id FROM PlanosTreinamento WHERE Ordem = 3);
DECLARE @LocalValor decimal(10,2) = (SELECT ValorMensal FROM PlanosTreinamento WHERE Id = @LocalId);
DECLARE @RegionalValor decimal(10,2) = (SELECT ValorMensal FROM PlanosTreinamento WHERE Id = @RegionalId);
DECLARE @EliteValor decimal(10,2) = (SELECT ValorMensal FROM PlanosTreinamento WHERE Id = @EliteId);

IF NOT EXISTS (SELECT 1 FROM Matriculas WHERE PokemonId = @PikachuId AND Status = 'Ativa')
    INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, DataFim, Status, ValorMensal, MotivoEncerramento)
    VALUES (@PikachuId, @RegionalId, '2026-07-01', NULL, 'Ativa', @RegionalValor, NULL);
IF NOT EXISTS (SELECT 1 FROM Matriculas WHERE PokemonId = @CharmanderId AND Status = 'Ativa')
    INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, DataFim, Status, ValorMensal, MotivoEncerramento)
    VALUES (@CharmanderId, @LocalId, '2026-07-05', NULL, 'Ativa', @LocalValor, NULL);
IF NOT EXISTS (SELECT 1 FROM Matriculas WHERE PokemonId = @StarmieId AND Status = 'Ativa')
    INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, DataFim, Status, ValorMensal, MotivoEncerramento)
    VALUES (@StarmieId, @EliteId, '2026-07-03', NULL, 'Ativa', @EliteValor, NULL);
IF NOT EXISTS (SELECT 1 FROM Matriculas WHERE PokemonId = @OnixId AND Status = 'Cancelada')
    INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, DataFim, Status, ValorMensal, MotivoEncerramento)
    VALUES (@OnixId, @LocalId, '2026-06-01', '2026-06-20', 'Cancelada', @LocalValor, N'Treinamento encerrado pelo treinador');
IF NOT EXISTS (SELECT 1 FROM Matriculas WHERE PokemonId = @GarchompId AND Status = 'Concluida')
    INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, DataFim, Status, ValorMensal, MotivoEncerramento)
    VALUES (@GarchompId, @RegionalId, '2026-06-01', '2026-07-01', 'Concluida', @RegionalValor, N'Upgrade para Elite dos 4');
IF NOT EXISTS (SELECT 1 FROM Matriculas WHERE PokemonId = @GarchompId AND Status = 'Ativa')
    INSERT INTO Matriculas (PokemonId, PlanoTreinamentoId, DataInicio, DataFim, Status, ValorMensal, MotivoEncerramento)
    VALUES (@GarchompId, @EliteId, '2026-07-01', NULL, 'Ativa', @EliteValor, NULL);

COMMIT TRANSACTION;
