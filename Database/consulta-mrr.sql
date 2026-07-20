-- Receita Mensal Recorrente (MRR) do Centro, agrupada por plano,
-- considerando apenas matrículas ativas, com uma linha de total geral ao final.
--
-- MRR de um plano = quantidade de matrículas Ativas nesse plano * valorMensal do plano.
-- (usa o valorMensal cheio de cada plano, não o valorPrimeiraCobranca, que é só a cobrança
-- pro-rata de entrada em caso de upgrade — a receita recorrente do ciclo seguinte em diante
-- é sempre o valor cheio do plano.)
--
-- LEFT JOIN em vez de INNER na primeira consulta para que planos sem nenhuma matrícula
-- ativa no momento apareçam com MRR R$ 0,00, em vez de sumirem do relatório.
--
-- A linha de total geral é somada a partir do próprio agregado por plano (mrrPorPlano),
-- e não de uma segunda varredura de Matricula, para que o total nunca possa divergir
-- das linhas exibidas. Isso exige CTE: derived table não pode ser referenciada duas vezes.
--
-- O SELECT final existe porque o SQL Server não aceita expressão no ORDER BY de uma query
-- com UNION ALL; envolver o resultado é o que permite ordenar pelo CASE que joga o total
-- geral para a última linha.

USE PokemonCenter;
GO

WITH mrrPorPlano AS (
    SELECT
        pl.nome                                                AS plano,
        COUNT(m.id)                                            AS matriculasAtivas,
        CAST(COUNT(m.id) * pl.valorMensal AS DECIMAL(10, 2))   AS mrr
    FROM Planos pl
    LEFT JOIN Matricula m
        ON m.planoId = pl.id
       AND m.status = 'Ativa'
    GROUP BY pl.id, pl.nome, pl.valorMensal
),
resultado AS (
    SELECT plano, matriculasAtivas, mrr
    FROM mrrPorPlano

    UNION ALL

    SELECT
        'Total Geral'          AS plano,
        SUM(matriculasAtivas)  AS matriculasAtivas,
        SUM(mrr)               AS mrr
    FROM mrrPorPlano
)
SELECT plano, matriculasAtivas, mrr
FROM resultado
ORDER BY CASE WHEN plano = 'Total Geral' THEN 1 ELSE 0 END, plano;
GO
