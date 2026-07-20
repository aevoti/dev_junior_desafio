-- Receita Mensal Recorrente (MRR) do Centro de Treinamento, agrupada por
-- plano, considerando apenas matrículas ativas (EndDate IS NULL ou
-- EndDate >= hoje — mesma definição de "ativa" usada em toda a aplicação,
-- FR-019/FR-020), com uma linha de Total Geral ao final.
--
-- Regras aplicadas (ver spec.md, Clarifications e Assumptions):
-- - Planos sem nenhuma matrícula ativa NÃO geram linha própria (INNER JOIN).
-- - A linha de Total Geral sempre aparece, mesmo com R$ 0,00, mesmo que não
--   haja nenhuma matrícula ativa em nenhum plano (o resultado nunca é vazio).
-- SQL puro, sem ORM (constituição do projeto — Stack Técnica).

SELECT
    tp.[Name]                  AS Plano,
    SUM(e.[MonthlyPrice])      AS MRR,
    0                          AS OrdemClassificacao
FROM [TrainingPlans] tp
INNER JOIN [Enrollments] e
    ON e.[TrainingPlanId] = tp.[Id]
    AND (e.[EndDate] IS NULL OR e.[EndDate] >= CAST(GETDATE() AS date))
GROUP BY tp.[Name]

UNION ALL

SELECT
    'Total Geral'                          AS Plano,
    COALESCE(SUM(e.[MonthlyPrice]), 0)     AS MRR,
    1                                      AS OrdemClassificacao
FROM [Enrollments] e
WHERE e.[EndDate] IS NULL OR e.[EndDate] >= CAST(GETDATE() AS date)

ORDER BY OrdemClassificacao, Plano;
