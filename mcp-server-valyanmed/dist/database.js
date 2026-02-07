import sql from 'mssql';
const config = {
    server: 'DESKTOP-3Q8HI82\\ERP',
    database: 'ValyanMed',
    options: {
        encrypt: false,
        trustServerCertificate: true,
        enableArithAbort: true,
    },
    authentication: {
        type: 'default',
        options: {
            userName: '',
            password: '',
        },
    },
    // Use Windows Authentication
    driver: 'msnodesqlv8',
};
// For Windows Auth we use a connection string approach
const connectionString = `Server=DESKTOP-3Q8HI82\\ERP;Database=ValyanMed;Trusted_Connection=Yes;TrustServerCertificate=Yes;Driver={ODBC Driver 17 for SQL Server}`;
let pool = null;
export async function getConnection() {
    if (pool && pool.connected) {
        return pool;
    }
    try {
        // Try Windows Authentication first via config
        pool = new sql.ConnectionPool({
            server: 'DESKTOP-3Q8HI82\\ERP',
            database: 'ValyanMed',
            options: {
                encrypt: false,
                trustServerCertificate: true,
                enableArithAbort: true,
                trustedConnection: true,
            },
        });
        await pool.connect();
        return pool;
    }
    catch (err) {
        // Fallback: try with integrated security in connection string
        try {
            pool = await sql.connect(`Server=DESKTOP-3Q8HI82\\ERP;Database=ValyanMed;Trusted_Connection=Yes;TrustServerCertificate=Yes;`);
            return pool;
        }
        catch (err2) {
            throw new Error(`Nu s-a putut conecta la DESKTOP-3Q8HI82\\ERP / ValyanMed: ${err2}`);
        }
    }
}
export async function executeQuery(query) {
    const conn = await getConnection();
    return conn.request().query(query);
}
export async function closeConnection() {
    if (pool) {
        await pool.close();
        pool = null;
    }
}
//# sourceMappingURL=database.js.map