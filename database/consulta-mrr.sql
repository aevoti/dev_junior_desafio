SELECT
    CASE
        WHEN GROUPING(plano.Nome) = 1
            THEN 'TOTAL GERAL'
        ELSE plano.Nome
    END AS Plano,

    SUM(matricula.ValorMensal) AS MRR

FROM dbo.Matricula matricula

INNER JOIN dbo.PlanoTreinamento plano
    ON plano.Id = matricula.PlanoTreinamentoId

WHERE matricula.Status = 1

GROUP BY ROLLUP(plano.Nome)

ORDER BY
    GROUPING(plano.Nome),
    plano.Nome;