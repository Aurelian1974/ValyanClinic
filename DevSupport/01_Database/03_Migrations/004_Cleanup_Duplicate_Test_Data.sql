/*
==============================================================================
CLEANUP: Remove duplicate test data from normalized structure
Author: AI Agent
Date: 2026-01-02
Description: Clean up duplicate consultations and test data from all tables
==============================================================================
*/

USE [ValyanMed]
GO

PRINT '========================================='
PRINT 'Starting cleanup of duplicate test data'
PRINT '========================================='

BEGIN TRANSACTION;

BEGIN TRY
    -- Show what will be deleted
    PRINT ''
    PRINT 'Records to be deleted:'
    PRINT '----------------------'
    
    SELECT 
        'Consultatii' AS TableName,
        COUNT(*) AS RecordCount
    FROM Consultatii
    WHERE Status = 'In desfasurare'
    
    UNION ALL
    
    SELECT 
        'ConsultatieMotivePrezentare' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieMotivePrezentare
    
    UNION ALL
    
    SELECT 
        'ConsultatieAntecedente' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieAntecedente
    
    UNION ALL
    
    SELECT 
        'ConsultatieExamenObiectiv' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieExamenObiectiv
    
    UNION ALL
    
    SELECT 
        'ConsultatieInvestigatii' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieInvestigatii
    
    UNION ALL
    
    SELECT 
        'ConsultatieDiagnostic' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieDiagnostic
    
    UNION ALL
    
    SELECT 
        'ConsultatieTratament' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieTratament
    
    UNION ALL
    
    SELECT 
        'ConsultatieConcluzii' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieConcluzii;
    
    PRINT ''
    PRINT 'Deleting data...'
    PRINT ''
    
    -- Delete from child tables first (to avoid FK violations)
    DELETE FROM ConsultatieAnalizaDetalii;
    PRINT '✓ ConsultatieAnalizaDetalii: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    DELETE FROM ConsultatieAnalizeMedicale;
    PRINT '✓ ConsultatieAnalizeMedicale: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    DELETE FROM ConsultatieConcluzii;
    PRINT '✓ ConsultatieConcluzii: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    DELETE FROM ConsultatieTratament;
    PRINT '✓ ConsultatieTratament: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    DELETE FROM ConsultatieDiagnostic;
    PRINT '✓ ConsultatieDiagnostic: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    DELETE FROM ConsultatieInvestigatii;
    PRINT '✓ ConsultatieInvestigatii: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    DELETE FROM ConsultatieExamenObiectiv;
    PRINT '✓ ConsultatieExamenObiectiv: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    DELETE FROM ConsultatieAntecedente;
    PRINT '✓ ConsultatieAntecedente: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    DELETE FROM ConsultatieMotivePrezentare;
    PRINT '✓ ConsultatieMotivePrezentare: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    -- Delete from master table last
    DELETE FROM Consultatii WHERE Status = 'In desfasurare';
    PRINT '✓ Consultatii (In desfasurare): ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    -- Optionally delete ALL test data (uncomment if needed)
    -- DELETE FROM Consultatii;
    -- PRINT '✓ Consultatii (ALL): ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows deleted';
    
    PRINT ''
    PRINT '========================================='
    PRINT 'Cleanup completed successfully!'
    PRINT '========================================='
    
    -- Show final counts
    PRINT ''
    PRINT 'Final record counts:'
    PRINT '--------------------'
    
    SELECT 
        'Consultatii' AS TableName,
        COUNT(*) AS RecordCount
    FROM Consultatii
    
    UNION ALL
    
    SELECT 
        'ConsultatieMotivePrezentare' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieMotivePrezentare
    
    UNION ALL
    
    SELECT 
        'ConsultatieAntecedente' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieAntecedente
    
    UNION ALL
    
    SELECT 
        'ConsultatieExamenObiectiv' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieExamenObiectiv
    
    UNION ALL
    
    SELECT 
        'ConsultatieInvestigatii' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieInvestigatii
    
    UNION ALL
    
    SELECT 
        'ConsultatieDiagnostic' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieDiagnostic
    
    UNION ALL
    
    SELECT 
        'ConsultatieTratament' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieTratament
    
    UNION ALL
    
    SELECT 
        'ConsultatieConcluzii' AS TableName,
        COUNT(*) AS RecordCount
    FROM ConsultatieConcluzii;
    
    COMMIT TRANSACTION;
    PRINT ''
    PRINT '✓✓✓ Transaction committed successfully!'
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT ''
    PRINT '❌ ERROR occurred - transaction rolled back!'
    PRINT 'Error: ' + ERROR_MESSAGE();
    
    THROW;
END CATCH

GO
