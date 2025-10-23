using Oraculo.Models;
using Sap.Data.Hana;
using Oraculo.Dictionary;

namespace Oraculo.Data.Repositories
{
    public class BranchManagersRepository : IBranchManagersRepository
    {
        private readonly Func<int, HanaConnection> _connectionString;

        public BranchManagersRepository(Func<int, HanaConnection> connectionString)
        {
            _connectionString = connectionString;
        }

        protected HanaConnection dbConnection(int environment)
        {
            return _connectionString(environment);
        }

        private static T GetValueOrDefault<T>(HanaDataReader reader, string column, T defaultValue)
        {
            var value = reader[column];
            if (value == DBNull.Value || value == null)
                return defaultValue;

            // Manejo especial para DateTime -> DateTime?
            if (typeof(T) == typeof(DateTime?))
                return (T)(object)Convert.ToDateTime(value);

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public async Task<List<Dictionary<string, object>>> GetStockResupply(int environment, string grupo, string sucursal)
        {
            var result = new List<Dictionary<string, object>>();
            var familyWarehouses = BinMapsFamilyWarehouse.Get();

            if (!familyWarehouses.ContainsKey(grupo.ToUpper()))
                throw new Exception($"El grupo {grupo} no tiene almacenes configurados.");

            var almacenes = familyWarehouses[grupo.ToUpper()];

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                // Armar columnas dinámicas para cada almacén
                var stockColumns = string.Join(",\n", almacenes.Select(a =>
                    $@"COALESCE(
                (SELECT T3.""OnHand""/T2.""NumInSale""
                     FROM ""OITW"" T3
                     WHERE T3.""ItemCode"" = T0.""ItemCode""
                       AND T3.""WhsCode"" = '{a}'), 0
                    ) AS ""Stock {a}"""));

                string query = $@"
                                SELECT 
                                    CASE T0.""WhsCode""
                                        WHEN '309' THEN 'Mandarina'
                                        WHEN '311' THEN 'Mercado'
                                        WHEN '313' THEN 'Granadilla'
                                        WHEN '315' THEN 'Base Aérea'
                                        WHEN '317' THEN 'Tlajomulco'
                                        WHEN '319' THEN '8 de Julio'
                                        WHEN '321' THEN 'Juan de la Barrera'
                                        WHEN '325' THEN 'Chavez Carrillo'
                                        WHEN '327' THEN 'Niños Héroes'
                                        WHEN '329' THEN 'Tecoman'
                                        WHEN '323' THEN 'Ciudad Guzmán'
                                        WHEN '331' THEN 'Manzanillo'
                                        WHEN '303' THEN 'Cedis Colima'
                                        WHEN '334' THEN 'Villa de Alvarez'
                                    END AS ""Sucursal"",
                                    T0.""ItemCode"" AS ""Número"",
                                    T2.""ItemName"",
                                    ROUND((T0.""OnHand""/T2.""NumInSale"" - T0.""MaxStock""/T2.""NumInSale"") * -1 ,0) AS ""Resurtir"",
                                    {stockColumns},
                                    ROUND(T0.""OnHand""/T2.""NumInSale"",0) AS ""StockSucursal"",
                                    ROUND(T0.""MinStock""/T2.""NumInSale"",0) AS ""Minimo7DiasVenta"",
                                    ROUND(T0.""MaxStock""/T2.""NumInSale"",0) AS ""Maximo15DiasVenta"",
                                    (
                                        SELECT MAX(CAST(S.""DocDate"" AS DATE))
                                        FROM (
                                            SELECT 
                                                V.""ItemCode"",
                                                V.""LocCode"" AS ""WhsCode"",
                                                CAST(V.""DocDate"" AS DATE) AS ""DocDate"",
                                                SUM(V.""InQty"" - V.""OutQty"") 
                                                  OVER (PARTITION BY V.""ItemCode"", V.""LocCode"" ORDER BY V.""DocDate"", V.""TransSeq"") AS ""StockAcumulado""
                                            FROM ""OIVL"" V
                                        ) S
                                        WHERE S.""ItemCode"" = T0.""ItemCode""
                                          AND S.""WhsCode"" = T0.""WhsCode""
                                          AND S.""StockAcumulado"" < T0.""MinStock""
                                    ) AS ""UltimaVezBajoMinimo"",
                                    CASE 
                                        WHEN T0.""OnHand"" < T0.""MinStock"" THEN
                                            CASE 
                                                WHEN (
                                                    SELECT MAX(CAST(S.""DocDate"" AS DATE))
                                                    FROM (
                                                        SELECT 
                                                            V.""ItemCode"",
                                                            V.""LocCode"" AS ""WhsCode"",
                                                            CAST(V.""DocDate"" AS DATE) AS ""DocDate"",
                                                            SUM(V.""InQty"" - V.""OutQty"") 
                                                              OVER (PARTITION BY V.""ItemCode"", V.""LocCode"" ORDER BY V.""DocDate"", V.""TransSeq"") AS ""StockAcumulado""
                                                        FROM ""OIVL"" V
                                                    ) S
                                                    WHERE S.""ItemCode"" = T0.""ItemCode""
                                                      AND S.""WhsCode"" = T0.""WhsCode""
                                                      AND S.""StockAcumulado"" < T0.""MinStock""
                                                ) = CURRENT_DATE 
                                                THEN 0
                                                ELSE DAYS_BETWEEN(
                                                        CURRENT_DATE,
                                                        COALESCE(
                                                            (
                                                                SELECT MAX(CAST(S.""DocDate"" AS DATE))
                                                                FROM (
                                                                    SELECT 
                                                                        V.""ItemCode"",
                                                                        V.""LocCode"" AS ""WhsCode"",
                                                                        CAST(V.""DocDate"" AS DATE) AS ""DocDate"",
                                                                        SUM(V.""InQty"" - V.""OutQty"") 
                                                                          OVER (PARTITION BY V.""ItemCode"", V.""LocCode"" ORDER BY V.""DocDate"", V.""TransSeq"") AS ""StockAcumulado""
                                                                    FROM ""OIVL"" V
                                                                ) S
                                                                WHERE S.""ItemCode"" = T0.""ItemCode""
                                                                  AND S.""WhsCode"" = T0.""WhsCode""
                                                                  AND S.""StockAcumulado"" < T0.""MinStock""
                                                            ), CURRENT_DATE
                                                        )
                                                    ) * -1
                                            END
                                        ELSE NULL
                                    END AS ""UltimoDiaArribaMinimo""
                                FROM ""OITW"" T0
                                JOIN ""OITM"" T2 ON T0.""ItemCode"" = T2.""ItemCode""
                                INNER JOIN OITB T4 ON T2.""ItmsGrpCod"" = T4.""ItmsGrpCod""
                                WHERE T0.""OnHand"" < T0.""MinStock""
                                  AND T4.""ItmsGrpNam"" = ?
                                  AND T0.""WhsCode"" = ?
                                  AND EXISTS (
                                      SELECT 1 
                                      FROM ""OITW"" T3
                                      WHERE T3.""ItemCode"" = T0.""ItemCode""
                                        AND T3.""WhsCode"" IN ({string.Join(",", almacenes.Select(a => $"'{a}'"))})
                                        AND T3.""OnHand"" > 0
                                  )
                                ORDER BY T0.""WhsCode"", ""Resurtir"" DESC;
                            ";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("", grupo);
                    cmd.Parameters.AddWithValue("", sucursal);

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dto = new Dictionary<string, object>
                            {
                                ["itemCode"] = reader["Número"].ToString(),
                                ["itemName"] = reader["ItemName"].ToString(),
                                ["sucursal"] = reader["Sucursal"].ToString(),
                                ["resurtir"] = GetValueOrDefault(reader, "Resurtir", 0m),
                                ["stockSucursal"] = GetValueOrDefault(reader, "StockSucursal", 0m),
                                ["minimo7DiasVenta"] = GetValueOrDefault(reader, "Minimo7DiasVenta", 0m),
                                ["maximo15DiasVenta"] = GetValueOrDefault(reader, "Maximo15DiasVenta", 0m),
                                ["ultimaVezBajoMinimo"] = GetValueOrDefault(reader, "UltimaVezBajoMinimo", (DateTime?)null),
                                ["ultimoDiaArribaMinimo"] = GetValueOrDefault(reader, "UltimoDiaArribaMinimo", 0)
                            };

                            // agregar dinámicamente cada stock de cedis como propiedad separada
                            foreach (var alm in almacenes)
                            {
                                var colName = $"Stock {alm}";
                                if (!reader.IsDBNull(reader.GetOrdinal(colName)))
                                    dto[$"stockAlmacen{alm}"] = Convert.ToDecimal(reader[colName]);
                            }

                            result.Add(dto);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> GetStockZeroResupply(int environment, string family)
        {
            var result = new List<Dictionary<string, object>>();
            var familyWarehouses = BinMapsFamilyWarehouse.Get();

            if (!familyWarehouses.ContainsKey(family.ToUpper()))
                throw new Exception($"El grupo {family} no tiene almacenes configurados.");

            var almacenes = familyWarehouses[family.ToUpper()];

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                var stockColumns = string.Join(",\n", almacenes.Select(a =>
                    $"SUM(CASE WHEN T0.\"WhsCode\" = '{a}' THEN T0.\"OnHand\" ELSE 0 END) AS \"Stock_{a}\""));

                string query = $@"
                                SELECT 
                                    T0.""ItemCode"" AS ""ItemCode"",
                                    T2.""ItemName"",
                                    {stockColumns}
                                FROM ""OITW"" T0
                                JOIN ""OITM"" T2 ON T0.""ItemCode"" = T2.""ItemCode""
                                INNER JOIN ""OITB"" T4 ON T2.""ItmsGrpCod"" = T4.""ItmsGrpCod""
                                WHERE T4.""ItmsGrpNam"" = ?
                                  AND T2.""frozenFor"" = 'N'
                                  AND T0.""WhsCode"" IN ({string.Join(",", almacenes.Select(a => $"'{a}'"))})
                                GROUP BY T0.""ItemCode"", T2.""ItemName""
                                HAVING SUM(T0.""OnHand"") = 0";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    // 👇 Importante: sin nombre, en el mismo orden que los ?
                    cmd.Parameters.AddWithValue("", family);

                    using (HanaDataReader reader = (HanaDataReader)await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dto = new Dictionary<string, object>
                            {
                                ["itemCode"] = reader["ItemCode"].ToString(),
                                ["itemName"] = reader["ItemName"].ToString()
                            };

                            foreach (var whs in almacenes)
                            {
                                var colName = $"Stock_{whs}";
                                dto[$"stock{whs}"] = GetValueOrDefault(reader, colName, 0m);
                            }

                            result.Add(dto);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> GetLast45DaysByFamily(int environment, string family)
        {
            var result = new List<Dictionary<string, object>>();

            string query = @"
                            WITH Movs AS (
                                SELECT 
                                    W1.""ItemCode"",
                                    SUM(CASE 
                                            WHEN T0.""Filler"" IN ('300','301') 
                                             AND T0.""DocDate"" >= ADD_DAYS(CURRENT_DATE, -15)
                                            THEN W1.""Quantity"" ELSE 0 
                                        END) AS mov_15,
                                    SUM(CASE 
                                            WHEN T0.""Filler"" IN ('300','301') 
                                             AND T0.""DocDate"" >= ADD_DAYS(CURRENT_DATE, -45)
                                            THEN W1.""Quantity"" ELSE 0 
                                        END) AS mov_45
                                FROM ""OWTR"" T0
                                JOIN ""WTR1"" W1 ON T0.""DocEntry"" = W1.""DocEntry""
                                GROUP BY W1.""ItemCode""
                            ),
                            Stocks AS (
                                SELECT 
                                    ""ItemCode"",
                                    SUM(CASE WHEN ""WhsCode"" = '300' THEN ""OnHand"" ELSE 0 END) AS onhand_300,
                                    SUM(CASE WHEN ""WhsCode"" = '301' THEN ""OnHand"" ELSE 0 END) AS onhand_301,
                                    SUM(CASE WHEN ""WhsCode"" = '305' THEN ""OnHand"" ELSE 0 END) AS onhand_305,
                                    SUM(CASE WHEN ""WhsCode"" = '336' THEN ""OnHand"" ELSE 0 END) AS onhand_336
                                FROM ""OITW""
                                WHERE ""WhsCode"" IN ('300','301','305','336')
                                GROUP BY ""ItemCode""
                            )
                            SELECT
                                i.""ItemCode"",
                                i.""ItemName"",
                                b.""ItmsGrpNam"" AS ""Familia"",
                                ROUND(COALESCE(m.mov_15 / NULLIF(i.""NumInBuy"",0), 0), 2) AS ""CedisAbastos15Dias"",
                                ROUND(COALESCE(m.mov_45 / NULLIF(i.""NumInBuy"",0), 0), 2) AS ""CedisAbastos45Dias"",
                                ROUND((COALESCE(s.onhand_300,0) + COALESCE(s.onhand_301,0)) / NULLIF(i.""NumInBuy"",0), 2) AS ""StockCedis"",
                                ROUND(COALESCE(s.onhand_305,0) / NULLIF(i.""NumInBuy"",0), 2) AS ""ExistenciaCorporativo"",
                                ROUND(COALESCE(s.onhand_336,0) / NULLIF(i.""NumInBuy"",0), 2) AS ""Existencia14"",
                                ROUND(
                                    (COALESCE(s.onhand_300,0) + COALESCE(s.onhand_301,0) + COALESCE(s.onhand_305,0) + COALESCE(s.onhand_336,0))
                                    / NULLIF(i.""NumInBuy"",0), 2
                                ) AS ""TotalStock"",
                                ROUND(
                                    COALESCE(m.mov_45 / NULLIF(i.""NumInBuy"",0), 0)
                                    -
                                    (
                                        (COALESCE(s.onhand_300,0) + 
                                         COALESCE(s.onhand_301,0) + 
                                         COALESCE(s.onhand_305,0) + 
                                         COALESCE(s.onhand_336,0)
                                        ) / NULLIF(i.""NumInBuy"",0)
                                    ), 2
                                ) AS ""DiferenciaMax45StockTotal""
                            FROM ""OITM"" i
                            JOIN ""OITB"" b ON i.""ItmsGrpCod"" = b.""ItmsGrpCod""
                            LEFT JOIN Movs m ON m.""ItemCode"" = i.""ItemCode""
                            LEFT JOIN Stocks s ON s.""ItemCode"" = i.""ItemCode""
                            WHERE b.""ItmsGrpNam"" = ?
                              AND i.""frozenFor"" = 'N'
                            ORDER BY i.""ItemCode"";";

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("", family);

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dto = new Dictionary<string, object>
                            {
                                ["itemCode"] = reader["ItemCode"].ToString(),
                                ["itemName"] = reader["ItemName"].ToString(),
                                ["familia"] = reader["Familia"].ToString(),
                                ["cedisAbastos15Dias"] = GetValueOrDefault(reader, "CedisAbastos15Dias", 0m),
                                ["cedisAbastos45Dias"] = GetValueOrDefault(reader, "CedisAbastos45Dias", 0m),
                                ["stockCedis"] = GetValueOrDefault(reader, "StockCedis", 0m),
                                ["existenciaCorporativo"] = GetValueOrDefault(reader, "ExistenciaCorporativo", 0m),
                                ["existencia14"] = GetValueOrDefault(reader, "Existencia14", 0m),
                                ["totalStock"] = GetValueOrDefault(reader, "TotalStock", 0m),
                                ["diferenciaMax45StockTotal"] = GetValueOrDefault(reader, "DiferenciaMax45StockTotal", 0m)
                            };

                            result.Add(dto);
                        }
                    }
                }
            }

            return result;
        }

    }
}
