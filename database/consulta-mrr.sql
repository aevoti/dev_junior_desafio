-- =========================================
-- Receita Mensal Recorrente (MRR) do Centro
-- Agrupada por plano, considerando apenas
-- matrículas ATIVAS (R4), com total geral ao final.
-- =========================================

SELECT
    p.Nome AS Plano,
    COUNT(m.Id) AS QuantidadeMatriculasAtivas,
    SUM(m.ValorMensal) AS ReceitaMensal
FROM Matriculas m
INNER JOIN Planos p ON p.Id = m.PlanoId
WHERE m.Status = 'Ativa'
GROUP BY p.Nome

UNION ALL

SELECT
    'TOTAL GERAL' AS Plano,
    COUNT(m.Id) AS QuantidadeMatriculasAtivas,
    SUM(m.ValorMensal) AS ReceitaMensal
FROM Matriculas m
WHERE m.Status = 'Ativa';