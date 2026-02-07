# MCP Server ValyanMed

Server MCP (Model Context Protocol) pentru managementul bazei de date **ValyanMed** pe serverul `DESKTOP-3Q8HI82\ERP`.

## Funcționalități

### 🔍 Explorare
| Tool | Descriere |
|------|-----------|
| `list_objects` | Listează obiecte (TABLE, VIEW, PROCEDURE, FUNCTION, TRIGGER, INDEX, SCHEMA, TYPE, SYNONYM) |
| `get_object_definition` | Obține definiția/DDL-ul unui obiect |
| `get_table_details` | Detalii complete despre un tabel (coloane, constrainte, indecși) |
| `get_dependencies` | Verifică dependențele unui obiect |
| `get_database_info` | Informații generale despre baza de date |
| `run_sql` | Execută interogări SQL arbitrare |

### ✨ Creare
| Tool | Descriere |
|------|-----------|
| `create_object` | Creează obiecte noi (CREATE TABLE, VIEW, PROCEDURE, etc.) |

### ✏️ Modificare
| Tool | Descriere |
|------|-----------|
| `modify_object` | Modifică obiecte existente (ALTER, CREATE OR ALTER) |

### ⚡ Optimizare
| Tool | Descriere |
|------|-----------|
| `analyze_index_fragmentation` | Analizează fragmentarea indecșilor |
| `optimize_index` | REORGANIZE sau REBUILD indecși |
| `update_statistics` | Actualizează statisticile |
| `analyze_missing_indexes` | Indecși lipsă recomandați de SQL Server |
| `analyze_query_plan` | Plan de execuție estimat pentru o interogare |

### 🗑️ Ștergere (cu confirmare)
| Tool | Descriere |
|------|-----------|
| `prepare_delete` | **Pas 1**: Analizează dependențe și pregătește raport |
| `execute_delete` | **Pas 2**: Execută ștergerea doar cu `confirmed=true` |

> ⚠️ Ștergerea necesită **2 pași**: mai întâi `prepare_delete` pentru analiza dependențelor, apoi confirmarea utilizatorului, apoi `execute_delete`.

## Configurare

### Conexiune
Serverul se conectează automat la:
- **Server**: `DESKTOP-3Q8HI82\ERP`
- **Database**: `ValyanMed`
- **Autentificare**: Windows Authentication (Trusted Connection)

### Build & Run
```powershell
cd mcp-server-valyanmed
npm install
npm run build
```

### VS Code
Configurarea este în `.vscode/mcp.json`:
```json
{
    "servers": {
        "valyanmed-db": {
            "type": "stdio",
            "command": "node",
            "args": ["${workspaceFolder}/mcp-server-valyanmed/dist/index.js"]
        }
    }
}
```

## Exemple de utilizare

### Explorare
- "Listează toate tabelele din ValyanMed"
- "Arată structura tabelului Pacient"
- "Ce stored procedures conțin 'Consultatie'?"
- "Ce dependențe are tabelul PersonalMedical?"

### Creare
- "Creează un tabel pentru stocarea logurilor de audit"
- "Creează un stored procedure pentru raportul lunar"

### Modificare
- "Adaugă o coloană Email la tabelul Pacient"
- "Modifică stored procedure-ul Consultatie_GetById"

### Optimizare
- "Verifică fragmentarea indecșilor"
- "Ce indecși lipsesc?"
- "Optimizează indecșii tabelului Consultatie"
- "Actualizează statisticile bazei de date"

### Ștergere
- "Vreau să șterg tabelul TestTable" → analizează dependențe → cere confirmare → execută
