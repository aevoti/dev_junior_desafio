-- =========================================
-- Dados de teste para facilitar execução local
-- (Planos já são inseridos pelo schema.sql / migration do EF)
-- =========================================

INSERT INTO Treinadores (Nome, Email, CidadeOrigem) VALUES
    ('Ash Ketchum', 'ash@pallettown.com', 'Pallet Town'),
    ('Misty', 'misty@cerulean.com', 'Cerulean City'),
    ('Brock', 'brock@pewter.com', 'Pewter City');

INSERT INTO Pokemons (Nome, Tipo, Nivel, TreinadorId) VALUES
    ('Charizard', 'Fogo', 55, 1),
    ('Pikachu', 'Eletrico', 30, 1),
    ('Starmie', 'Agua', 45, 2),
    ('Onix', 'Terra', 52, 3);

INSERT INTO Matriculas (PokemonId, PlanoId, DataInicio, Status, ValorMensal) VALUES
    (1, 1, DATEADD(DAY, -15, CAST(GETDATE() AS DATE)), 'Ativa', 50.00);

INSERT INTO Matriculas (PokemonId, PlanoId, DataInicio, Status, ValorMensal) VALUES
    (2, 2, CAST(GETDATE() AS DATE), 'Ativa', 120.00);

INSERT INTO Matriculas (PokemonId, PlanoId, DataInicio, Status, ValorMensal) VALUES
    (3, 1, DATEADD(DAY, -40, CAST(GETDATE() AS DATE)), 'Cancelada', 50.00);

INSERT INTO Matriculas (PokemonId, PlanoId, DataInicio, Status, ValorMensal) VALUES
    (4, 3, CAST(GETDATE() AS DATE), 'Ativa', 300.00);