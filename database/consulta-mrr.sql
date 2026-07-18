-- ============================================================
-- MRR (Receita Mensal Recorrente) do Centro de Treinamento
-- Agrupado por Plano, com total geral ao final
-- Considera apenas matrículas com Status = 'Ativa'
-- ============================================================

SELECT
    Plano,
    COUNT(*)                   AS QtdMatriculas,
    SUM(ValorMensal)           AS MRR
FROM Matriculas
WHERE Status = 'Ativa'
GROUP BY Plano

UNION ALL

SELECT
    'TOTAL'                    AS Plano,
    COUNT(*)                   AS QtdMatriculas,
    SUM(ValorMensal)           AS MRR
FROM Matriculas
WHERE Status = 'Ativa'

ORDER BY
    CASE Plano
        WHEN 'GinasioLocal'  THEN 1
        WHEN 'LigaRegional'  THEN 2
        WHEN 'EliteDos4'     THEN 3
        WHEN 'TOTAL'         THEN 4
    END;
