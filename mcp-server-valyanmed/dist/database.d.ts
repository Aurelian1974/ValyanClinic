import sql from 'mssql';
export declare function getConnection(): Promise<sql.ConnectionPool>;
export declare function executeQuery(query: string): Promise<sql.IResult<any>>;
export declare function closeConnection(): Promise<void>;
//# sourceMappingURL=database.d.ts.map