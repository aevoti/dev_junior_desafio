DECLARE @TreinadorId INT;

INSERT INTO Treinador
(
    Nome,
    Email,
    CidadeOrigem
)
VALUES
(
    'Ash Ketchum',
    'ash@pokemon.com',
    'Pallet'
);

SET @TreinadorId = SCOPE_IDENTITY();

INSERT INTO Pokemon
(
    Nome,
    Tipo,
    Nivel,
    TreinadorId
)
VALUES
(
    'Pikachu',
    4,
    35,
    @TreinadorId
);
GO

INSERT INTO PlanoTreinamento
(
    Nome,
    ValorMensal,
    Descricao,
    NivelPlano
)
VALUES
(
    'Ginásio Local',
    50.00,
    'Treinos básicos',
    1
),
(
    'Liga Regional',
    120.00,
    'Treinos intermediários + batalhas simuladas',
    2
),
(
    'Elite dos 4',
    300.00,
    'Preparação completa para a Liga',
    3
);
GO