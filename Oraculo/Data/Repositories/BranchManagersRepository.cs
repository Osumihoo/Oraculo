using Oraculo.Dictionary;
using Oraculo.Models;
using Sap.Data.Hana;
using System.Text.RegularExpressions;

namespace Oraculo.Data.Repositories
{
    public class BranchManagersRepository :  IBranchManagersRepository
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
                                        WHEN '338' THEN 'Tlajomulco Centro'
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
                                    CASE 
                                        WHEN ROUND((T0.""OnHand""/T2.""NumInSale"" - T0.""MaxStock""/T2.""NumInSale"") * -1 ,0) >= 
                                             COALESCE((
                                                 SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                                 FROM ""@SO1_01VENTA"" V
                                                 INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                                     ON V.""Name"" = T1.""U_SO1_FOLIO""
                                                 WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                                   AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                                                   AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                             ), 0) -T0.""OnHand""/T2.""NumInSale""
                                        THEN ROUND((T0.""OnHand""/T2.""NumInSale"" - T0.""MaxStock""/T2.""NumInSale"") * -1 ,0)
                                        ELSE COALESCE((
                                                 SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                                 FROM ""@SO1_01VENTA"" V
                                                 INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                                     ON V.""Name"" = T1.""U_SO1_FOLIO""
                                                 WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                                   AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                                                   AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                             ), 0) - T0.""OnHand""/T2.""NumInSale""
                                    END AS ""Resurtir"",
                                    {stockColumns},
                                    ROUND(T0.""OnHand""/T2.""NumInSale"",0) AS ""StockSucursal"",
                                    ROUND(T0.""MinStock""/T2.""NumInSale"",0) AS ""Minimo7DiasVenta"",
                                    CASE 
                                        WHEN ROUND((T0.""MaxStock""/T2.""NumInSale"") ,0) >= 
                                             COALESCE((
                                                 SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                                 FROM ""@SO1_01VENTA"" V
                                                 INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                                     ON V.""Name"" = T1.""U_SO1_FOLIO""
                                                 WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                                   AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                                                   AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                             ), 0)
                                        THEN ROUND((T0.""MaxStock""/T2.""NumInSale""),0)
                                        ELSE COALESCE((
                                                 SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                                 FROM ""@SO1_01VENTA"" V
                                                 INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                                     ON V.""Name"" = T1.""U_SO1_FOLIO""
                                                 WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                                   AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                                                   AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                             ), 0)
                                    END AS ""Maximo15DiasVenta"",
                                    CASE
                                        WHEN T0.""OnHand"" < T0.""MinStock""
                                        THEN (
                                            SELECT MAX(CAST(S.""DocDate"" AS DATE))
                                            FROM (
                                                SELECT 
                                                    V.""ItemCode"",
                                                    V.""LocCode"" AS ""WhsCode"",
                                                    CAST(V.""DocDate"" AS DATE) AS ""DocDate"",
                                                    SUM(V.""InQty"" - V.""OutQty"") 
                                                      OVER (
                                                          PARTITION BY V.""ItemCode"", V.""LocCode""
                                                          ORDER BY V.""DocDate"", V.""TransSeq""
                                                      ) AS ""StockAcumulado""
                                                FROM ""OIVL"" V
                                            ) S
                                            WHERE S.""ItemCode"" = T0.""ItemCode""
                                              AND S.""WhsCode"" = T0.""WhsCode""
                                              AND S.""StockAcumulado"" < T0.""MinStock""
                                        )
                                        ELSE NULL
                                    END AS ""UltimaVezBajoMinimo"",
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
                                WHERE  T4.""ItmsGrpNam"" = ?
                                  AND T0.""WhsCode"" = ?
                                  AND EXISTS (
                                      SELECT 1 
                                      FROM ""OITW"" T3
                                      WHERE T3.""ItemCode"" = T0.""ItemCode""
                                        AND T3.""WhsCode"" IN ({string.Join(",", almacenes.Select(a => $"'{a}'"))})
                                  )
                                  AND COALESCE((
                                      SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                      FROM ""@SO1_01VENTA"" V
                                      INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                          ON V.""Name"" = T1.""U_SO1_FOLIO""
                                      WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                        AND T2.""frozenFor"" = 'N'

                                        AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                  ), 0) > 0
                                ORDER BY T0.""WhsCode"", ""Resurtir"" DESC;
                            ";
                //AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15) , esto va en el espacio vacío del where por si se quiere volver a mostrar SOLO los productos que hayan vendido algo en los últimos 15 días

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
                            //foreach (var alm in almacenes)
                            //{
                            //    var colName = $"Stock {alm}";
                            //    if (!reader.IsDBNull(reader.GetOrdinal(colName)))
                            //        dto[$"stockAlmacen{alm}"] = Convert.ToDecimal(reader[colName]);
                            //}

                            // Variables para acumular los CEDIS Abastos
                            decimal cedisAbastos = 0;

                            // Recorremos los almacenes (por código)
                            foreach (var alm in almacenes)
                            {
                                var colName = $"Stock {alm}";
                                if (!reader.IsDBNull(reader.GetOrdinal(colName)))
                                {
                                    var valor = Convert.ToDecimal(reader[colName]);

                                    // Si el almacén es 300 o 301 → CEDIS ABASTOS
                                    if (alm == "300" || alm == "301")
                                    {
                                        cedisAbastos += valor;
                                    }
                                    else
                                    {
                                        dto[$"stockAlmacen{alm}"] = valor;
                                    }
                                }
                            }

                            // Agregamos la suma de los CEDIS Abastos
                            dto["stockAlmacen299"] = cedisAbastos;

                            result.Add(dto);
                        }
                    }
                }
            }

            return result;
        }
        public async Task<List<Dictionary<string, object>>> GetStockResupplyWOCV(int environment, string sucursal)
        {
            var result = new List<Dictionary<string, object>>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        CASE T0.""WhsCode""
                            WHEN '309' THEN 'Mandarina'
                            WHEN '311' THEN 'Mercado'
                            WHEN '313' THEN 'Granadilla'
                            WHEN '315' THEN 'Base Aérea'
                            WHEN '317' THEN 'Tlajomulco'
                            WHEN '338' THEN 'Tlajomulco Centro'
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

                        CASE 
                            WHEN ROUND((T0.""OnHand""/T2.""NumInSale"" - T0.""MaxStock""/T2.""NumInSale"") * -1 ,0) >= 
                                 COALESCE((
                                     SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                     FROM ""@SO1_01VENTA"" V
                                     INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                         ON V.""Name"" = T1.""U_SO1_FOLIO""
                                     WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                       AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                                       AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                 ), 0) - T0.""OnHand""/T2.""NumInSale""
                            THEN ROUND((T0.""OnHand""/T2.""NumInSale"" - T0.""MaxStock""/T2.""NumInSale"") * -1 ,0)
                            ELSE COALESCE((
                                     SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                     FROM ""@SO1_01VENTA"" V
                                     INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                         ON V.""Name"" = T1.""U_SO1_FOLIO""
                                     WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                       AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                                       AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                 ), 0) - T0.""OnHand""/T2.""NumInSale""
                        END AS ""Resurtir"",

                        -- stocks fijos por almacén (sin columna dinámica)
                        COALESCE((SELECT T3.""OnHand""/T2.""NumInSale"" FROM ""OITW"" T3 WHERE T3.""ItemCode"" = T0.""ItemCode"" AND T3.""WhsCode"" = '309'), 0) AS ""Stock 309"",
                        COALESCE((SELECT T3.""OnHand""/T2.""NumInSale"" FROM ""OITW"" T3 WHERE T3.""ItemCode"" = T0.""ItemCode"" AND T3.""WhsCode"" = '300'), 0) AS ""Stock 300"",
                        COALESCE((SELECT T3.""OnHand""/T2.""NumInSale"" FROM ""OITW"" T3 WHERE T3.""ItemCode"" = T0.""ItemCode"" AND T3.""WhsCode"" = '301'), 0) AS ""Stock 301"",
                        COALESCE((SELECT T3.""OnHand""/T2.""NumInSale"" FROM ""OITW"" T3 WHERE T3.""ItemCode"" = T0.""ItemCode"" AND T3.""WhsCode"" = '305'), 0) AS ""Stock 305"",
                        COALESCE((SELECT T3.""OnHand""/T2.""NumInSale"" FROM ""OITW"" T3 WHERE T3.""ItemCode"" = T0.""ItemCode"" AND T3.""WhsCode"" = '336'), 0) AS ""Stock 336"",

                        ROUND(T0.""OnHand""/T2.""NumInSale"",0) AS ""StockSucursal"",
                        ROUND(T0.""MinStock""/T2.""NumInSale"",0) AS ""Minimo7DiasVenta"",

                        CASE 
                            WHEN ROUND((T0.""MaxStock""/T2.""NumInSale"") ,0) >= 
                                 COALESCE((
                                     SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                     FROM ""@SO1_01VENTA"" V
                                     INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                         ON V.""Name"" = T1.""U_SO1_FOLIO""
                                     WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                       AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                                       AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                 ), 0)
                            THEN ROUND((T0.""MaxStock""/T2.""NumInSale""),0)
                            ELSE COALESCE((
                                     SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                                     FROM ""@SO1_01VENTA"" V
                                     INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                                         ON V.""Name"" = T1.""U_SO1_FOLIO""
                                     WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                                       AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                                       AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                                 ), 0)
                        END AS ""Maximo15DiasVenta"",

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
                      AND T4.""ItmsGrpNam"" IN (
                            'BEBIDAS SIN ALCOHOL',
                            'ABARROTES',
                            'BEBIDAS CON ALCOHOL',
                            'MASCOTAS'
                      )
                      AND T0.""WhsCode"" = ?

                      AND EXISTS (
                          SELECT 1 
                          FROM ""OITW"" T3
                          WHERE T3.""ItemCode"" = T0.""ItemCode""
                            AND T3.""WhsCode"" IN ('309','300','301','305','336')
                      )

                      AND COALESCE((
                          SELECT SUM(T1.""U_SO1_CANTIDAD"") / T2.""NumInSale""
                          FROM ""@SO1_01VENTA"" V
                          INNER JOIN ""@SO1_01VENTADETALLE"" T1 
                              ON V.""Name"" = T1.""U_SO1_FOLIO""
                          WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                            AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                            AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                      ), 0) > 0

                    ORDER BY T0.""WhsCode"", ""Resurtir"" DESC;
                    ";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    // parámetro: sucursal (WhsCode)
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

                            // --- SUMA CEDIS ABASTOS (300 + 301) ---
                            decimal cedisAbastos =
                                GetValueOrDefault(reader, "Stock 300", 0m) +
                                GetValueOrDefault(reader, "Stock 301", 0m);

                            dto["stockAlmacen299"] = cedisAbastos;

                            // --- STOCK INDIVIDUAL DE LOS DEMÁS ALMACENES ---
                            dto["stockAlmacen309"] = GetValueOrDefault(reader, "Stock 309", 0m);
                            dto["stockAlmacen305"] = GetValueOrDefault(reader, "Stock 305", 0m);
                            dto["stockAlmacen336"] = GetValueOrDefault(reader, "Stock 336", 0m);

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

        public async Task<List<Dictionary<string, object>>> GetLast30DaysByFamily(int environment, string family)
        {
            var result = new List<Dictionary<string, object>>();

            string query = @"
                            WITH Movs AS (
                                SELECT
                                T1.""U_SO1_NUMEROARTICULO"" AS ""ItemCode"",
                                SUM(CASE 
                                        WHEN T0.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15) 
                                        THEN T1.""U_SO1_CANTIDAD"" * T1.""U_SO1_CANTUNIMEDINV""
                                        ELSE 0 
                                    END) AS mov_15,
                                SUM(CASE 
                                        WHEN T0.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -30) 
                                        THEN T1.""U_SO1_CANTIDAD"" * T1.""U_SO1_CANTUNIMEDINV""
                                        ELSE 0 
                                    END) AS mov_30
                            FROM ""@SO1_01VENTA"" T0
                            JOIN ""@SO1_01VENTADETALLE"" T1
                                ON T0.""Name"" = T1.""U_SO1_FOLIO""
                            WHERE T0.""U_SO1_TIPO"" IN ('CA','CR')
                            GROUP BY T1.""U_SO1_NUMEROARTICULO""
                            ),
                            Stocks AS (
                                SELECT 
                                    ""ItemCode"",
                                    SUM(CASE WHEN ""WhsCode"" = '300' THEN ""OnHand"" ELSE 0 END) AS onhand_300,
                                    SUM(CASE WHEN ""WhsCode"" = '301' THEN ""OnHand"" ELSE 0 END) AS onhand_301,
                                    SUM(CASE WHEN ""WhsCode"" = '305' THEN ""OnHand"" ELSE 0 END) AS onhand_305,
                                    SUM(CASE WHEN ""WhsCode"" = '336' THEN ""OnHand"" ELSE 0 END) AS onhand_336,
                                    SUM(CASE WHEN ""WhsCode"" = '309' THEN ""OnHand"" ELSE 0 END) AS onhand_309
                                FROM ""OITW""
                                WHERE ""WhsCode"" IN ('300','301','305','336','309')
                                GROUP BY ""ItemCode""
                            )
                            SELECT
                                i.""ItemCode"",
                                i.""ItemName"",
                                b.""ItmsGrpNam"" AS ""Familia"",
                                ROUND(COALESCE(m.mov_15 / NULLIF(i.""NumInBuy"",0), 0), 2) AS ""CedisAbastos15Dias"",
                                ROUND(COALESCE(m.mov_30 / NULLIF(i.""NumInBuy"",0), 0), 2) AS ""CedisAbastos30Dias"",
                                ROUND(COALESCE(m.mov_30 / NULLIF(i.""NumInBuy"",0), 0), 2)/30 AS ""CedisAbastos30DiasPromedio"",
                                ROUND((COALESCE(s.onhand_300,0) + COALESCE(s.onhand_301,0)) / NULLIF(i.""NumInBuy"",0), 2) AS ""StockCedis"",
                                ROUND(COALESCE(s.onhand_305,0) / NULLIF(i.""NumInBuy"",0), 2) AS ""ExistenciaCorporativo"",
                                ROUND(COALESCE(s.onhand_336,0) / NULLIF(i.""NumInBuy"",0), 2) AS ""Existencia14"",
                                CASE 
                                    WHEN b.""ItmsGrpNam"" IN ('BEBIDAS CON ALCOHOL','BEBIDAS SIN ALCOHOL','CIGARROS','ABARROTES')
                                    THEN ROUND(COALESCE(s.onhand_309,0) / NULLIF(i.""NumInBuy"",0), 2)
                                    ELSE NULL
                                END AS ""ExistenciaMandarina"",

                                ROUND((
                                    (COALESCE(s.onhand_300,0) + 
                                     COALESCE(s.onhand_301,0) + 
                                     COALESCE(s.onhand_305,0) + 
                                     COALESCE(s.onhand_336,0) +
                                     CASE 
                                         WHEN b.""ItmsGrpNam"" IN ('BEBIDAS CON ALCOHOL','BEBIDAS SIN ALCOHOL','CIGARROS','ABARROTES')
                                         THEN COALESCE(s.onhand_309,0)
                                         ELSE 0 
                                     END
                                    ) / NULLIF(i.""NumInBuy"",0)
                                ), 2) AS ""TotalStock"",

                                ROUND((
                                    COALESCE(m.mov_30 / NULLIF(i.""NumInBuy"",0), 0)
                                    -
                                    (
                                        (COALESCE(s.onhand_300,0) + 
                                         COALESCE(s.onhand_301,0) + 
                                         COALESCE(s.onhand_305,0) + 
                                         COALESCE(s.onhand_336,0) +
                                         CASE 
                                             WHEN b.""ItmsGrpNam"" IN ('BEBIDAS CON ALCOHOL','BEBIDAS SIN ALCOHOL','CIGARROS','ABARROTES')
                                             THEN COALESCE(s.onhand_309,0)
                                             ELSE 0 
                                         END
                                        ) / NULLIF(i.""NumInBuy"",0)
                                    )
                                ), 2) AS ""DiferenciaMax30StockTotal""

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
                                ["cedisAbastos30Dias"] = GetValueOrDefault(reader, "CedisAbastos30Dias", 0m),
                                ["cedisAbastos30DiasPromedio"] = GetValueOrDefault(reader, "CedisAbastos30DiasPromedio", 0m),
                                ["stockCedis"] = GetValueOrDefault(reader, "StockCedis", 0m),
                                ["existenciaCorporativo"] = GetValueOrDefault(reader, "ExistenciaCorporativo", 0m),
                                ["existencia14"] = GetValueOrDefault(reader, "Existencia14", 0m),
                                ["totalStock"] = GetValueOrDefault(reader, "TotalStock", 0m),
                                ["diferenciaMax30StockTotal"] = GetValueOrDefault(reader, "DiferenciaMax30StockTotal", 0m)
                            };

                            // Solo incluir existenciaMandarina si la familia es de las 4
                            var familiasConMandarina = new HashSet<string>
                            {
                                "BEBIDAS CON ALCOHOL",
                                "BEBIDAS SIN ALCOHOL",
                                "CIGARROS",
                                "ABARROTES"
                            };

                            if (familiasConMandarina.Contains(family?.Trim().ToUpperInvariant()))
                            {
                                dto["existenciaMandarina"] = GetValueOrDefault(reader, "ExistenciaMandarina", 0m);
                            }

                            result.Add(dto);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<List<BranchStockResupplyJuan>> GetBranchStockResupply(int environment)
        {
            var result = new List<BranchStockResupplyJuan>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT 
                    T0.""ItemCode"" AS ""Número"",
                    T2.""ItemName"",
                    T2.""ItmsGrpCod"",
                    CASE T0.""WhsCode""
                        WHEN '309' THEN 'Mandarina'
                        WHEN '311' THEN 'Mercado'
                        WHEN '313' THEN 'Granadilla'
                        WHEN '315' THEN 'Base Aérea'
                        WHEN '317' THEN 'Tlajomulco'
                        WHEN '338' THEN 'Tlajomulco Centro'
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
                    T2.""NumInSale"" AS ""PZAS X CAJA"",

                    /* -------- Venta últimos 15 días -------- */
                    COALESCE((
                        SELECT ROUND(SUM(T1.""U_SO1_CANTIDAD"" * T1.""U_SO1_CANTUNIMEDINV"") / T2.""NumInSale"", 0)
                        FROM ""@SO1_01VENTA"" V
                        INNER JOIN ""@SO1_01VENTADETALLE"" T1
                            ON V.""Name"" = T1.""U_SO1_FOLIO""
                        WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                          AND V.""U_SO1_TIPO"" IN ('CA','CR')
                          AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                          AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                    ), 0) AS ""Venta"",

                    /* -------- Stock Sucursal -------- */
                    ROUND(T0.""OnHand"" / T2.""NumInSale"", 0) AS ""Stock Sucursal"",

                    /* -------- Necesita (Venta 15 días - Stock) -------- */
                    COALESCE((
                        SELECT ROUND(
                            SUM(T1.""U_SO1_CANTIDAD"" * T1.""U_SO1_CANTUNIMEDINV"") / T2.""NumInSale"", 0
                        )
                        FROM ""@SO1_01VENTA"" V
                        INNER JOIN ""@SO1_01VENTADETALLE"" T1
                            ON V.""Name"" = T1.""U_SO1_FOLIO""
                        WHERE T1.""U_SO1_NUMEROARTICULO"" = T0.""ItemCode""
                          AND V.""U_SO1_TIPO"" IN ('CA','CR')
                          AND V.""U_SO1_FECHA"" >= ADD_DAYS(CURRENT_DATE, -15)
                          AND T1.""U_SO1_ALMACEN"" = T0.""WhsCode""
                    ), 0)
                    - ROUND(T0.""OnHand"" / T2.""NumInSale"", 0)
                    AS ""Necesita"",

                    /* -------- Stock Cedis GDL (300 + 301) -------- */
                    (
                        SELECT ROUND(SUM(TC.""OnHand"") / T2.""NumInSale"", 0)
                        FROM ""OITW"" TC
                        WHERE TC.""ItemCode"" = T0.""ItemCode""
                          AND TC.""WhsCode"" IN ('300','301')
                    ) AS ""Stock Cedis GDL"",

                    /* -------- Stock Calle 14 -------- */
                    (
                        SELECT ROUND(T302.""OnHand"" / T2.""NumInSale"", 0)
                        FROM ""OITW"" T302
                        WHERE T302.""ItemCode"" = T0.""ItemCode""
                          AND T302.""WhsCode"" = '337'
                    ) AS ""Stock Calle 14"",

                    /* -------- Stock Corporativo -------- */
                    (
                        SELECT ROUND(T305.""OnHand"" / T2.""NumInSale"", 0)
                        FROM ""OITW"" T305
                        WHERE T305.""ItemCode"" = T0.""ItemCode""
                          AND T305.""WhsCode"" = '305'
                    ) AS ""Stock Corporativo""

                FROM ""OITW"" T0
                JOIN ""OITM"" T2 ON T0.""ItemCode"" = T2.""ItemCode""
                INNER JOIN ""OITB"" T4 ON T2.""ItmsGrpCod"" = T4.""ItmsGrpCod""
                WHERE T2.""frozenFor"" = 'N'
                  AND T0.""WhsCode"" IN ('303','309','311','313','315','317','319','321','323','325','327','329','331','334','338')
                  AND T2.""ItmsGrpCod"" NOT IN ('108','109','110','111')
                ORDER BY T0.""ItemCode"", T0.""WhsCode"";
                ";



                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new BranchStockResupplyJuan
                            {
                                Numero = reader["Número"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                ItmsGrpCod = reader["ItmsGrpCod"].ToString(),
                                Sucursal = reader["Sucursal"].ToString(),
                                PzasPorCaja = GetValueOrDefault(reader, "PZAS X CAJA", 0m),
                                Venta = GetValueOrDefault(reader, "Venta", 0m),
                                StockSucursal = GetValueOrDefault(reader, "Stock Sucursal", 0m),
                                Necesita = GetValueOrDefault(reader, "Necesita", 0m),
                                StockCedisGdl = GetValueOrDefault(reader, "Stock Cedis GDL", 0m),
                                StockCalle14 = GetValueOrDefault(reader, "Stock Calle 14", 0m),
                                StockCorporativo = GetValueOrDefault(reader, "Stock Corporativo", 0m)
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<List<PriceList>> GetPriceList(int environment)
        {
            var result = new List<PriceList>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        T0.""ItemCode"", 
                        T0.""ItemName"",
                        MAX(CASE WHEN T1.""PriceList"" = '1' THEN T1.""Price"" END) AS ""PrecioArtGDL"",
                        MAX(CASE WHEN T1.""PriceList"" = '1' THEN T1.""Price"" * T0.""NumInSale"" END) AS ""PrecioCajaGDL"",
                        MAX(CASE WHEN T1.""PriceList"" = '2' THEN T1.""Price"" END) AS ""PrecioArtCOL"",
                        MAX(CASE WHEN T1.""PriceList"" = '2' THEN T1.""Price"" * T0.""NumInSale"" END) AS ""PrecioCajaCOL""
                    FROM 
                        ""SBO_ELVALOR_PRODUCTIVA"".""OITM"" T0
                    INNER JOIN 
                        ITM1 T1 ON T0.""ItemCode"" = T1.""ItemCode""
                    WHERE 
                        T0.""frozenFor"" = 'N'
                        AND T1.""PriceList"" IN ('1', '2')
                    GROUP BY 
                        T0.""ItemCode"", 
                        T0.""ItemName""
                    ORDER BY 
                        T0.""ItemCode"";
                ";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                using (HanaDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new PriceList
                        {
                            ItemCode = reader["ItemCode"].ToString(),
                            ItemName = reader["ItemName"].ToString(),

                            PrecioArtGDL = GetValueOrDefault(reader, "PrecioArtGDL", 0m),
                            PrecioCajaGDL = GetValueOrDefault(reader, "PrecioCajaGDL", 0m),

                            PrecioArtCOL = GetValueOrDefault(reader, "PrecioArtCOL", 0m),
                            PrecioCajaCOL = GetValueOrDefault(reader, "PrecioCajaCOL", 0m)
                        });
                    }
                }
            }

            return result;
        }

        public async Task<List<CostList>> GetCostList(int environment)
        {
            var result = new List<CostList>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT
                    T1.""ItemCode"",
                    T0.""ItemName"", 
                    T0.""LstEvlPric"" AS ""CostoSinImpuestos"",
                    T0.""NumInSale""  AS ""UMI"",
                    CASE 
                        WHEN T0.""TaxCodeAR"" = 'V00' THEN T0.""LstEvlPric""
                        WHEN T0.""TaxCodeAR"" = 'V00IEP8' THEN T0.""LstEvlPric"" * 1.08 
                        WHEN T0.""TaxCodeAR"" = 'V16' THEN T0.""LstEvlPric"" * 1.16        
                        WHEN T0.""TaxCodeAR"" = 'V16IEP26' THEN T0.""LstEvlPric"" * 1.16 * 1.265 
                        WHEN T0.""TaxCodeAR"" = 'V16IEP30' THEN T0.""LstEvlPric"" * 1.16 * 1.30 
                        WHEN T0.""TaxCodeAR"" = 'V16IEP53' THEN T0.""LstEvlPric"" * 1.16 * 1.53 
                        ELSE T0.""LstEvlPric""
                    END AS ""CostoConImpuestos"",
                    CASE 
                        WHEN T0.""TaxCodeAR"" = 'V00' THEN T0.""LstEvlPric"" * T0.""NumInSale""
                        WHEN T0.""TaxCodeAR"" = 'V00IEP8' THEN T0.""LstEvlPric"" * 1.08 * T0.""NumInSale""
                        WHEN T0.""TaxCodeAR"" = 'V16' THEN T0.""LstEvlPric"" * 1.16 * T0.""NumInSale""
                        WHEN T0.""TaxCodeAR"" = 'V16IEP26' THEN T0.""LstEvlPric"" * 1.16 * 1.265 * T0.""NumInSale""
                        WHEN T0.""TaxCodeAR"" = 'V16IEP30' THEN T0.""LstEvlPric"" * 1.16 * 1.30 * T0.""NumInSale""
                        WHEN T0.""TaxCodeAR"" = 'V16IEP53' THEN T0.""LstEvlPric"" * 1.16 * 1.53 * T0.""NumInSale""
                        ELSE T0.""LstEvlPric"" * T0.""NumInSale""
                    END AS ""CostoCaja""
                FROM OITM T0
                INNER JOIN ITM1 T1 ON T0.""ItemCode"" = T1.""ItemCode""
                WHERE T0.""frozenFor"" = 'N'
                AND T1.""PriceList"" = '1'
                ";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                using (HanaDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new CostList
                        {
                            ItemCode = reader["ItemCode"].ToString(),
                            ItemName = reader["ItemName"].ToString(),

                            CostoSinImpuestos = GetValueOrDefault(reader, "CostoSinImpuestos", 0m),
                            UMI = GetValueOrDefault(reader, "UMI", 0m),

                            CostoConImpuestos = GetValueOrDefault(reader, "CostoConImpuestos", 0m),
                            CostoCaja = GetValueOrDefault(reader, "CostoCaja", 0m)
                        });
                    }
                }
            }

            return result;
        }
    }
}
