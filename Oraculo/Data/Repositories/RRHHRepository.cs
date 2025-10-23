using Oraculo.Models;
using Sap.Data.Hana;
using System;

namespace Oraculo.Data.Repositories
{
    public class RRHHRepository : IRRHHRepository
    {
        private readonly Func<int, HanaConnection> _connectionString;

        public RRHHRepository(Func<int, HanaConnection> connectionString)
        {
            _connectionString = connectionString;
        }

        protected HanaConnection dbConnection(int environment)
        {
            return _connectionString(environment);
        }
        private static T GetValueOrDefault<T>(HanaDataReader reader, string column, T defaultValue)
        {
            return reader[column] == DBNull.Value ? defaultValue : (T)Convert.ChangeType(reader[column], typeof(T));
        }
        public async Task<List<RRHHVariousDebtors>> GetVariousDebtorsAsync(int environment)
        {
            var result = new List<RRHHVariousDebtors>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                                SELECT
                                    T1.""ShortName"",
                                    T2.""CardName"", 
                                    T1.""LineMemo"",
                                    T1.""RefDate"",
                                    T1.""Debit"" AS ""Cargo"",
                                    T1.""Credit"" AS ""Abono""
                                FROM ""SBO_ELVALOR_PRODUCTIVA"".""JDT1"" T1 
                                INNER JOIN OCRD T2 ON T1.""ShortName"" = T2.""CardCode""
                                WHERE T2.""CardCode"" LIKE 'DD%'
                                  AND T2.""Balance"" != '0'
                                ORDER BY T1.""ShortName""";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                using (HanaDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new RRHHVariousDebtors
                        {
                            ShortName = reader["ShortName"].ToString(),
                            CardName = reader["CardName"].ToString(),
                            LineMemo = reader["LineMemo"].ToString(),
                            RefDate = GetValueOrDefault(reader, "RefDate", DateTime.MinValue),
                            Cargo = GetValueOrDefault(reader, "Cargo", 0m),
                            Abono = GetValueOrDefault(reader, "Abono", 0m)
                        });
                    }
                }
            }

            return result;
        }

    }
}
