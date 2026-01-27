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

        public async Task<List<RRHHTimeClock>> GetRRHHTimeClock(int environment)
        {
            var result = new List<RRHHTimeClock>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT 
                    CASE T5.""branch""
                        WHEN '1' THEN 'Mandarina'
                        WHEN '2' THEN 'Mercado'
                        WHEN '3' THEN 'Granadilla'
                        WHEN '4' THEN 'Base Aérea'
                        WHEN '5' THEN 'Tlajomulco'
                        WHEN '6' THEN '8 de Julio'
                        WHEN '7' THEN 'Juan de la Barrera'
                        WHEN '8' THEN 'Chavez Carrillo'
                        WHEN '9' THEN 'Niños Héroes'
                        WHEN '10' THEN 'Tecoman'
                        WHEN '11' THEN 'Ciudad Guzmán'
                        WHEN '12' THEN 'Manzanillo'
                        WHEN '13' THEN 'Cedis Colima'
                        WHEN '14' THEN 'Villa de Alvarez'
                        WHEN '15' THEN 'Cedis Abastos'
                        WHEN '16' THEN 'Oficinas Abastos'
                        WHEN '17' THEN 'Corporativo'
                        WHEN '18' THEN 'Oficinas Abastos'
                    END AS ""Sucursal"",
                    CASE LEFT(T0.""Name"",3)
                        WHEN 'S01' THEN 'Mandarina'
                        WHEN 'S02' THEN 'Mercado'
                        WHEN 'S03' THEN 'Granadilla'
                        WHEN 'S04' THEN 'Base Aérea'
                        WHEN 'S05' THEN 'Tlajomulco'
                        WHEN 'S06' THEN '8 de Julio'
                        WHEN 'S07' THEN 'Juan de la Barrera'
                        WHEN 'S08' THEN 'Chavez Carrillo'
                        WHEN 'S09' THEN 'Niños Héroes'
                        WHEN 'S10' THEN 'Tecoman'
                        WHEN 'S11' THEN 'Ciudad Guzmán'
                        WHEN 'S12' THEN 'Manzanillo'
                        WHEN 'S13' THEN 'Cedis Colima'
                        WHEN 'S14' THEN 'Villa de Alvarez'
                        WHEN 'S15' THEN 'Cedis Abastos'
                        WHEN 'S16' THEN 'Ecommerce'
                        WHEN 'S17' THEN 'Cedis Guadalajara'
                    END AS ""Sucursal Checada"",
                    (IFNULL(T5.""lastName"", '') || ' ' || IFNULL(T5.""firstName"", '')) AS ""NombreUsuario"",
                    T0.""U_SO1_FECHA"",
                    MIN(CASE WHEN T0.""U_SO1_ESTATUS"" = 'E' THEN T0.""U_SO1_HORACADENA"" END) AS ""Entrada"",
                    MAX(CASE WHEN T0.""U_SO1_ESTATUS"" = 'S' THEN T0.""U_SO1_HORACADENA"" END) AS ""Salida"",
                    ROUND(
                        (SECONDS_BETWEEN(
                            MAX(CASE WHEN T0.""U_SO1_ESTATUS"" = 'S' THEN TO_TIME(T0.""U_SO1_HORACADENA"") END),
                            MIN(CASE WHEN T0.""U_SO1_ESTATUS"" = 'E' THEN TO_TIME(T0.""U_SO1_HORACADENA"") END)
                        ) / 3600 * -1 - 1), 2
                    ) AS ""HorasTrabajadas"",
                    CASE 
                        WHEN ROUND(
                            (SECONDS_BETWEEN(
                                MAX(CASE WHEN T0.""U_SO1_ESTATUS"" = 'S' THEN TO_TIME(T0.""U_SO1_HORACADENA"") END),
                                MIN(CASE WHEN T0.""U_SO1_ESTATUS"" = 'E' THEN TO_TIME(T0.""U_SO1_HORACADENA"") END)
                            ) / 3600) * -1, 2
                        ) > 1 THEN 1 ELSE 0
                    END AS ""Hora de comida""
                FROM OHEM T5
                LEFT JOIN ""@SO1_01REGENTRASALI"" T0
                    ON T0.""U_SO1_CODEMPLEADO"" = T5.""empID""
                   AND T0.""U_SO1_FECHA"" > ADD_DAYS(CURRENT_DATE, -2)
                   AND T0.""U_SO1_CODEMPLEADO"" <> '2'
                WHERE T5.""branch"" IS NOT NULL
                  AND T5.""branch"" <> '-2'
                GROUP BY
                    CASE T5.""branch""
                        WHEN '1' THEN 'Mandarina'
                        WHEN '2' THEN 'Mercado'
                        WHEN '3' THEN 'Granadilla'
                        WHEN '4' THEN 'Base Aérea'
                        WHEN '5' THEN 'Tlajomulco'
                        WHEN '6' THEN '8 de Julio'
                        WHEN '7' THEN 'Juan de la Barrera'
                        WHEN '8' THEN 'Chavez Carrillo'
                        WHEN '9' THEN 'Niños Héroes'
                        WHEN '10' THEN 'Tecoman'
                        WHEN '11' THEN 'Ciudad Guzmán'
                        WHEN '12' THEN 'Manzanillo'
                        WHEN '13' THEN 'Cedis Colima'
                        WHEN '14' THEN 'Villa de Alvarez'
                        WHEN '15' THEN 'Cedis Abastos'
                        WHEN '16' THEN 'Oficinas Abastos'
                        WHEN '17' THEN 'Corporativo'
                        WHEN '18' THEN 'Oficinas Abastos'
                    END,
                    CASE LEFT(T0.""Name"",3)
                        WHEN 'S01' THEN 'Mandarina'
                        WHEN 'S02' THEN 'Mercado'
                        WHEN 'S03' THEN 'Granadilla'
                        WHEN 'S04' THEN 'Base Aérea'
                        WHEN 'S05' THEN 'Tlajomulco'
                        WHEN 'S06' THEN '8 de Julio'
                        WHEN 'S07' THEN 'Juan de la Barrera'
                        WHEN 'S08' THEN 'Chavez Carrillo'
                        WHEN 'S09' THEN 'Niños Héroes'
                        WHEN 'S10' THEN 'Tecoman'
                        WHEN 'S11' THEN 'Ciudad Guzmán'
                        WHEN 'S12' THEN 'Manzanillo'
                        WHEN 'S13' THEN 'Cedis Colima'
                        WHEN 'S14' THEN 'Villa de Alvarez'
                        WHEN 'S15' THEN 'Cedis Abastos'
                        WHEN 'S16' THEN 'Ecommerce'
                        WHEN 'S17' THEN 'Cedis Guadalajara'
                    END,
                    (IFNULL(T5.""lastName"", '') || ' ' || IFNULL(T5.""firstName"", '')),
                    T0.""U_SO1_FECHA""
                ORDER BY ""Sucursal"", T0.""U_SO1_FECHA"" DESC, ""NombreUsuario"";
                ";


                using (HanaCommand cmd = new HanaCommand(query, conn))
                using (HanaDataReader reader = cmd.ExecuteReader())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new RRHHTimeClock
                        {
                            Sucursal = reader["Sucursal"]?.ToString(),
                            SucursalChecada = reader["Sucursal Checada"]?.ToString(),
                            NombreUsuario = reader["NombreUsuario"]?.ToString(),
                            Fecha = reader["U_SO1_FECHA"] == DBNull.Value ? null : reader.GetDateTime(reader.GetOrdinal("U_SO1_FECHA")),
                            Entrada = reader["Entrada"] == DBNull.Value ? null : reader["Entrada"].ToString(),
                            Salida = reader["Salida"] == DBNull.Value ? null : reader["Salida"].ToString(),
                            HorasTrabajadas = reader["HorasTrabajadas"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["HorasTrabajadas"]),
                            HoraDeComida = reader["Hora de Comida"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Hora de Comida"])
                        });
                    }
                }
            }

            return result;
        }

    }
}
