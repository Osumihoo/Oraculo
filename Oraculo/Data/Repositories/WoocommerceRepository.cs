using Oraculo.Models;
using Sap.Data.Hana;
using System;

namespace Oraculo.Data.Repositories
{
    public class WoocommerceRepository : IWoocommerceRepository
    {
        private readonly Func<int, HanaConnection> _connectionString;

        public WoocommerceRepository(Func<int, HanaConnection> connectionString)
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

        public async Task<List<WoocommerceStockComplete>> GetStockComplete(int environment)
        {
            var result = new List<WoocommerceStockComplete>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                (
                    SELECT 
                        T0.""ItemCode"", 
                        T0.""ItemName"", 
                        T1.""WhsCode"", 
                        T3.""ItmsGrpNam"", 
                        T2.""Price"" AS ""UnitPrice"",
                        (T2.""Price"" * T0.""NumInSale"") AS ""BoxPrice"",
                        ROUND((SUM(T1.""OnHand"")/T0.""NumInSale""),0) AS ""Stock"",
                        T0.""NumInSale"",
                        CASE 
                            WHEN LEFT(T0.""ItemName"", 3) = 'RET' THEN 1 
                            ELSE 0 
                        END AS ""EsRET"",
                        T0.""SWeight1"", 
                        T0.""SWidth1"",
                        T0.""SHeight1"",
                        T0.""SLength1"",
                        T0.""SVolume""
                    FROM OITM T0
                    INNER JOIN OITW T1 ON T0.""ItemCode"" = T1.""ItemCode""
    
                    INNER JOIN ITM1 T2 
                        ON T0.""ItemCode"" = T2.""ItemCode""
                        AND T2.""PriceList"" = 
                            CASE 
                                WHEN T1.""WhsCode"" = '323' THEN 6
                                WHEN T1.""WhsCode"" = '329' THEN 7
                                WHEN T1.""WhsCode"" = '331' THEN 8
                                WHEN T1.""WhsCode"" IN ('303','325','327','334') THEN 2
                                ELSE 1
                            END
    
                    INNER JOIN OITB T3 ON T0.""ItmsGrpCod"" = T3.""ItmsGrpCod""
    
                    WHERE 
                        T0.""validFor"" = 'Y'
                        AND T1.""WhsCode"" IN 
                        ('303','309','311','313','315','317','319','321',
                            '323','325','327','329','331','334','338')
    
                    GROUP BY 
                        T0.""ItemCode"", 
                        T0.""ItemName"", 
                        T1.""WhsCode"", 
                        T3.""ItmsGrpNam"", 
                        T2.""Price"",
                        T0.""NumInSale"",
                        T0.""SWeight1"", 
                        T0.""SWidth1"",
                        T0.""SHeight1"",
                        T0.""SLength1"",
                        T0.""SVolume""
                )

                UNION ALL

                (
                    SELECT 
                        T0.""ItemCode"", 
                        T0.""ItemName"", 
                        '300' AS ""WhsCode"", 
                        T3.""ItmsGrpNam"", 
                        T2.""Price"" AS ""UnitPrice"",
                        (T2.""Price"" * T0.""NumInSale"") AS ""BoxPrice"",
                        ROUND((SUM(T1.""OnHand"")/T0.""NumInSale""),0) AS ""Stock"",
                        T0.""NumInSale"",
                        CASE 
                            WHEN LEFT(T0.""ItemName"", 3) = 'RET' THEN 1 
                            ELSE 0 
                        END AS ""EsRET"",
                        T0.""SWeight1"", 
                        T0.""SWidth1"",
                        T0.""SHeight1"",
                        T0.""SLength1"",
                        T0.""SVolume""
                    FROM OITM T0
                    INNER JOIN OITW T1 ON T0.""ItemCode"" = T1.""ItemCode""
    
                    INNER JOIN ITM1 T2 
                        ON T0.""ItemCode"" = T2.""ItemCode""
                        AND T2.""PriceList"" = 1

                    INNER JOIN OITB T3 ON T0.""ItmsGrpCod"" = T3.""ItmsGrpCod""
    
                    WHERE 
                        T0.""validFor"" = 'Y'
                        AND T1.""WhsCode"" IN ('300','301','309','311','313')
    
                    GROUP BY 
                        T0.""ItemCode"", 
                        T0.""ItemName"", 
                        T3.""ItmsGrpNam"", 
                        T2.""Price"",
                        T0.""NumInSale"",
                        T0.""SWeight1"", 
                        T0.""SWidth1"",
                        T0.""SHeight1"",
                        T0.""SLength1"",
                        T0.""SVolume""
                )

                ORDER BY ""ItemCode"", ""WhsCode"";
                ";


                using (HanaCommand cmd = new HanaCommand(query, conn))
                using (HanaDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new WoocommerceStockComplete
                        {
                            ItemCode = reader["ItemCode"].ToString(),
                            ItemName = reader["ItemName"].ToString(),
                            WhsCode = reader["WhsCode"].ToString(),
                            ItmsGrpNam = reader["ItmsGrpNam"].ToString(),
                            UnitPrice = Math.Round(GetValueOrDefault(reader, "UnitPrice", 0m), 2),   // unitario
                            BoxPrice = Math.Round(GetValueOrDefault(reader, "BoxPrice", 0m), 2), // nuevo campo
                            Stock = Math.Round(GetValueOrDefault(reader, "Stock", 0m), 2),
                            NumInSale = GetValueOrDefault(reader, "NumInSale", 0m),
                            EsRET = GetValueOrDefault(reader, "EsRET", 0),
                            SWeight1 = GetValueOrDefault(reader, "SWeight1", 0m),
                            SWidth1 = GetValueOrDefault(reader, "SWidth1", 0m),
                            SHeight1 = GetValueOrDefault(reader, "SHeight1", 0m),
                            SLength1 = GetValueOrDefault(reader, "SLength1", 0m),
                            SVolume = GetValueOrDefault(reader, "SVolume", 0m)
                        });
                    }
                }

            }

            return result;
        }

        public async Task<List<WoocommercePriceUpdate>> GetPriceUpdates(int environment)
        {
            var result = new List<WoocommercePriceUpdate>();

            using (var conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT 
                    T0.""ItemCode"",
                    T0.""ItemName"",
                    W.""WhsCode"",
                    T1.""Price"" AS ""UnitPrice"",
                    (T1.""Price"" * T0.""NumInSale"") AS ""Price""

                FROM OITM T0

                INNER JOIN ITM1 T1 
                    ON T0.""ItemCode"" = T1.""ItemCode""

                INNER JOIN (
                        -- LISTA 1
                        SELECT '309' AS ""WhsCode"", 1 AS ""PriceList"" FROM DUMMY UNION ALL
                        SELECT '311', 1 FROM DUMMY UNION ALL
                        SELECT '313', 1 FROM DUMMY UNION ALL
                        SELECT '315', 1 FROM DUMMY UNION ALL
                        SELECT '317', 1 FROM DUMMY UNION ALL
                        SELECT '319', 1 FROM DUMMY UNION ALL
                        SELECT '321', 1 FROM DUMMY UNION ALL
                        SELECT '300', 1 FROM DUMMY UNION ALL
                        SELECT '338', 1 FROM DUMMY UNION ALL
        
                        -- LISTA 2
                        SELECT '303', 2 FROM DUMMY UNION ALL
                        SELECT '325', 2 FROM DUMMY UNION ALL
                        SELECT '327', 2 FROM DUMMY UNION ALL
                        SELECT '334', 2 FROM DUMMY UNION ALL
        
                        -- LISTAS ESPECIALES
                        SELECT '323', 6 FROM DUMMY UNION ALL
                        SELECT '329', 7 FROM DUMMY UNION ALL
                        SELECT '331', 8 FROM DUMMY

                ) W
                    ON T1.""PriceList"" = W.""PriceList""

                WHERE 
                    T1.""PriceList"" IN (1,2,6,7,8)
                    AND T0.""UpdateDate"" = CURRENT_DATE

                ORDER BY T0.""ItemCode"", W.""WhsCode"";
                ";

                using (var cmd = new HanaCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new WoocommercePriceUpdate
                        {
                            ItemCode = reader.GetString(0),
                            ItemName = reader.GetString(1),
                            WhsCode = reader.GetString(2),
                            UnitPrice = reader.GetDecimal(3),
                            Price = reader.GetDecimal(4)
                        });
                    }
                }
            }

            return result;
        }

        public async Task<List<WoocommerceStockComplete>> GetAllStockByBranch(int environment, string branch)
        {
            var result = new List<WoocommerceStockComplete>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                        (
                            SELECT 
                                T0.""ItemCode"", 
                                T0.""ItemName"", 
                                T1.""WhsCode"", 
                                T3.""ItmsGrpNam"", 
                                T2.""Price"" AS ""UnitPrice"",
                                (T2.""Price"" * T0.""NumInSale"") AS ""BoxPrice"",
                                ROUND((SUM(T1.""OnHand"")/T0.""NumInSale""),0) AS ""Stock"",
                                T0.""NumInSale"",
                                CASE 
                                    WHEN LEFT(T0.""ItemName"", 3) = 'RET' THEN 1 
                                    ELSE 0 
                                END AS ""EsRET"",
                                T0.""SWeight1"", 
                                T0.""SWidth1"",
                                T0.""SHeight1"",
                                T0.""SLength1"",
                                T0.""SVolume""
                            FROM OITM T0
                            INNER JOIN OITW T1 ON T0.""ItemCode"" = T1.""ItemCode""
                            INNER JOIN ITM1 T2 ON T0.""ItemCode"" = T2.""ItemCode""
                            INNER JOIN OITB T3 ON T0.""ItmsGrpCod"" = T3.""ItmsGrpCod""
                            WHERE 
                                T0.""validFor"" = 'Y'
                                AND T2.""PriceList"" = 1
                                AND T1.""WhsCode"" = ?
                            GROUP BY 
                                T0.""ItemCode"", 
                                T0.""ItemName"", 
                                T1.""WhsCode"", 
                                T3.""ItmsGrpNam"", 
                                T2.""Price"",
                                T0.""NumInSale"",
                                T0.""SWeight1"", 
                                T0.""SWidth1"",
                                T0.""SHeight1"",
                                T0.""SLength1"",
                                T0.""SVolume""
                        )
                        UNION ALL
                        (
                            SELECT 
                                T0.""ItemCode"", 
                                T0.""ItemName"", 
                                '300' AS ""WhsCode"", 
                                T3.""ItmsGrpNam"", 
                                T2.""Price"" AS ""UnitPrice"",
                                (T2.""Price"" * T0.""NumInSale"") 
                                    + CASE 
                                        WHEN LEFT(T0.""ItemName"", 3) = 'RET' AND T0.""ItemName"" LIKE '%CocaCola%' THEN 196
                                        WHEN LEFT(T0.""ItemName"", 3) = 'RET' THEN 50
                                        ELSE 0
                                      END AS ""BoxPrice"",
                                ROUND((SUM(T1.""OnHand"")/T0.""NumInSale""),0) AS ""Stock"",
                                T0.""NumInSale"",
                                CASE 
                                    WHEN LEFT(T0.""ItemName"", 3) = 'RET' THEN 1 
                                    ELSE 0 
                                END AS ""EsRET"",
                                T0.""SWeight1"", 
                                T0.""SWidth1"",
                                T0.""SHeight1"",
                                T0.""SLength1"",
                                T0.""SVolume""
                            FROM OITM T0
                            INNER JOIN OITW T1 ON T0.""ItemCode"" = T1.""ItemCode""
                            INNER JOIN ITM1 T2 ON T0.""ItemCode"" = T2.""ItemCode""
                            INNER JOIN OITB T3 ON T0.""ItmsGrpCod"" = T3.""ItmsGrpCod""
                            WHERE 
                                T0.""validFor"" = 'Y'
                                AND T2.""PriceList"" = 1
                                AND T1.""WhsCode"" = ?
                            GROUP BY 
                                T0.""ItemCode"", 
                                T0.""ItemName"", 
                                T3.""ItmsGrpNam"", 
                                T2.""Price"",
                                T0.""NumInSale"",
                                T0.""SWeight1"", 
                                T0.""SWidth1"",
                                T0.""SHeight1"",
                                T0.""SLength1"",
                                T0.""SVolume""
                        )
                        ORDER BY ""ItemCode"", ""WhsCode""";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("", branch);
                    cmd.Parameters.AddWithValue("", branch);

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new WoocommerceStockComplete
                            {
                                ItemCode = reader["ItemCode"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                WhsCode = reader["WhsCode"].ToString(),
                                ItmsGrpNam = reader["ItmsGrpNam"].ToString(),
                                UnitPrice = Math.Round(GetValueOrDefault(reader, "UnitPrice", 0m), 2),
                                BoxPrice = Math.Round(GetValueOrDefault(reader, "BoxPrice", 0m), 2),
                                Stock = Math.Round(GetValueOrDefault(reader, "Stock", 0m), 2),
                                NumInSale = GetValueOrDefault(reader, "NumInSale", 0m),
                                EsRET = GetValueOrDefault(reader, "EsRET", 0),
                                SWeight1 = GetValueOrDefault(reader, "SWeight1", 0m),
                                SWidth1 = GetValueOrDefault(reader, "SWidth1", 0m),
                                SHeight1 = GetValueOrDefault(reader, "SHeight1", 0m),
                                SLength1 = GetValueOrDefault(reader, "SLength1", 0m),
                                SVolume = GetValueOrDefault(reader, "SVolume", 0m)
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<WoocommerceStockComplete> GetAllStockByBranchByProduct(int environment, string branch, string item)
        {
            WoocommerceStockComplete result = null;

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                        (
                            SELECT 
                                T0.""ItemCode"", 
                                T0.""ItemName"", 
                                T1.""WhsCode"", 
                                T3.""ItmsGrpNam"", 
                                T2.""Price"" AS ""UnitPrice"",
                                (T2.""Price"" * T0.""NumInSale"") AS ""BoxPrice"",
                                ROUND((SUM(T1.""OnHand"")/T0.""NumInSale""),0) AS ""Stock"",
                                T0.""NumInSale"",
                                CASE 
                                    WHEN LEFT(T0.""ItemName"", 3) = 'RET' THEN 1 
                                    ELSE 0 
                                END AS ""EsRET"",
                                T0.""SWeight1"", 
                                T0.""SWidth1"",
                                T0.""SHeight1"",
                                T0.""SLength1"",
                                T0.""SVolume""
                            FROM OITM T0
                            INNER JOIN OITW T1 ON T0.""ItemCode"" = T1.""ItemCode""
                            INNER JOIN ITM1 T2 ON T0.""ItemCode"" = T2.""ItemCode""
                            INNER JOIN OITB T3 ON T0.""ItmsGrpCod"" = T3.""ItmsGrpCod""
                            WHERE 
                                T0.""validFor"" = 'Y'
                                AND T2.""PriceList"" = 1
                                AND (? <> '300' AND T1.""WhsCode"" = ?)
                                AND T0.""ItemCode"" = ?
                            GROUP BY 
                                T0.""ItemCode"", 
                                T0.""ItemName"", 
                                T1.""WhsCode"", 
                                T3.""ItmsGrpNam"", 
                                T2.""Price"",
                                T0.""NumInSale"",
                                T0.""SWeight1"", 
                                T0.""SWidth1"",
                                T0.""SHeight1"",
                                T0.""SLength1"",
                                T0.""SVolume""
                        )
                        UNION ALL
                        (
                            SELECT 
                                T0.""ItemCode"", 
                                T0.""ItemName"", 
                                '300' AS ""WhsCode"", 
                                T3.""ItmsGrpNam"", 
                                T2.""Price"" AS ""UnitPrice"",
                                (T2.""Price"" * T0.""NumInSale"") 
                                    + CASE 
                                        WHEN LEFT(T0.""ItemName"", 3) = 'RET' AND T0.""ItemName"" LIKE '%CocaCola%' THEN 196
                                        WHEN LEFT(T0.""ItemName"", 3) = 'RET' THEN 50
                                        ELSE 0
                                      END AS ""BoxPrice"",
                                ROUND((SUM(T1.""OnHand"")/T0.""NumInSale""),0) AS ""Stock"",
                                T0.""NumInSale"",
                                CASE 
                                    WHEN LEFT(T0.""ItemName"", 3) = 'RET' THEN 1 
                                    ELSE 0 
                                END AS ""EsRET"",
                                T0.""SWeight1"", 
                                T0.""SWidth1"",
                                T0.""SHeight1"",
                                T0.""SLength1"",
                                T0.""SVolume""
                            FROM OITM T0
                            INNER JOIN OITW T1 ON T0.""ItemCode"" = T1.""ItemCode""
                            INNER JOIN ITM1 T2 ON T0.""ItemCode"" = T2.""ItemCode""
                            INNER JOIN OITB T3 ON T0.""ItmsGrpCod"" = T3.""ItmsGrpCod""
                            WHERE 
                                T0.""validFor"" = 'Y'
                                AND T2.""PriceList"" = 1
                                AND (? = '300' AND T1.""WhsCode"" IN ('300','301','309','311','313'))
                                AND T0.""ItemCode"" = ?
                            GROUP BY 
                                T0.""ItemCode"", 
                                T0.""ItemName"", 
                                T3.""ItmsGrpNam"", 
                                T2.""Price"",
                                T0.""NumInSale"",
                                T0.""SWeight1"", 
                                T0.""SWidth1"",
                                T0.""SHeight1"",
                                T0.""SLength1"",
                                T0.""SVolume""
                        )
                        ORDER BY ""ItemCode"", ""WhsCode""";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("", branch);
                    cmd.Parameters.AddWithValue("", branch);
                    cmd.Parameters.AddWithValue("", item);

                    cmd.Parameters.AddWithValue("", branch);
                    cmd.Parameters.AddWithValue("", item);

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        if (await reader.ReadAsync())
                        {
                            result = new WoocommerceStockComplete
                            {
                                ItemCode = reader["ItemCode"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                WhsCode = reader["WhsCode"].ToString(),
                                ItmsGrpNam = reader["ItmsGrpNam"].ToString(),
                                UnitPrice = Math.Round(GetValueOrDefault(reader, "UnitPrice", 0m), 2),
                                BoxPrice = Math.Round(GetValueOrDefault(reader, "BoxPrice", 0m), 2),
                                Stock = Math.Round(GetValueOrDefault(reader, "Stock", 0m), 2),
                                NumInSale = GetValueOrDefault(reader, "NumInSale", 0m),
                                EsRET = GetValueOrDefault(reader, "EsRET", 0),
                                SWeight1 = GetValueOrDefault(reader, "SWeight1", 0m),
                                SWidth1 = GetValueOrDefault(reader, "SWidth1", 0m),
                                SHeight1 = GetValueOrDefault(reader, "SHeight1", 0m),
                                SLength1 = GetValueOrDefault(reader, "SLength1", 0m),
                                SVolume = GetValueOrDefault(reader, "SVolume", 0m)
                            };
                        }
                    }
                }
            }

            return result;
        }
    }
}
