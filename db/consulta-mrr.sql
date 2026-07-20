-- =============================================================================
-- MRR (Receita Mensal Recorrente) do Centro, agrupada por plano,
-- considerando apenas matrículas Ativas, com uma linha de total geral ao final.
-- =============================================================================
USE PokemonTrainingCenter;
GO

SELECT
    COALESCE(p.Nome, N'TOTAL GERAL')   AS Plano,
    COUNT(m.Id)                        AS QuantidadeMatriculasAtivas,
    SUM(m.ValorMensal)                 AS MRR,
    CASE WHEN GROUPING(p.Nome) = 1 THEN 1 ELSE 0 END AS EhTotalGeral
FROM Matriculas m
JOIN PlanosTreinamento p ON p.Id = m.PlanoTreinamentoId
WHERE m.Status = N'Ativa'
GROUP BY GROUPING SETS ((p.Nome), ())
ORDER BY EhTotalGeral, Plano;
GO
