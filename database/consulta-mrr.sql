SELECT
    CASE
        WHEN GROUPING(p.Nome) = 1 THEN N'TOTAL GERAL'
        ELSE p.Nome
    END AS Plano,
    COUNT(m.Id) AS QuantidadeMatriculas,
    CAST(COALESCE(SUM(m.ValorMensal), 0) AS DECIMAL(10,2)) AS MRR
FROM dbo.Matriculas AS m
INNER JOIN dbo.PlanosTreinamento AS p
    ON p.Id = m.PlanoTreinamentoId
WHERE m.Status = 'Ativa'
GROUP BY ROLLUP(p.Nome)
ORDER BY
    CASE WHEN GROUPING(p.Nome) = 1 THEN 1 ELSE 0 END,
    p.Nome;
