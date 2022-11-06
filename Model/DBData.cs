using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DBManager.Model
{
    internal static class DBData
    {
        /// <summary>
        /// Get all databases at the server
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <returns>List of database's names</returns>
        /// <exception cref="Exception">Throw an exception if cannot connect to the server</exception>
        public static async Task<List<string>> GetDBNames(string serverName)
        {
            List<string> db = new List<string>();

            using (SqlConnection connection = DBConnection.GetConnection(GetServerConnectionString(serverName)))
            {
                try
                {
                    await connection.OpenAsync();

                    string commandString = @"select * from sys.databases where name not in ('master', 'model', 'msdb', 'tempdb') order by name";

                    using (SqlCommand command = new SqlCommand(commandString, connection))
                    {
                        SqlDataReader sqlReader = command.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            db.Add(sqlReader.GetString(0));
                        }

                        sqlReader.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot connect to the server", ex);
                }
            }

            return db;
        }


        /// <summary>
        /// Get general schema of the database
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <param name="dbName">Name of the database</param>
        /// <returns>List of the tables in this database</returns>
        /// <exception cref="Exception">Throw an exception if cannot connect to the database</exception>
        public static async Task<List<DBTable>> GetDBSchema(string serverName, string dbName)
        {
            using (SqlConnection connection = DBConnection.GetConnection(GetDBConnectionString(serverName, dbName)))
            {
                try
                {
                    await connection.OpenAsync();

                    List<DBTable> tables = new List<DBTable>();
                    string[] restrictions = new string[4];

                    DataTable tableSchema = connection.GetSchema("tables");

                    foreach (DataRow row in tableSchema.Rows)
                    {
                        if(row["TABLE_NAME"].ToString() == null || row["TABLE_NAME"].ToString()!.ToLower().Equals("sysdiagrams"))
                        {
                            continue;
                        }
                        DBTable table = new DBTable() { TableName = row["TABLE_NAME"].ToString() };

                        restrictions[2] = row["TABLE_NAME"].ToString()!;

                        DataTable columnsSchema = connection.GetSchema("columns", restrictions);

                        foreach (DataRow columnRow in columnsSchema.Rows)
                        {
                            DBTableColumn column = new DBTableColumn()
                            {
                                ColumnName = columnRow["COLUMN_NAME"].ToString()!,
                                ColumnDataType = columnRow["DATA_TYPE"].ToString()!,
                                IsNullable = columnRow["IS_NULLABLE"].ToString()!.ToLower().Equals("yes")
                            };

                            if (columnRow["CHARACTER_MAXIMUM_LENGTH"] is not DBNull)
                            {
                                column.CharacterMaximumLength = (int)columnRow["CHARACTER_MAXIMUM_LENGTH"];
                            }

                            table.Columns.Add(column);
                        }

                        DataTable indexColumnsSchema = connection.GetSchema("IndexColumns", restrictions);
                        foreach (DataRow indexColRow in indexColumnsSchema.Rows)
                        {
                            table.Indexes.Add(indexColRow["index_name"].ToString()!);

                            if (indexColRow["index_name"].ToString()!.ToLower().StartsWith("pk"))
                            {
                                DBTableColumn? c = table.Columns.Where((col) => col.ColumnName!.Equals(indexColRow["column_name"])).FirstOrDefault();
                                if (c != null)
                                {
                                    c.IsPrimaryKey = true;
                                }
                            }
                        }

                        DataTable foreignKeysSchema = connection.GetSchema("ForeignKeys", restrictions);
                        foreach (DataRow forKeyRow in foreignKeysSchema.Rows)
                        {
                            table.ForeignKeys.Add(forKeyRow["CONSTRAINT_NAME"].ToString()!);
                        }

                        tables.Add(table);
                    }

                    return tables;
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot connect to the database", ex);
                }
            }
        }

        /// <summary>
        /// Determines if the databas contains the table
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <param name="dbName">Name of the database</param>
        /// <param name="tableName">Name of the table</param>
        /// <returns>True if the database contains the table otherwise false</returns>
        /// <exception cref="Exception">Throw an exception if cannot connect to the database</exception>
        private static async Task<bool> IsTableInDB(string serverName, string dbName, string tableName)
        {
            using (SqlConnection connection = DBConnection.GetConnection(GetDBConnectionString(serverName, dbName)))
            {
                try
                {
                    if (connection.State != ConnectionState.Open)
                    {
                        await connection.OpenAsync();
                    }

                    DataTable tableSchema = connection.GetSchema("tables");

                    foreach (DataRow row in tableSchema.Rows)
                    {
                        if(row["TABLE_NAME"].ToString()!.ToLower().Equals(tableName.ToLower()))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot connect to the database", ex);
                }
            }
        }

        /// <summary>
        /// Load rows of the table from the database
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <param name="dbName">Name of the database</param>
        /// <param name="tableName">Name of the table</param>
        /// <returns>DataTable object with values</returns>
        /// <exception cref="Exception">Throw an exception if cannot connect to the database or if the database does not contain the table</exception>
        public static async Task<DataTable> GetTableValues(string serverName, string dbName, string tableName)
        {
            if(!(await IsTableInDB(serverName, dbName, tableName)))
            {
                throw new Exception("The database does not contain this table");
            }

            using (SqlConnection connection = DBConnection.GetConnection(GetDBConnectionString(serverName, dbName)))
            {
                try
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand($"select * from {tableName}", connection))
                    {
                        try
                        {
                            SqlDataReader reader = await command.ExecuteReaderAsync();
                            DataTable table = new DataTable();
                            table.Load(reader);

                            return table;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("SQL command error", ex);
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Cannot connect to the database", ex);
                }
            }
        }

        /// <summary>
        /// Insert a new row into the table
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <param name="dbName">Name of the database</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="row">New row to insert</param>
        /// <returns>True if a new row was successfully inserted otherwise false</returns>
        /// <exception cref="Exception">Throw an exception if cannot connect to the database or if the SQL command returns error</exception>
        public static async Task<bool> InsertRow(string serverName, string dbName, string tableName, DBTableRow row)
        {
            using (SqlConnection connection = DBConnection.GetConnection(GetDBConnectionString(serverName, dbName)))
            {
                try
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand())
                    {

                        StringBuilder sqlCommand = new StringBuilder($"insert into [{tableName}] (");
                        StringBuilder valuesParameters = new StringBuilder();

                        int i = 0;
                        foreach (var cell in row.Values)
                        {
                            if (cell.Key.ToLower().Equals("id"))
                            {
                                continue;
                            }

                            sqlCommand.Append($"[{cell.Key}],");
                            valuesParameters.Append($"@p{i},");
                            command.Parameters.AddWithValue($"@p{i}", cell.Value ?? DBNull.Value);
                            i++;
                        }

                       
                        sqlCommand.Remove(sqlCommand.Length - 1, 1);
                        sqlCommand.Append(") values (");

                        sqlCommand.Append(valuesParameters);
                        sqlCommand.Remove(sqlCommand.Length - 1, 1);
                        sqlCommand.Append(");");

                        command.CommandText = sqlCommand.ToString();
                        command.Connection = connection;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error of the SQL command", ex);
                }
            }
        }

        /// <summary>
        /// Update a row in the table
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <param name="dbName">Name of the database</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="row">Row to be updated</param>
        /// <returns>True if a new row was successfully updated otherwise false</returns>
        /// <exception cref="Exception">Throw an exception if cannot connect to the database or if the SQL command returns error</exception>
        public static async Task<bool> UpdateRow(string serverName, string dbName, string tableName, DBTableRow row)
        {
            object? id = GetId(row);
            if(id == null)
            {
                return false;
            }

            using (SqlConnection connection = DBConnection.GetConnection(GetDBConnectionString(serverName, dbName)))
            {
                try
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand())
                    {
                        StringBuilder sqlCommand = new StringBuilder($"update [{tableName}] set ");

                        int i = 0;
                        foreach (var cell in row.Values)
                        {

                            if(cell.Key.ToLower().Equals("id"))
                            {
                                continue;
                            }

                            sqlCommand.Append($"[{cell.Key}]=@p{i},");
                            command.Parameters.AddWithValue($"@p{i}", cell.Value ?? DBNull.Value);
                            i++;
                        }


                        sqlCommand.Remove(sqlCommand.Length - 1, 1);
                        sqlCommand.Append(" where id=@id;");

                        command.Parameters.Add(new SqlParameter("@id", SqlDbType.Int)).Value = id;

                        command.CommandText = sqlCommand.ToString();
                        command.Connection = connection;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error of the SQL command", ex);
                }
            }
        }

        /// <summary>
        /// Delete a row in the table
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <param name="dbName">Name of the database</param>
        /// <param name="tableName">Name of the table</param>
        /// <param name="row">Row to delete</param>
        /// <returns>True if a new row was successfully deleted otherwise false</returns>
        /// <exception cref="Exception">Throw an exception if cannot connect to the database or if the SQL command returns error</exception>
        public static async Task<bool> DeleteRow(string serverName, string dbName, string tableName, DBTableRow row)
        {
            object? id = GetId(row);
            if (id == null)
            {
                return false;
            }

            using (SqlConnection connection = DBConnection.GetConnection(GetDBConnectionString(serverName, dbName)))
            {
                try
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand())
                    {
                        string sqlCommand = $"delete from[{tableName}] where [id]=@p0;";

                        command.Parameters.Add(new SqlParameter("@p0", SqlDbType.Int)).Value = id;

                        command.CommandText = sqlCommand.ToString();
                        command.Connection = connection;

                        return command.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error of the SQL command", ex);
                }
            }
        }

        /// <summary>
        /// Get the value of the column Id
        /// </summary>
        /// <param name="row">Table row</param>
        /// <returns>Id value if the row has Id column otherwise null</returns>
        private static object? GetId(DBTableRow row)
        {
            var idColumn = row.Values.FirstOrDefault(column => column.Key.ToLower().Equals("id"));

            return idColumn.Value;
        }
        
        /// <summary>
        /// Get the connection string to connect to the server
        /// </summary>
        /// <param name="serverName">Name of the server to connect</param>
        /// <returns>Connection string</returns>
        private static string GetServerConnectionString(string serverName)
        {
            return ConfigurationManager.ConnectionStrings["ServerConnection"].ConnectionString.Replace("[server_name]", serverName);
        }

        /// <summary>
        /// Get the connection string to connect to the database
        /// </summary>
        /// <param name="serverName">Name of the server</param>
        /// <param name="dbName">Name of the database to connect</param>
        /// <returns>Connection string</returns>
        private static string GetDBConnectionString(string serverName, string dbName)
        {
            return ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString.Replace("[server_name]", serverName).Replace("[db_name]", dbName);
        }

    }
}
