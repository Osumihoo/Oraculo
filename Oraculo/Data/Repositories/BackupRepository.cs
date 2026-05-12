using Oraculo.Models;
using Sap.Data.Hana;
using System.Data;

namespace Oraculo.Data.Repositories
{
    public class BackupRepository : IBackupRepository
    {
        private readonly Func<int, HanaConnection> _connectionString;

        public BackupRepository(Func<int, HanaConnection> connectionString)
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
        public async Task<List<BackupCostosPreciosExistencias>> ObtenerBackupCostosPreciosExistenciasAsync(int environment)
        {
            var result = new List<BackupCostosPreciosExistencias>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT
                         T1.""ItemCode"",
                         T1.""ItemName"",
                         T1.""NumInSale"" AS ""Piezas por caja"",
                         T0.""OnHand"" / T1.""NumInSale"" AS ""Existencia en cajas"",
                         T0.""AvgPrice"" AS ""Costo"",
                         T1.""TaxCodeAR"" AS ""Impuesto"",
                         T0.""WhsCode"",

                         CASE 
                                WHEN T1.""TaxCodeAR"" = 'V00IEP8' THEN T0.""AvgPrice"" * 1.08 - T0.""AvgPrice""
                                ELSE 0
                            END AS ""IVA 0+ IEPS 8%"",

                         CASE 
                                WHEN T1.""TaxCodeAR"" = 'V16' THEN T0.""AvgPrice"" * 1.16 - T0.""AvgPrice""
                                ELSE 0
                            END AS ""IVA 16%"",

                         CASE 
                                WHEN T1.""TaxCodeAR"" = 'V16IEP26' THEN T0.""AvgPrice"" * 1.16 * 1.265 - T0.""AvgPrice""
                                ELSE 0
                            END AS ""IVA 16% + IEPS 26.5%"",

                         CASE 
                                WHEN T1.""TaxCodeAR"" = 'V16IEP30' THEN T0.""AvgPrice"" * 1.16 * 1.30 - T0.""AvgPrice""
                                ELSE 0
                            END AS ""IVA 16% + IEPS 30%"",

                        CASE
                                WHEN T1.""TaxCodeAR"" = 'V16IEP53' THEN T0.""AvgPrice"" * 1.16 * 1.53 - T0.""AvgPrice""
                                ELSE 0
                            END AS ""IVA 16% + IEPS 53%"",

                         CASE 
                                WHEN T1.""TaxCodeAR"" = 'V00' THEN T0.""AvgPrice""
                                WHEN T1.""TaxCodeAR"" = 'V00IEP8' THEN T0.""AvgPrice"" * 1.08
                                WHEN T1.""TaxCodeAR"" = 'V16' THEN T0.""AvgPrice"" * 1.16
                                WHEN T1.""TaxCodeAR"" = 'V16IEP26' THEN T0.""AvgPrice"" * 1.16 * 1.265
                                WHEN T1.""TaxCodeAR"" = 'V16IEP30' THEN T0.""AvgPrice"" * 1.16 * 1.30
                                WHEN T1.""TaxCodeAR"" = 'V16IEP53' THEN T0.""AvgPrice"" * 1.16 * 1.53
                                ELSE T0.""AvgPrice""
                            END AS ""Costo con Impuestos"",
    
                         CASE 
                                WHEN T1.""TaxCodeAR"" = 'V00' THEN T0.""AvgPrice"" * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V00IEP8' THEN T0.""AvgPrice"" * 1.08  * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V16' THEN T0.""AvgPrice"" * 1.16  * T1.""NumInSale""       
                                WHEN T1.""TaxCodeAR"" = 'V16IEP26' THEN T0.""AvgPrice"" * 1.16 * 1.265  * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V16IEP30' THEN T0.""AvgPrice"" * 1.16 * 1.30 * T1.""NumInSale"" 
                                WHEN T1.""TaxCodeAR"" = 'V16IEP53' THEN T0.""AvgPrice"" * 1.16 * 1.53 * T1.""NumInSale"" 
                                ELSE T0.""AvgPrice"" * T1.""NumInSale""
                           END AS ""Costo Caja"",  
 
                         T2.""Price"" * T1.""NumInSale"" AS Precio,
 
                         (T2.""Price"" * T1.""NumInSale"") -
                         ( CASE 
                                WHEN T1.""TaxCodeAR"" = 'V00' THEN T0.""AvgPrice"" * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V00IEP8' THEN T0.""AvgPrice"" * 1.08  * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V16' THEN T0.""AvgPrice"" * 1.16  * T1.""NumInSale""       
                                WHEN T1.""TaxCodeAR"" = 'V16IEP26' THEN T0.""AvgPrice"" * 1.16 * 1.265  * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V16IEP30' THEN T0.""AvgPrice"" * 1.16 * 1.30 * T1.""NumInSale"" 
                                WHEN T1.""TaxCodeAR"" = 'V16IEP53' THEN T0.""AvgPrice"" * 1.16 * 1.53 * T1.""NumInSale"" 
                                ELSE T0.""AvgPrice"" * T1.""NumInSale""
                         END) AS Ganancia,
 
                        ROUND(
                           (
                             ((T2.""Price"" * T1.""NumInSale"") -
                              (CASE 
                                WHEN T1.""TaxCodeAR"" = 'V00' THEN T0.""AvgPrice"" * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V00IEP8' THEN T0.""AvgPrice"" * 1.08  * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V16' THEN T0.""AvgPrice"" * 1.16  * T1.""NumInSale""       
                                WHEN T1.""TaxCodeAR"" = 'V16IEP26' THEN T0.""AvgPrice"" * 1.16 * 1.265  * T1.""NumInSale""
                                WHEN T1.""TaxCodeAR"" = 'V16IEP30' THEN T0.""AvgPrice"" * 1.16 * 1.30 * T1.""NumInSale"" 
                                WHEN T1.""TaxCodeAR"" = 'V16IEP53' THEN T0.""AvgPrice"" * 1.16 * 1.53 * T1.""NumInSale"" 
                                ELSE T0.""AvgPrice"" * T1.""NumInSale""
                              END)
                             ) / NULLIF((T2.""Price"" * T1.""NumInSale""), 0)
                           ) * 100, 2
                         ) AS ""Margen %""

 
                        FROM OITW T0 
                        INNER JOIN OITM T1 ON T0.""ItemCode"" = T1.""ItemCode"" 
                        INNER JOIN ITM1 T2 ON T0.""ItemCode"" = T2.""ItemCode""
                        WHERE T1.""frozenFor"" = 'N'
                          AND T2.""PriceList"" = '1'
                        ORDER BY T1.""ItemCode"", T0.""WhsCode"";";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new BackupCostosPreciosExistencias
                            {
                                ItemCode = reader["ItemCode"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                PiezasPorCaja = GetValueOrDefault(reader, "Piezas por caja", 0m),
                                ExistenciaEnCajas = GetValueOrDefault(reader, "Existencia en cajas", 0m),
                                Costo = GetValueOrDefault(reader, "Costo", 0m),
                                Impuesto = reader["Impuesto"].ToString(),
                                WhsCode = reader["WhsCode"].ToString(),
                                IVA0IEPS8 = GetValueOrDefault(reader, "IVA 0+ IEPS 8%", 0m),
                                IVA16 = GetValueOrDefault(reader, "IVA 16%", 0m),
                                IVA16IEPS265 = GetValueOrDefault(reader, "IVA 16% + IEPS 26.5%", 0m),
                                IVA16IEPS30 = GetValueOrDefault(reader, "IVA 16% + IEPS 30%", 0m),
                                IVA16IEPS53 = GetValueOrDefault(reader, "IVA 16% + IEPS 53%", 0m),
                                CostoConImpuestos = GetValueOrDefault(reader, "Costo con Impuestos", 0m),
                                CostoCaja = GetValueOrDefault(reader, "Costo Caja", 0m),
                                Precio = GetValueOrDefault(reader, "Precio", 0m),
                                Ganancia = GetValueOrDefault(reader, "Ganancia", 0m),
                                MargenPorcentaje = GetValueOrDefault(reader, "Margen %", 0m)
                            });
                        }
                    }
                        
                }
                
            }

            return result;
        }

        public async Task<List<BackupSaldosDeudoresDiversos>> ObtenerBackupSaldosDeudoresDiversosAsync(int environment)
        {
            var result = new List<BackupSaldosDeudoresDiversos>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                            SELECT 
                                T0.""CardCode"", 
                                T0.""CardName"", 
                                T0.""Balance"" 
                            FROM OCRD T0 
                            WHERE T0.""Balance"" != 0 
                            AND T0.""CardCode"" LIKE 'DD%' 
                            ORDER BY T0.""CardCode""";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new BackupSaldosDeudoresDiversos
                            {
                                CardCode = reader["CardCode"].ToString(),
                                CardName = reader["CardName"].ToString(),
                                Balance = GetValueOrDefault(reader, "Balance", 0m)
                            });
                        }
                    }
                }
            }

            return result;
        }
        public async Task<List<BackupSaldosSociosDeNegocios>> ObtenerBackupSaldosSociosDeNegociosAsync(int environment)
        {
            var result = new List<BackupSaldosSociosDeNegocios>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                        SELECT 
                            T0.""CardCode"", 
                            T0.""CardName"", 
                            T0.""Balance"" 
                        FROM OCRD T0 
                        WHERE T0.""Balance"" != 0 
                        AND T0.""CardCode"" NOT LIKE 'DD%' 
                        ORDER BY T0.""CardCode""";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new BackupSaldosSociosDeNegocios
                            {
                                CardCode = reader["CardCode"].ToString(),
                                CardName = reader["CardName"].ToString(),
                                Balance = GetValueOrDefault(reader, "Balance", 0m)
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<List<BackupStock>> ObtenerBackupStockAsync(int environment)
        {
            var result = new List<BackupStock>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                        SELECT 
                            T1.""ItemCode"",
                            T1.""ItemName"",
                            T1.""SalUnitMsr"",
                            SUM(CASE WHEN T0.""WhsCode"" = '300' THEN T0.""OnHand"" ELSE 0 END) AS ""CEDIS_ABASTOS_ADMIN"",
                            SUM(CASE WHEN T0.""WhsCode"" = '301' THEN T0.""OnHand"" ELSE 0 END) AS ""CEDIS_ABASTOS_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '303' THEN T0.""OnHand"" ELSE 0 END) AS ""ALMACEN_COLIMA_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '306' THEN T0.""OnHand"" ELSE 0 END) AS ""TRANSITO_ADMIN"",
                            SUM(CASE WHEN T0.""WhsCode"" = '309' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_MANDARINA_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '311' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_MERCADO_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '313' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_GRANADILLA_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '315' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_AVIACION_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '317' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_TLAJOMULCO_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '319' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_8_DE_JULIO_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '321' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_JUAN_DE_LA_B_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '323' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_CD_GUZMAN_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '325' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_CHAVEZ_CARRILLO_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '327' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_N_HEROES_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '329' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_TECOMAN_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '331' THEN T0.""OnHand"" ELSE 0 END) AS ""SC_MANZANILLO_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '334' THEN T0.""OnHand"" ELSE 0 END) AS ""VILLA_DE_ALVAREZ_OPER"",
                            SUM(CASE WHEN T0.""WhsCode"" = '337' THEN T0.""OnHand"" ELSE 0 END) AS ""BODEGA_CALLE_14"",
                            SUM(CASE WHEN T0.""WhsCode"" = 'AMZ' THEN T0.""OnHand"" ELSE 0 END) AS ""AMAZON"",
                            SUM(CASE WHEN T0.""WhsCode"" = 'ECM' THEN T0.""OnHand"" ELSE 0 END) AS ""ECOMMERCE"",
                            SUM(CASE WHEN T0.""WhsCode"" = 'MER' THEN T0.""OnHand"" ELSE 0 END) AS ""MERMA_AUT_CAMBIO"",
                            SUM(CASE WHEN T0.""WhsCode"" = 'MLI' THEN T0.""OnHand"" ELSE 0 END) AS ""MERCADO_LIBRE""
                        FROM OITW T0
                        INNER JOIN OITM T1 ON T0.""ItemCode"" = T1.""ItemCode""
                        WHERE T1.""frozenFor"" = 'N'
                        GROUP BY T1.""ItemCode"", T1.""SalUnitMsr"", T1.""ItemName""
                        ORDER BY T1.""ItemCode""";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new BackupStock
                            {
                                ItemCode = reader["ItemCode"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                SalUnitMsr = reader["SalUnitMsr"].ToString(),

                                CEDIS_ABASTOS_ADMIN = GetValueOrDefault(reader, "CEDIS_ABASTOS_ADMIN", 0m),
                                CEDIS_ABASTOS_OPER = GetValueOrDefault(reader, "CEDIS_ABASTOS_OPER", 0m),
                                ALMACEN_COLIMA_OPER = GetValueOrDefault(reader, "ALMACEN_COLIMA_OPER", 0m),
                                TRANSITO_ADMIN = GetValueOrDefault(reader, "TRANSITO_ADMIN", 0m),
                                SC_MANDARINA_OPER = GetValueOrDefault(reader, "SC_MANDARINA_OPER", 0m),
                                SC_MERCADO_OPER = GetValueOrDefault(reader, "SC_MERCADO_OPER", 0m),
                                SC_GRANADILLA_OPER = GetValueOrDefault(reader, "SC_GRANADILLA_OPER", 0m),
                                SC_AVIACION_OPER = GetValueOrDefault(reader, "SC_AVIACION_OPER", 0m),
                                SC_TLAJOMULCO_OPER = GetValueOrDefault(reader, "SC_TLAJOMULCO_OPER", 0m),
                                SC_8_DE_JULIO_OPER = GetValueOrDefault(reader, "SC_8_DE_JULIO_OPER", 0m),
                                SC_JUAN_DE_LA_B_OPER = GetValueOrDefault(reader, "SC_JUAN_DE_LA_B_OPER", 0m),
                                SC_CD_GUZMAN_OPER = GetValueOrDefault(reader, "SC_CD_GUZMAN_OPER", 0m),
                                SC_CHAVEZ_CARRILLO_OPER = GetValueOrDefault(reader, "SC_CHAVEZ_CARRILLO_OPER", 0m),
                                SC_N_HEROES_OPER = GetValueOrDefault(reader, "SC_N_HEROES_OPER", 0m),
                                SC_TECOMAN_OPER = GetValueOrDefault(reader, "SC_TECOMAN_OPER", 0m),
                                SC_MANZANILLO_OPER = GetValueOrDefault(reader, "SC_MANZANILLO_OPER", 0m),
                                VILLA_DE_ALVAREZ_OPER = GetValueOrDefault(reader, "VILLA_DE_ALVAREZ_OPER", 0m),
                                BODEGA_CALLE_14 = GetValueOrDefault(reader, "BODEGA_CALLE_14", 0m),
                                AMAZON = GetValueOrDefault(reader, "AMAZON", 0m),
                                ECOMMERCE = GetValueOrDefault(reader, "ECOMMERCE", 0m),
                                MERMA_AUT_CAMBIO = GetValueOrDefault(reader, "MERMA_AUT_CAMBIO", 0m),
                                MERCADO_LIBRE = GetValueOrDefault(reader, "MERCADO_LIBRE", 0m)
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
