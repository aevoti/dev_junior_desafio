INSERT INTO Treinadores
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
GO

INSERT INTO Pokemons
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
    1
);
GO


INSERT INTO PlanosTreinamento
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