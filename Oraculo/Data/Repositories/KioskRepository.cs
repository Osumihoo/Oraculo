using Oraculo.Models;
using Sap.Data.Hana;

namespace Oraculo.Data.Repositories
{
    public class KioskRepository : IKioskRepository
    {
        private readonly Func<int, HanaConnection> _connectionString;

        public KioskRepository(Func<int, HanaConnection> connectionString)
        {
            _connectionString = connectionString;
        }
        private static T GetValueOrDefault<T>(HanaDataReader reader, string column, T defaultValue)
        {
            return reader[column] == DBNull.Value ? defaultValue : (T)Convert.ChangeType(reader[column], typeof(T));
        }

        public async Task<List<KioskStockComplete>> GetKioskStockComplete(int environment, string whsCode, string priceList)
        {
            var result = new List<KioskStockComplete>();

            using (HanaConnection conn = _connectionString(environment))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        T0.""ItemCode"", 
                        T0.""ItemName"", 
                        T2.""OnHand"", 
                        T0.""NumInSale"", 
                        T1.""Price"", 
                        T0.""ItmsGrpCod""
                    FROM OITM T0  
                    INNER JOIN ITM1 T1 ON T0.""ItemCode"" = T1.""ItemCode"" 
                    INNER JOIN OITW T2 ON T0.""ItemCode"" = T2.""ItemCode"" 
                    WHERE 
                        T2.""WhsCode"" = ?
                        AND T0.""frozenFor"" = 'N'
                        AND T1.""PriceList"" = ?
                    ORDER BY T0.""ItemCode"";
                ";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("", whsCode);
                    cmd.Parameters.AddWithValue("", priceList);

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new KioskStockComplete
                            {
                                ItemCode = reader["ItemCode"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                OnHand = GetValueOrDefault(reader, "OnHand", 0m),
                                NumInSale = GetValueOrDefault(reader, "NumInSale", 0m),
                                Price = GetValueOrDefault(reader, "Price", 0m),
                                ItmsGrpCod = reader["ItmsGrpCod"].ToString()
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<List<KioskPriceUpdates>> GetKioskPriceUpdates(int environment, string whsCode, string priceList)
        {
            var result = new List<KioskPriceUpdates>();

            using (HanaConnection conn = _connectionString(environment))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        T0.""ItemCode"", 
                        T0.""NumInSale"", 
                        T1.""Price""
                    FROM OITM T0  
                    INNER JOIN ITM1 T1 ON T0.""ItemCode"" = T1.""ItemCode"" 
                    INNER JOIN OITW T2 ON T0.""ItemCode"" = T2.""ItemCode"" 
                    WHERE 
                        T2.""WhsCode"" = ?
                        AND T0.""frozenFor"" = 'N'
                        AND T1.""PriceList"" = ?
                    ORDER BY T0.""ItemCode"";
                ";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("", whsCode);
                    cmd.Parameters.AddWithValue("", priceList);

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new KioskPriceUpdates
                            {
                                ItemCode = reader["ItemCode"].ToString(),
                                NumInSale = GetValueOrDefault(reader, "NumInSale", 0m),
                                Price = GetValueOrDefault(reader, "Price", 0m)
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
