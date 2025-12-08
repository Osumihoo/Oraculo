using Oraculo.Models;
using Sap.Data.Hana;
using System;

namespace Oraculo.Data.Repositories
{
    public class ITRepository : IITRepository
    {
        private readonly Func<int, HanaConnection> _connectionString;

        public ITRepository(Func<int, HanaConnection> connectionString)
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
        public async Task<List<ITStockByWh>> GetStockByWh(int environment, string whsCode)
        {
            var result = new List<ITStockByWh>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT 
                    T0.""ItemCode"",
                    T0.""ItemName"",
                    T1.""OnHand"" AS ""Piezas"",
                    T0.""NumInSale"",
                    -- Evitamos division por cero: si NumInSale = 0 devolvemos 0
                    CASE 
                      WHEN COALESCE(T0.""NumInSale"",0) = 0 THEN 0
                      ELSE ROUND(T1.""OnHand"" / T0.""NumInSale"", 2)
                    END AS ""Cajas""
                FROM OITM T0
                INNER JOIN OITW T1 ON T0.""ItemCode"" = T1.""ItemCode""
                WHERE T1.""WhsCode"" = :whs
                  AND T0.""frozenFor"" = 'N'
                ORDER BY T0.""ItemCode"";";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    // Parámetro
                    cmd.Parameters.AddWithValue("whs", whsCode);

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new ITStockByWh
                            {
                                ItemCode = reader["ItemCode"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                Piezas = GetValueOrDefault(reader, "Piezas", 0m),
                                NumInSale = GetValueOrDefault(reader, "NumInSale", 0m),
                                Cajas = GetValueOrDefault(reader, "Cajas", 0m)
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
