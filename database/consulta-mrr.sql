SELECT 
    COALESCE(training_plan, 'TOTAL GERAL') AS plano_treinamento,
    COUNT(id) AS qtd_matriculas_ativas,
    SUM(monthly_value) AS mrr_total
FROM 
    Registration
WHERE 
    status = 'Ativa'
GROUP BY 
    ROLLUP(training_plan);