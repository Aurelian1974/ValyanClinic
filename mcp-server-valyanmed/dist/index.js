#!/usr/bin/env node
import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { CallToolRequestSchema, ListToolsRequestSchema, } from '@modelcontextprotocol/sdk/types.js';
import { executeQuery } from './database.js';
// ============================================================================
// Tool Definitions
// ============================================================================
const tools = [
    // ── LIST / EXPLORE ──────────────────────────────────────────────────
    {
        name: 'list_objects',
        description: 'Listează obiectele din baza de date ValyanMed. Tipuri: TABLE, VIEW, PROCEDURE, FUNCTION, TRIGGER, INDEX, SCHEMA, TYPE, SYNONYM. Poate filtra după schema și pattern de nume.',
        inputSchema: {
            type: 'object',
            properties: {
                objectType: {
                    type: 'string',
                    enum: [
                        'TABLE',
                        'VIEW',
                        'PROCEDURE',
                        'FUNCTION',
                        'TRIGGER',
                        'INDEX',
                        'SCHEMA',
                        'TYPE',
                        'SYNONYM',
                        'ALL',
                    ],
                    description: 'Tipul de obiect de listat',
                },
                schemaFilter: {
                    type: 'string',
                    description: 'Filtrare opțională după schema (ex: dbo, med). Default: toate schemele.',
                },
                namePattern: {
                    type: 'string',
                    description: 'Pattern opțional pentru filtrare după nume (LIKE pattern, ex: %Pacient%)',
                },
            },
            required: ['objectType'],
        },
    },
    {
        name: 'get_object_definition',
        description: 'Obține definiția/DDL-ul complet al unui obiect din baza de date (stored procedure, view, function, trigger). Pentru tabele, returnează structura completă cu coloane, tipuri, constrainte, indecși.',
        inputSchema: {
            type: 'object',
            properties: {
                objectName: {
                    type: 'string',
                    description: 'Numele obiectului (cu sau fără schema, ex: dbo.Pacient sau Pacient)',
                },
                objectType: {
                    type: 'string',
                    enum: ['TABLE', 'VIEW', 'PROCEDURE', 'FUNCTION', 'TRIGGER', 'AUTO'],
                    description: 'Tipul obiectului. Folosește AUTO pentru detectare automată.',
                    default: 'AUTO',
                },
            },
            required: ['objectName'],
        },
    },
    {
        name: 'get_table_details',
        description: 'Obține detalii complete despre un tabel: coloane, tipuri de date, constrainte (PK, FK, UNIQUE, CHECK, DEFAULT), indecși, și relații.',
        inputSchema: {
            type: 'object',
            properties: {
                tableName: {
                    type: 'string',
                    description: 'Numele tabelului (cu sau fără schema)',
                },
            },
            required: ['tableName'],
        },
    },
    {
        name: 'get_dependencies',
        description: 'Obține dependențele unui obiect - ce obiecte depind de el și de ce obiecte depinde.',
        inputSchema: {
            type: 'object',
            properties: {
                objectName: {
                    type: 'string',
                    description: 'Numele obiectului',
                },
            },
            required: ['objectName'],
        },
    },
    // ── CREATE ──────────────────────────────────────────────────────────
    {
        name: 'create_object',
        description: 'Creează un obiect nou în baza de date ValyanMed. Execută instrucțiuni CREATE (TABLE, VIEW, PROCEDURE, FUNCTION, TRIGGER, INDEX, TYPE, SCHEMA). Validează sintaxa înainte de execuție.',
        inputSchema: {
            type: 'object',
            properties: {
                sql: {
                    type: 'string',
                    description: 'Instrucțiunea SQL CREATE completă. Exemplu: CREATE TABLE dbo.TestTable (Id INT PRIMARY KEY, Name NVARCHAR(100))',
                },
                description: {
                    type: 'string',
                    description: 'Descriere opțională a obiectului creat (pentru audit)',
                },
            },
            required: ['sql'],
        },
    },
    // ── MODIFY ──────────────────────────────────────────────────────────
    {
        name: 'modify_object',
        description: 'Modifică un obiect existent în baza de date ValyanMed. Execută instrucțiuni ALTER (TABLE, VIEW, PROCEDURE, FUNCTION, TRIGGER). Pentru stored procedures și funcții, poate folosi CREATE OR ALTER.',
        inputSchema: {
            type: 'object',
            properties: {
                sql: {
                    type: 'string',
                    description: 'Instrucțiunea SQL ALTER sau CREATE OR ALTER completă.',
                },
                description: {
                    type: 'string',
                    description: 'Descriere opțională a modificării (pentru audit)',
                },
            },
            required: ['sql'],
        },
    },
    // ── OPTIMIZE ────────────────────────────────────────────────────────
    {
        name: 'analyze_index_fragmentation',
        description: 'Analizează fragmentarea indecșilor pentru un tabel sau toată baza de date. Returnează procentul de fragmentare și recomandări (REORGANIZE < 30%, REBUILD > 30%).',
        inputSchema: {
            type: 'object',
            properties: {
                tableName: {
                    type: 'string',
                    description: 'Numele tabelului de analizat. Lasă gol pentru toată baza de date.',
                },
                minFragmentation: {
                    type: 'number',
                    description: 'Fragmentare minimă % pentru filtrare (default: 5). Indecșii cu fragmentare sub acest prag nu sunt afișați.',
                    default: 5,
                },
            },
        },
    },
    {
        name: 'optimize_index',
        description: 'Optimizează un index specific prin REORGANIZE (fragmentare < 30%) sau REBUILD (fragmentare > 30%). Poate optimiza și toate indecșii unui tabel.',
        inputSchema: {
            type: 'object',
            properties: {
                tableName: {
                    type: 'string',
                    description: 'Numele tabelului',
                },
                indexName: {
                    type: 'string',
                    description: 'Numele indexului. Lasă gol pentru a optimiza toți indecșii tabelului.',
                },
                action: {
                    type: 'string',
                    enum: ['REORGANIZE', 'REBUILD', 'AUTO'],
                    description: 'Acțiunea de optimizare. AUTO alege automat pe baza fragmentării.',
                    default: 'AUTO',
                },
            },
            required: ['tableName'],
        },
    },
    {
        name: 'update_statistics',
        description: 'Actualizează statisticile pentru un tabel sau toată baza de date. Statisticile actuale ajută query optimizer-ul să aleagă planuri de execuție optime.',
        inputSchema: {
            type: 'object',
            properties: {
                tableName: {
                    type: 'string',
                    description: 'Numele tabelului. Lasă gol pentru a actualiza toate statisticile.',
                },
                fullScan: {
                    type: 'boolean',
                    description: 'Dacă să folosească FULLSCAN (mai precis dar mai lent). Default: false.',
                    default: false,
                },
            },
        },
    },
    {
        name: 'analyze_missing_indexes',
        description: 'Analizează indecșii lipsă sugerați de SQL Server pe baza query-urilor executate. Returnează recomandări cu impact estimat.',
        inputSchema: {
            type: 'object',
            properties: {
                tableName: {
                    type: 'string',
                    description: 'Filtrare opțională pentru un tabel specific.',
                },
                minImpact: {
                    type: 'number',
                    description: 'Impactul minim al indexului (default: 100). Valori mai mari = recomandări mai importante.',
                    default: 100,
                },
            },
        },
    },
    {
        name: 'analyze_query_plan',
        description: 'Obține planul de execuție estimat (XML) pentru o interogare SQL, util pentru optimizare.',
        inputSchema: {
            type: 'object',
            properties: {
                sql: {
                    type: 'string',
                    description: 'Interogarea SQL de analizat',
                },
            },
            required: ['sql'],
        },
    },
    // ── DELETE (cu confirmare) ──────────────────────────────────────────
    {
        name: 'prepare_delete',
        description: 'PASUL 1 pentru ștergere: Analizează ce se va șterge, verifică dependențele și returnează un raport complet. NU șterge nimic - doar pregătește raportul. Utilizatorul trebuie să confirme apoi cu execute_delete.',
        inputSchema: {
            type: 'object',
            properties: {
                objectName: {
                    type: 'string',
                    description: 'Numele obiectului de șters (cu sau fără schema)',
                },
                objectType: {
                    type: 'string',
                    enum: [
                        'TABLE',
                        'VIEW',
                        'PROCEDURE',
                        'FUNCTION',
                        'TRIGGER',
                        'INDEX',
                        'TYPE',
                        'SCHEMA',
                        'AUTO',
                    ],
                    description: 'Tipul obiectului. Folosește AUTO pentru detectare automată.',
                    default: 'AUTO',
                },
            },
            required: ['objectName'],
        },
    },
    {
        name: 'execute_delete',
        description: 'PASUL 2 pentru ștergere: Execută ștergerea obiectului DOAR după ce utilizatorul a confirmat. ATENȚIE: Această acțiune este IREVERSIBILĂ. Trebuie apelat prepare_delete mai întâi pentru a vedea dependențele.',
        inputSchema: {
            type: 'object',
            properties: {
                objectName: {
                    type: 'string',
                    description: 'Numele obiectului de șters',
                },
                objectType: {
                    type: 'string',
                    enum: [
                        'TABLE',
                        'VIEW',
                        'PROCEDURE',
                        'FUNCTION',
                        'TRIGGER',
                        'INDEX',
                        'TYPE',
                        'SCHEMA',
                    ],
                    description: 'Tipul obiectului de șters',
                },
                parentTable: {
                    type: 'string',
                    description: 'Pentru INDEX și TRIGGER - numele tabelului părinte',
                },
                confirmed: {
                    type: 'boolean',
                    description: 'Trebuie să fie TRUE pentru a executa ștergerea. Setează pe TRUE doar dacă utilizatorul a confirmat explicit.',
                },
            },
            required: ['objectName', 'objectType', 'confirmed'],
        },
    },
    // ── UTILITY ─────────────────────────────────────────────────────────
    {
        name: 'run_sql',
        description: 'Execută o interogare SQL arbitrară (SELECT, EXEC, etc.) pe baza de date ValyanMed. Pentru operații DDL (CREATE, ALTER, DROP), folosește tool-urile specifice.',
        inputSchema: {
            type: 'object',
            properties: {
                sql: {
                    type: 'string',
                    description: 'Interogarea SQL de executat',
                },
            },
            required: ['sql'],
        },
    },
    {
        name: 'get_database_info',
        description: 'Obține informații generale despre baza de date ValyanMed: dimensiune, număr obiecte, recovery model, compatibilitate, etc.',
        inputSchema: {
            type: 'object',
            properties: {},
        },
    },
];
// ============================================================================
// Tool Handlers
// ============================================================================
async function handleListObjects(args) {
    const { objectType, schemaFilter, namePattern } = args;
    let query = '';
    switch (objectType) {
        case 'TABLE':
            query = `
        SELECT s.name AS [Schema], t.name AS [Name], 
               t.create_date AS [Created], t.modify_date AS [Modified],
               p.rows AS [RowCount]
        FROM sys.tables t
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        LEFT JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0,1)
        WHERE t.is_ms_shipped = 0
        ${schemaFilter ? `AND s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND t.name LIKE '${namePattern}'` : ''}
        ORDER BY s.name, t.name`;
            break;
        case 'VIEW':
            query = `
        SELECT s.name AS [Schema], v.name AS [Name],
               v.create_date AS [Created], v.modify_date AS [Modified]
        FROM sys.views v
        JOIN sys.schemas s ON v.schema_id = s.schema_id
        WHERE v.is_ms_shipped = 0
        ${schemaFilter ? `AND s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND v.name LIKE '${namePattern}'` : ''}
        ORDER BY s.name, v.name`;
            break;
        case 'PROCEDURE':
            query = `
        SELECT s.name AS [Schema], p.name AS [Name],
               p.create_date AS [Created], p.modify_date AS [Modified]
        FROM sys.procedures p
        JOIN sys.schemas s ON p.schema_id = s.schema_id
        WHERE p.is_ms_shipped = 0
        ${schemaFilter ? `AND s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND p.name LIKE '${namePattern}'` : ''}
        ORDER BY s.name, p.name`;
            break;
        case 'FUNCTION':
            query = `
        SELECT s.name AS [Schema], o.name AS [Name], o.type_desc AS [Type],
               o.create_date AS [Created], o.modify_date AS [Modified]
        FROM sys.objects o
        JOIN sys.schemas s ON o.schema_id = s.schema_id
        WHERE o.type IN ('FN','IF','TF','AF')
        ${schemaFilter ? `AND s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND o.name LIKE '${namePattern}'` : ''}
        ORDER BY s.name, o.name`;
            break;
        case 'TRIGGER':
            query = `
        SELECT s.name AS [Schema], t.name AS [TriggerName], 
               OBJECT_NAME(t.parent_id) AS [ParentTable],
               t.create_date AS [Created], t.modify_date AS [Modified],
               CASE WHEN t.is_disabled = 1 THEN 'Disabled' ELSE 'Enabled' END AS [Status]
        FROM sys.triggers t
        JOIN sys.objects o ON t.parent_id = o.object_id
        JOIN sys.schemas s ON o.schema_id = s.schema_id
        WHERE t.is_ms_shipped = 0
        ${schemaFilter ? `AND s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND t.name LIKE '${namePattern}'` : ''}
        ORDER BY s.name, t.name`;
            break;
        case 'INDEX':
            query = `
        SELECT s.name AS [Schema], t.name AS [Table], i.name AS [IndexName],
               i.type_desc AS [Type], i.is_unique AS [IsUnique], i.is_primary_key AS [IsPK],
               STUFF((SELECT ', ' + c.name 
                      FROM sys.index_columns ic 
                      JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                      WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
                      ORDER BY ic.key_ordinal
                      FOR XML PATH('')), 1, 2, '') AS [KeyColumns]
        FROM sys.indexes i
        JOIN sys.tables t ON i.object_id = t.object_id
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE t.is_ms_shipped = 0 AND i.name IS NOT NULL
        ${schemaFilter ? `AND s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND (i.name LIKE '${namePattern}' OR t.name LIKE '${namePattern}')` : ''}
        ORDER BY s.name, t.name, i.name`;
            break;
        case 'SCHEMA':
            query = `
        SELECT s.name AS [Schema], s.schema_id AS [SchemaId],
               dp.name AS [Owner]
        FROM sys.schemas s
        JOIN sys.database_principals dp ON s.principal_id = dp.principal_id
        WHERE s.schema_id < 16384
        ORDER BY s.name`;
            break;
        case 'TYPE':
            query = `
        SELECT s.name AS [Schema], t.name AS [TypeName], t.system_type_id,
               TYPE_NAME(t.system_type_id) AS [BaseType], t.is_table_type AS [IsTableType]
        FROM sys.types t
        JOIN sys.schemas s ON t.schema_id = s.schema_id
        WHERE t.is_user_defined = 1
        ${schemaFilter ? `AND s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND t.name LIKE '${namePattern}'` : ''}
        ORDER BY s.name, t.name`;
            break;
        case 'SYNONYM':
            query = `
        SELECT s.name AS [Schema], syn.name AS [Name], syn.base_object_name AS [Target]
        FROM sys.synonyms syn
        JOIN sys.schemas s ON syn.schema_id = s.schema_id
        ${schemaFilter ? `WHERE s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND syn.name LIKE '${namePattern}'` : ''}
        ORDER BY s.name, syn.name`;
            break;
        case 'ALL':
            query = `
        SELECT s.name AS [Schema], o.name AS [Name], o.type_desc AS [Type],
               o.create_date AS [Created], o.modify_date AS [Modified]
        FROM sys.objects o
        JOIN sys.schemas s ON o.schema_id = s.schema_id
        WHERE o.is_ms_shipped = 0 AND o.type IN ('U','V','P','FN','IF','TF','TR','SN')
        ${schemaFilter ? `AND s.name = '${schemaFilter}'` : ''}
        ${namePattern ? `AND o.name LIKE '${namePattern}'` : ''}
        ORDER BY o.type_desc, s.name, o.name`;
            break;
    }
    const result = await executeQuery(query);
    if (result.recordset.length === 0) {
        return `Nu s-au găsit obiecte de tip ${objectType}${schemaFilter ? ` în schema '${schemaFilter}'` : ''}${namePattern ? ` care se potrivesc cu '${namePattern}'` : ''}.`;
    }
    return formatResultAsTable(result.recordset);
}
async function handleGetObjectDefinition(args) {
    const { objectName } = args;
    let { objectType } = args;
    // Auto-detect type
    if (!objectType || objectType === 'AUTO') {
        const detectQuery = `
      SELECT TOP 1 type_desc 
      FROM sys.objects 
      WHERE name = '${objectName.includes('.') ? objectName.split('.').pop() : objectName}'
      OR OBJECT_ID('${objectName}') = object_id`;
        const detected = await executeQuery(detectQuery);
        if (detected.recordset.length === 0) {
            return `Obiectul '${objectName}' nu a fost găsit în baza de date.`;
        }
        const typeDesc = detected.recordset[0].type_desc;
        if (typeDesc.includes('TABLE'))
            objectType = 'TABLE';
        else if (typeDesc.includes('VIEW'))
            objectType = 'VIEW';
        else if (typeDesc.includes('PROCEDURE'))
            objectType = 'PROCEDURE';
        else if (typeDesc.includes('FUNCTION'))
            objectType = 'FUNCTION';
        else if (typeDesc.includes('TRIGGER'))
            objectType = 'TRIGGER';
        else
            objectType = 'TABLE';
    }
    if (objectType === 'TABLE') {
        return await getTableDefinition(objectName);
    }
    // For views, procedures, functions, triggers - use sp_helptext or OBJECT_DEFINITION
    const defQuery = `SELECT OBJECT_DEFINITION(OBJECT_ID('${objectName}')) AS [Definition]`;
    const result = await executeQuery(defQuery);
    if (result.recordset[0]?.Definition) {
        return `-- Definiția obiectului: ${objectName}\n${result.recordset[0].Definition}`;
    }
    // Fallback: sp_helptext
    try {
        const helpResult = await executeQuery(`EXEC sp_helptext '${objectName}'`);
        return helpResult.recordset.map((r) => r.Text).join('');
    }
    catch {
        return `Nu s-a putut obține definiția pentru '${objectName}'.`;
    }
}
async function getTableDefinition(tableName) {
    const colQuery = `
    SELECT c.name AS [Column], TYPE_NAME(c.user_type_id) AS [Type],
           c.max_length AS [MaxLength], c.precision AS [Precision], c.scale AS [Scale],
           c.is_nullable AS [Nullable], c.is_identity AS [Identity],
           dc.definition AS [Default],
           cc.definition AS [Computed]
    FROM sys.columns c
    LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
    LEFT JOIN sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id
    WHERE c.object_id = OBJECT_ID('${tableName}')
    ORDER BY c.column_id`;
    const result = await executeQuery(colQuery);
    if (result.recordset.length === 0) {
        return `Tabelul '${tableName}' nu a fost găsit.`;
    }
    let output = `-- Structura tabelului: ${tableName}\n`;
    output += formatResultAsTable(result.recordset);
    return output;
}
async function handleGetTableDetails(args) {
    const { tableName } = args;
    let output = '';
    // Columns
    const colResult = await executeQuery(`
    SELECT c.column_id AS [#], c.name AS [Column], TYPE_NAME(c.user_type_id) AS [Type],
           CASE WHEN TYPE_NAME(c.user_type_id) IN ('nvarchar','varchar','nchar','char','varbinary') 
                THEN CASE c.max_length WHEN -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR) END
                WHEN TYPE_NAME(c.user_type_id) IN ('decimal','numeric') 
                THEN CAST(c.precision AS VARCHAR) + ',' + CAST(c.scale AS VARCHAR)
                ELSE '' END AS [Size],
           CASE c.is_nullable WHEN 1 THEN 'YES' ELSE 'NO' END AS [Nullable],
           CASE c.is_identity WHEN 1 THEN 'YES' ELSE '' END AS [Identity],
           dc.definition AS [Default],
           cc.definition AS [Computed]
    FROM sys.columns c
    LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
    LEFT JOIN sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id
    WHERE c.object_id = OBJECT_ID('${tableName}')
    ORDER BY c.column_id`);
    output += `═══ COLOANE ═══\n`;
    output += formatResultAsTable(colResult.recordset);
    // Constraints
    const constraintResult = await executeQuery(`
    SELECT kc.name AS [ConstraintName], kc.type_desc AS [Type],
           STUFF((SELECT ', ' + c.name FROM sys.index_columns ic 
                  JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                  WHERE ic.object_id = kc.parent_object_id AND ic.index_id = 
                    CASE WHEN kc.type = 'PK' THEN (SELECT index_id FROM sys.indexes WHERE object_id = kc.parent_object_id AND is_primary_key = 1)
                         WHEN kc.type = 'UQ' THEN (SELECT index_id FROM sys.indexes WHERE object_id = kc.parent_object_id AND name = kc.name)
                    END
                  FOR XML PATH('')), 1, 2, '') AS [Columns]
    FROM sys.key_constraints kc
    WHERE kc.parent_object_id = OBJECT_ID('${tableName}')
    UNION ALL
    SELECT fk.name, 'FOREIGN_KEY', 
           STRING_AGG(COL_NAME(fkc.parent_object_id, fkc.parent_column_id), ', ') + ' -> ' + 
           OBJECT_NAME(fk.referenced_object_id) + '(' + 
           STRING_AGG(COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id), ', ') + ')'
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    WHERE fk.parent_object_id = OBJECT_ID('${tableName}')
    GROUP BY fk.name, fk.referenced_object_id
    UNION ALL
    SELECT cc.name, 'CHECK', cc.definition
    FROM sys.check_constraints cc
    WHERE cc.parent_object_id = OBJECT_ID('${tableName}')
    ORDER BY [Type], [ConstraintName]`);
    if (constraintResult.recordset.length > 0) {
        output += `\n═══ CONSTRAINTE ═══\n`;
        output += formatResultAsTable(constraintResult.recordset);
    }
    // Indexes
    const indexResult = await executeQuery(`
    SELECT i.name AS [Index], i.type_desc AS [Type],
           CASE i.is_unique WHEN 1 THEN 'UNIQUE' ELSE '' END AS [Unique],
           CASE i.is_primary_key WHEN 1 THEN 'PK' ELSE '' END AS [PK],
           STUFF((SELECT ', ' + c.name + CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE '' END
                  FROM sys.index_columns ic 
                  JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                  WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
                  ORDER BY ic.key_ordinal
                  FOR XML PATH('')), 1, 2, '') AS [KeyColumns],
           STUFF((SELECT ', ' + c.name
                  FROM sys.index_columns ic 
                  JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                  WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
                  ORDER BY ic.key_ordinal
                  FOR XML PATH('')), 1, 2, '') AS [IncludedColumns]
    FROM sys.indexes i
    WHERE i.object_id = OBJECT_ID('${tableName}') AND i.name IS NOT NULL
    ORDER BY i.index_id`);
    if (indexResult.recordset.length > 0) {
        output += `\n═══ INDECȘI ═══\n`;
        output += formatResultAsTable(indexResult.recordset);
    }
    return output || `Tabelul '${tableName}' nu a fost găsit.`;
}
async function handleGetDependencies(args) {
    const { objectName } = args;
    const depOnQuery = `
    SELECT DISTINCT 
      referenced_entity_name AS [DependsOn],
      referenced_minor_name AS [Column],
      referenced_class_desc AS [Type]
    FROM sys.dm_sql_referenced_entities('${objectName.includes('.') ? objectName : 'dbo.' + objectName}', 'OBJECT')
    WHERE referenced_entity_name IS NOT NULL
    ORDER BY referenced_entity_name`;
    const depByQuery = `
    SELECT DISTINCT 
      OBJECT_SCHEMA_NAME(referencing_id) + '.' + OBJECT_NAME(referencing_id) AS [ReferencedBy],
      o.type_desc AS [Type]
    FROM sys.sql_expression_dependencies sed
    JOIN sys.objects o ON sed.referencing_id = o.object_id
    WHERE sed.referenced_entity_name = '${objectName.includes('.') ? objectName.split('.').pop() : objectName}'
    ORDER BY [ReferencedBy]`;
    let output = '';
    try {
        const depOn = await executeQuery(depOnQuery);
        output += `═══ DEPINDE DE (obiecte referite) ═══\n`;
        output += depOn.recordset.length > 0
            ? formatResultAsTable(depOn.recordset)
            : 'Nicio dependență găsită.\n';
    }
    catch {
        output += `═══ DEPINDE DE ═══\nNu s-a putut determina.\n`;
    }
    try {
        const depBy = await executeQuery(depByQuery);
        output += `\n═══ REFERIT DE (obiecte dependente) ═══\n`;
        output += depBy.recordset.length > 0
            ? formatResultAsTable(depBy.recordset)
            : 'Niciun obiect dependent.\n';
    }
    catch {
        output += `\n═══ REFERIT DE ═══\nNu s-a putut determina.\n`;
    }
    return output;
}
async function handleCreateObject(args) {
    const { sql, description } = args;
    const sqlUpper = sql.trim().toUpperCase();
    if (!sqlUpper.startsWith('CREATE')) {
        return '❌ Instrucțiunea trebuie să înceapă cu CREATE. Folosește modify_object pentru ALTER.';
    }
    try {
        await executeQuery(sql);
        const objectMatch = sql.match(/CREATE\s+(?:OR\s+ALTER\s+)?(?:TABLE|VIEW|PROC(?:EDURE)?|FUNCTION|TRIGGER|INDEX|TYPE|SCHEMA|NONCLUSTERED\s+INDEX|CLUSTERED\s+INDEX|UNIQUE\s+INDEX)\s+(?:\[?(\w+)\]?\.)?(?:\[?(\w+)\]?)/i);
        const objectName = objectMatch
            ? `${objectMatch[1] || 'dbo'}.${objectMatch[2]}`
            : 'obiect';
        return `✅ Obiectul ${objectName} a fost creat cu succes.${description ? `\n📝 ${description}` : ''}`;
    }
    catch (err) {
        return `❌ Eroare la creare:\n${err.message}`;
    }
}
async function handleModifyObject(args) {
    const { sql, description } = args;
    const sqlUpper = sql.trim().toUpperCase();
    if (!sqlUpper.startsWith('ALTER') && !sqlUpper.startsWith('CREATE OR ALTER')) {
        return '❌ Instrucțiunea trebuie să înceapă cu ALTER sau CREATE OR ALTER. Folosește create_object pentru CREATE.';
    }
    try {
        await executeQuery(sql);
        return `✅ Obiectul a fost modificat cu succes.${description ? `\n📝 ${description}` : ''}`;
    }
    catch (err) {
        return `❌ Eroare la modificare:\n${err.message}`;
    }
}
async function handleAnalyzeIndexFragmentation(args) {
    const { tableName, minFragmentation = 5 } = args;
    const query = `
    SELECT 
      OBJECT_SCHEMA_NAME(ips.object_id) AS [Schema],
      OBJECT_NAME(ips.object_id) AS [Table],
      i.name AS [Index],
      i.type_desc AS [Type],
      ips.avg_fragmentation_in_percent AS [Fragmentation%],
      ips.page_count AS [Pages],
      ips.record_count AS [Rows],
      CASE 
        WHEN ips.avg_fragmentation_in_percent > 30 THEN 'REBUILD recomandat'
        WHEN ips.avg_fragmentation_in_percent > 5 THEN 'REORGANIZE recomandat'
        ELSE 'OK'
      END AS [Recomandare]
    FROM sys.dm_db_index_physical_stats(DB_ID(), ${tableName ? `OBJECT_ID('${tableName}')` : 'NULL'}, NULL, NULL, 'LIMITED') ips
    JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
    WHERE ips.avg_fragmentation_in_percent > ${minFragmentation}
      AND ips.page_count > 100
      AND i.name IS NOT NULL
    ORDER BY ips.avg_fragmentation_in_percent DESC`;
    const result = await executeQuery(query);
    if (result.recordset.length === 0) {
        return `✅ Nu s-au găsit indecși cu fragmentare > ${minFragmentation}%${tableName ? ` pentru tabelul '${tableName}'` : ''}.`;
    }
    return `═══ ANALIZA FRAGMENTĂRII INDECȘILOR ═══\n${formatResultAsTable(result.recordset)}\n\nLegendă: >30% = REBUILD, 5-30% = REORGANIZE. Folosește optimize_index pentru optimizare.`;
}
async function handleOptimizeIndex(args) {
    const { tableName, indexName, action = 'AUTO' } = args;
    if (indexName) {
        let finalAction = action;
        if (action === 'AUTO') {
            const fragQuery = `
        SELECT avg_fragmentation_in_percent AS frag
        FROM sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID('${tableName}'), NULL, NULL, 'LIMITED') ips
        JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
        WHERE i.name = '${indexName}'`;
            const fragResult = await executeQuery(fragQuery);
            if (fragResult.recordset.length === 0) {
                return `❌ Indexul '${indexName}' nu a fost găsit pe tabelul '${tableName}'.`;
            }
            finalAction = fragResult.recordset[0].frag > 30 ? 'REBUILD' : 'REORGANIZE';
        }
        try {
            await executeQuery(`ALTER INDEX [${indexName}] ON ${tableName} ${finalAction}`);
            return `✅ Indexul [${indexName}] pe ${tableName} a fost optimizat (${finalAction}).`;
        }
        catch (err) {
            return `❌ Eroare: ${err.message}`;
        }
    }
    // All indexes on table
    try {
        const allAction = action === 'AUTO' ? 'REORGANIZE' : action;
        await executeQuery(`ALTER INDEX ALL ON ${tableName} ${allAction}`);
        return `✅ Toți indecșii de pe ${tableName} au fost optimizați (${allAction}).`;
    }
    catch (err) {
        return `❌ Eroare: ${err.message}`;
    }
}
async function handleUpdateStatistics(args) {
    const { tableName, fullScan = false } = args;
    const scanOption = fullScan ? ' WITH FULLSCAN' : '';
    try {
        if (tableName) {
            await executeQuery(`UPDATE STATISTICS ${tableName}${scanOption}`);
            return `✅ Statisticile tabelului ${tableName} au fost actualizate${fullScan ? ' (FULLSCAN)' : ''}.`;
        }
        else {
            await executeQuery(`EXEC sp_updatestats`);
            return `✅ Toate statisticile bazei de date au fost actualizate.`;
        }
    }
    catch (err) {
        return `❌ Eroare: ${err.message}`;
    }
}
async function handleAnalyzeMissingIndexes(args) {
    const { tableName, minImpact = 100 } = args;
    const query = `
    SELECT TOP 20
      OBJECT_SCHEMA_NAME(mid.object_id) AS [Schema],
      OBJECT_NAME(mid.object_id) AS [Table],
      mid.equality_columns AS [EqualityColumns],
      mid.inequality_columns AS [InequalityColumns],
      mid.included_columns AS [IncludedColumns],
      migs.avg_user_impact AS [AvgImpact%],
      migs.user_seeks AS [Seeks],
      migs.user_scans AS [Scans],
      ROUND(migs.avg_user_impact * (migs.user_seeks + migs.user_scans), 0) AS [TotalImpact],
      'CREATE NONCLUSTERED INDEX [IX_' + OBJECT_NAME(mid.object_id) + '_' + 
        REPLACE(REPLACE(REPLACE(ISNULL(mid.equality_columns,''), '[', ''), ']', ''), ', ', '_') + '] ON ' +
        OBJECT_SCHEMA_NAME(mid.object_id) + '.' + OBJECT_NAME(mid.object_id) + 
        ' (' + ISNULL(mid.equality_columns,'') + 
        CASE WHEN mid.equality_columns IS NOT NULL AND mid.inequality_columns IS NOT NULL THEN ', ' ELSE '' END +
        ISNULL(mid.inequality_columns,'') + ')' +
        CASE WHEN mid.included_columns IS NOT NULL THEN ' INCLUDE (' + mid.included_columns + ')' ELSE '' END AS [SuggestedDDL]
    FROM sys.dm_db_missing_index_groups mig
    JOIN sys.dm_db_missing_index_group_stats migs ON mig.index_group_handle = migs.group_handle
    JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
    WHERE mid.database_id = DB_ID()
      AND ROUND(migs.avg_user_impact * (migs.user_seeks + migs.user_scans), 0) > ${minImpact}
      ${tableName ? `AND OBJECT_NAME(mid.object_id) = '${tableName.includes('.') ? tableName.split('.').pop() : tableName}'` : ''}
    ORDER BY [TotalImpact] DESC`;
    const result = await executeQuery(query);
    if (result.recordset.length === 0) {
        return `✅ Nu s-au găsit indecși lipsă semnificativi${tableName ? ` pentru '${tableName}'` : ''} (prag impact: ${minImpact}).`;
    }
    return `═══ INDECȘI LIPSĂ RECOMANDAȚI ═══\n${formatResultAsTable(result.recordset)}\n\n💡 Folosește create_object cu DDL-ul sugerat pentru a crea indexul.`;
}
async function handleAnalyzeQueryPlan(args) {
    try {
        await executeQuery('SET SHOWPLAN_XML ON');
        const result = await executeQuery(args.sql);
        await executeQuery('SET SHOWPLAN_XML OFF');
        if (result.recordset.length > 0) {
            const plan = result.recordset[0]['Microsoft SQL Server 2005 XML Showplan'] ||
                result.recordset[0][Object.keys(result.recordset[0])[0]];
            return `═══ PLAN DE EXECUȚIE ═══\n${plan}`;
        }
        return '❌ Nu s-a putut obține planul de execuție.';
    }
    catch (err) {
        try {
            await executeQuery('SET SHOWPLAN_XML OFF');
        }
        catch { }
        return `❌ Eroare: ${err.message}`;
    }
}
async function handlePrepareDelete(args) {
    const { objectName } = args;
    let { objectType } = args;
    // Auto-detect type
    if (!objectType || objectType === 'AUTO') {
        const detectQuery = `
      SELECT type_desc, type
      FROM sys.objects 
      WHERE object_id = OBJECT_ID('${objectName}')`;
        const detected = await executeQuery(detectQuery);
        if (detected.recordset.length === 0) {
            return `❌ Obiectul '${objectName}' nu a fost găsit în baza de date.`;
        }
        const typeDesc = detected.recordset[0].type_desc;
        if (typeDesc.includes('TABLE'))
            objectType = 'TABLE';
        else if (typeDesc.includes('VIEW'))
            objectType = 'VIEW';
        else if (typeDesc.includes('PROCEDURE'))
            objectType = 'PROCEDURE';
        else if (typeDesc.includes('FUNCTION'))
            objectType = 'FUNCTION';
        else if (typeDesc.includes('TRIGGER'))
            objectType = 'TRIGGER';
        else
            objectType = typeDesc;
    }
    let output = `\n⚠️  ATENȚIE: PREGĂTIRE ȘTERGERE OBIECT ⚠️\n`;
    output += `═══════════════════════════════════════\n`;
    output += `Obiect: ${objectName}\n`;
    output += `Tip: ${objectType}\n\n`;
    // Check dependencies
    try {
        const depByQuery = `
      SELECT DISTINCT 
        OBJECT_SCHEMA_NAME(referencing_id) + '.' + OBJECT_NAME(referencing_id) AS [ReferencedBy],
        o.type_desc AS [Type]
      FROM sys.sql_expression_dependencies sed
      JOIN sys.objects o ON sed.referencing_id = o.object_id
      WHERE sed.referenced_entity_name = '${objectName.includes('.') ? objectName.split('.').pop() : objectName}'`;
        const deps = await executeQuery(depByQuery);
        if (deps.recordset.length > 0) {
            output += `🔗 DEPENDENȚE (obiecte care vor fi AFECTATE):\n`;
            output += formatResultAsTable(deps.recordset);
            output += `\n⚠️  Ștergerea va AFECTA ${deps.recordset.length} obiect(e) dependente!\n`;
        }
        else {
            output += `✅ Nu s-au găsit dependențe directe.\n`;
        }
    }
    catch {
        output += `⚠️  Nu s-au putut verifica dependențele.\n`;
    }
    // For tables, check FK references and row count
    if (objectType === 'TABLE') {
        try {
            const fkQuery = `
        SELECT 
          OBJECT_SCHEMA_NAME(fk.parent_object_id) + '.' + OBJECT_NAME(fk.parent_object_id) AS [ReferencingTable],
          fk.name AS [FKName]
        FROM sys.foreign_keys fk
        WHERE fk.referenced_object_id = OBJECT_ID('${objectName}')`;
            const fks = await executeQuery(fkQuery);
            if (fks.recordset.length > 0) {
                output += `\n🔑 FOREIGN KEYS CARE REFERĂ ACEST TABEL:\n`;
                output += formatResultAsTable(fks.recordset);
            }
            const rowQuery = `
        SELECT SUM(p.rows) AS [RowCount]
        FROM sys.partitions p
        WHERE p.object_id = OBJECT_ID('${objectName}') AND p.index_id IN (0,1)`;
            const rows = await executeQuery(rowQuery);
            const rowCount = rows.recordset[0]?.RowCount || 0;
            output += `\n📊 Număr rânduri: ${rowCount}\n`;
        }
        catch { }
    }
    output += `\n═══════════════════════════════════════\n`;
    output += `💬 ÎNTREABĂ UTILIZATORUL dacă dorește să continue ștergerea.\n`;
    output += `   Dacă DA → apelează execute_delete cu confirmed=true\n`;
    output += `   Dacă NU → anulează operația\n`;
    output += `\n🚫 Această acțiune este IREVERSIBILĂ!\n`;
    return output;
}
async function handleExecuteDelete(args) {
    const { objectName, objectType, parentTable, confirmed } = args;
    if (!confirmed) {
        return '❌ Ștergerea nu a fost confirmată de utilizator. Operația a fost anulată.';
    }
    let dropSql = '';
    switch (objectType) {
        case 'TABLE':
            dropSql = `DROP TABLE ${objectName}`;
            break;
        case 'VIEW':
            dropSql = `DROP VIEW ${objectName}`;
            break;
        case 'PROCEDURE':
            dropSql = `DROP PROCEDURE ${objectName}`;
            break;
        case 'FUNCTION':
            dropSql = `DROP FUNCTION ${objectName}`;
            break;
        case 'TRIGGER':
            dropSql = `DROP TRIGGER ${objectName}`;
            break;
        case 'INDEX':
            if (!parentTable) {
                return '❌ Pentru ștergerea unui INDEX, trebuie specificat parentTable.';
            }
            dropSql = `DROP INDEX [${objectName}] ON ${parentTable}`;
            break;
        case 'TYPE':
            dropSql = `DROP TYPE ${objectName}`;
            break;
        case 'SCHEMA':
            dropSql = `DROP SCHEMA ${objectName}`;
            break;
        default:
            return `❌ Tip de obiect necunoscut: ${objectType}`;
    }
    try {
        await executeQuery(dropSql);
        return `✅ Obiectul ${objectType} '${objectName}' a fost ȘTERS cu succes.\n\nSQL executat: ${dropSql}`;
    }
    catch (err) {
        return `❌ Eroare la ștergere:\n${err.message}\n\nSQL încercat: ${dropSql}`;
    }
}
async function handleRunSql(args) {
    try {
        const result = await executeQuery(args.sql);
        if (result.recordset && result.recordset.length > 0) {
            return formatResultAsTable(result.recordset);
        }
        return `✅ Interogarea a fost executată cu succes. Rânduri afectate: ${result.rowsAffected.join(', ')}`;
    }
    catch (err) {
        return `❌ Eroare SQL:\n${err.message}`;
    }
}
async function handleGetDatabaseInfo() {
    const query = `
    SELECT 
      DB_NAME() AS [Database],
      @@SERVERNAME AS [Server],
      @@VERSION AS [SQLVersion],
      (SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped = 0) AS [Tables],
      (SELECT COUNT(*) FROM sys.views WHERE is_ms_shipped = 0) AS [Views],
      (SELECT COUNT(*) FROM sys.procedures WHERE is_ms_shipped = 0) AS [Procedures],
      (SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN','IF','TF') AND is_ms_shipped = 0) AS [Functions],
      (SELECT COUNT(*) FROM sys.triggers WHERE is_ms_shipped = 0) AS [Triggers],
      (SELECT SUM(size * 8 / 1024) FROM sys.database_files) AS [SizeMB],
      (SELECT recovery_model_desc FROM sys.databases WHERE database_id = DB_ID()) AS [RecoveryModel],
      (SELECT compatibility_level FROM sys.databases WHERE database_id = DB_ID()) AS [CompatibilityLevel],
      (SELECT collation_name FROM sys.databases WHERE database_id = DB_ID()) AS [Collation]`;
    const result = await executeQuery(query);
    if (result.recordset.length === 0) {
        return '❌ Nu s-au putut obține informații despre baza de date.';
    }
    const info = result.recordset[0];
    return `═══ INFORMAȚII BAZA DE DATE ═══
Baza de date: ${info.Database}
Server: ${info.Server}
Versiune SQL: ${info.SQLVersion?.split('\n')[0]}
Recovery Model: ${info.RecoveryModel}
Compatibilitate: ${info.CompatibilityLevel}
Collation: ${info.Collation}
Dimensiune: ${info.SizeMB} MB

═══ OBIECTE ═══
Tabele: ${info.Tables}
Views: ${info.Views}
Stored Procedures: ${info.Procedures}
Functions: ${info.Functions}
Triggers: ${info.Triggers}`;
}
// ============================================================================
// Utility Functions
// ============================================================================
function formatResultAsTable(rows) {
    if (!rows || rows.length === 0)
        return '(niciun rezultat)\n';
    const columns = Object.keys(rows[0]);
    const widths = columns.map((col) => {
        let maxWidth = col.length;
        for (const row of rows) {
            const val = formatValue(row[col]);
            if (val.length > maxWidth)
                maxWidth = val.length;
        }
        return Math.min(maxWidth, 60); // cap at 60 chars
    });
    let output = '';
    // Header
    output +=
        columns.map((col, i) => col.padEnd(widths[i])).join(' | ') + '\n';
    output += widths.map((w) => '-'.repeat(w)).join('-+-') + '\n';
    // Rows
    for (const row of rows) {
        output +=
            columns
                .map((col, i) => {
                const val = formatValue(row[col]);
                return val.substring(0, widths[i]).padEnd(widths[i]);
            })
                .join(' | ') + '\n';
    }
    output += `\n(${rows.length} rând${rows.length !== 1 ? 'uri' : ''})\n`;
    return output;
}
function formatValue(val) {
    if (val === null || val === undefined)
        return 'NULL';
    if (val instanceof Date)
        return val.toISOString().replace('T', ' ').substring(0, 19);
    if (typeof val === 'boolean')
        return val ? 'true' : 'false';
    if (typeof val === 'number')
        return val % 1 === 0 ? val.toString() : val.toFixed(2);
    return String(val);
}
// ============================================================================
// MCP Server Setup
// ============================================================================
const server = new Server({
    name: 'mcp-server-valyanmed',
    version: '1.0.0',
}, {
    capabilities: {
        tools: {},
    },
});
// List Tools Handler
server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools,
}));
// Call Tool Handler
server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const { name, arguments: args } = request.params;
    try {
        let result;
        switch (name) {
            case 'list_objects':
                result = await handleListObjects(args);
                break;
            case 'get_object_definition':
                result = await handleGetObjectDefinition(args);
                break;
            case 'get_table_details':
                result = await handleGetTableDetails(args);
                break;
            case 'get_dependencies':
                result = await handleGetDependencies(args);
                break;
            case 'create_object':
                result = await handleCreateObject(args);
                break;
            case 'modify_object':
                result = await handleModifyObject(args);
                break;
            case 'analyze_index_fragmentation':
                result = await handleAnalyzeIndexFragmentation(args);
                break;
            case 'optimize_index':
                result = await handleOptimizeIndex(args);
                break;
            case 'update_statistics':
                result = await handleUpdateStatistics(args);
                break;
            case 'analyze_missing_indexes':
                result = await handleAnalyzeMissingIndexes(args);
                break;
            case 'analyze_query_plan':
                result = await handleAnalyzeQueryPlan(args);
                break;
            case 'prepare_delete':
                result = await handlePrepareDelete(args);
                break;
            case 'execute_delete':
                result = await handleExecuteDelete(args);
                break;
            case 'run_sql':
                result = await handleRunSql(args);
                break;
            case 'get_database_info':
                result = await handleGetDatabaseInfo();
                break;
            default:
                result = `❌ Tool necunoscut: ${name}`;
        }
        return {
            content: [
                {
                    type: 'text',
                    text: result,
                },
            ],
        };
    }
    catch (error) {
        return {
            content: [
                {
                    type: 'text',
                    text: `❌ Eroare la executarea tool-ului '${name}':\n${error.message}`,
                },
            ],
            isError: true,
        };
    }
});
// Start Server
async function main() {
    const transport = new StdioServerTransport();
    await server.connect(transport);
    console.error('MCP Server ValyanMed pornit - conectat la DESKTOP-3Q8HI82\\ERP / ValyanMed');
}
main().catch((error) => {
    console.error('Eroare la pornirea serverului MCP:', error);
    process.exit(1);
});
//# sourceMappingURL=index.js.map