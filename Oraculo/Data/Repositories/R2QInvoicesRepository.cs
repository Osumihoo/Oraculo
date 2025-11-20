using Oraculo.Models;
using Sap.Data.Hana;

namespace Oraculo.Data.Repositories
{
    public class R2QInvoicesRepository : IR2QInvoicesRepository
    {
        private readonly Func<int, HanaConnection> _connectionString;

        public R2QInvoicesRepository(Func<int, HanaConnection> connectionString)
        {
            _connectionString = connectionString;
        }

        protected HanaConnection dbConnection(int environment)
        {
            return _connectionString(environment);
        }

        public async Task<List<R2QPurchaseInvoices>> GetPurchaseInvoices(int environment, DateTime fecha)
        {
            var result = new List<R2QPurchaseInvoices>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT 
                    T0.""Code"" AS code, 
                    T0.""Name"" AS name, 
                    T0.""U_SO1_PROVEEDOR"" AS proveedor, 
                    T0.""U_SO1_FECHA"" AS fecha, 
                    T0.""U_SO1_HORA"" AS hora, 
                    T0.""U_SO1_HORACADENA"" AS horacadena, 
                    T0.""U_SO1_SUCURSAL"" AS sucursal, 
                    T0.""U_SO1_ESTACION"" AS estacion, 
                    T0.""U_SO1_USUARIO"" AS usuario, 
                    T0.""U_SO1_TIPO"" AS tipo,
                    T0.""U_SO1_STATUS"" AS status, 
                    T0.""U_SO1_DOCDESTINO"" AS docdestino, 
                    T0.""U_SO1_REFERENCIA"" AS referencia, 
                    T0.""U_SO1_CONDICIONCREDI"" AS condcredito, 
                    T0.""U_SO1_DIASCREDITO"" AS diascredito, 
                    T0.""U_SO1_FECHAENTREGA"" AS fechaentrega, 
                    T0.""U_SO1_DESCUENTOACUM1"" AS descacumulable1, 
                    T0.""U_SO1_DESCUENTOACUM2"" AS descacumulable2, 
                    T0.""U_SO1_CONCEPTOGASTOS"" AS conceptogastos,
                    T0.""U_SO1_IMPORTEGASTOS"" AS importegastos, 
                    T0.""U_SO1_COMENTARIO"" AS comentario, 
                    T0.""U_SO1_MONEDA"" AS moneda, 
                    T0.""U_SO1_TIPOCAMBIO"" AS tipocambio, 
                    T0.""U_SO1_DCTOPORCENTAJE"" AS dctoporcentaje, 
                    T0.""U_SO1_DCTOMONTO"" AS dctomonto, 
                    T0.""U_SO1_TOTALNETO"" AS totalneto, 
                    T0.""U_SO1_TOTALNETOMONED"" AS totalnetomoned, 
                    T0.""U_SO1_GASTOSNETO"" AS importenetogastos, 
                    T0.""U_SO1_INDICAIMPUESTO"" AS indicadorimpuestos, 
                    T0.""U_SO1_TASAIMPUESTO"" AS tasaimpuesto, 
                    T0.""U_SO1_AUTORIZADO"" AS autorizado, 
                    T0.""U_SO1_SINCRONIZADO"" AS sincronizado, 
                    T0.""U_SO1_FOLIOCORTEX"" AS foliocortex, 
                    T0.""U_SO1_PREFIJO"" AS prefijo, 
                    T0.""U_SO1_FOLIO"" AS folio, 
                    T0.""U_SO1_FECHACONTABILI"" AS fechacontabili,
                    T0.""U_SO1_FECHADOCUMENTO"" AS fechadocumento, 
                    T0.""U_SO1_FECHAVENCIMIEN"" AS fechavencimiento, 
                    T0.""U_SO1_ARCHIVOPDF"" AS archivopdf, 
                    T0.""U_SO1_ARCHIVOXML"" AS archivoxml, 
                    T0.""U_SO1_TOTALFISICO"" AS totalfisico, 
                    T0.""U_SO1_FOLIOUUID"" AS foliouuid, 
                    T0.""U_SO1_REPARTIDO"" AS repartido, 
                    T0.""U_SO1_VERSIONR1"" AS versionr1, 
                    T0.""U_SO1_FECHACANCELOC"" AS fechacanceloc, 
                    T0.""U_SO1_PROVEEDORSEC"" AS proveedorsec, 
                    T0.""U_SO1_FOLIOREFOC"" AS foliorefoc, 
                    T0.""U_SO1_RECEPCIONPARCI"" AS recepcionparcial, 
                    T0.""U_SO1_FOLIOFAINTER"" AS foliofainter, 
                    T0.""U_SO1_SUCFILPROVE"" AS sucfilprove, 
                    T0.""U_SO1_COMPROCCENTRAL"" AS comproccentral, 
                    T0.""U_SO1_VERSCOMPROCENT"" AS verscomprocent, 
                    T0.""U_SO1_REDONDEOMONEDA"" AS redondeomoneda, 
                    T0.""U_SO1_PARAMETRO01"" AS parametro01, 
                    T0.""U_SO1_PARAMETRO02"" AS parametro02, 
                    T0.""U_SO1_COMPONENORIGEN"" AS componenorigen, 
                    T1.""Code"" AS detalle_code, 
                    T1.""Name"" AS detalle_name, 
                    T1.""U_SO1_FOLIO"" AS detalle_folio, 
                    T1.""U_SO1_NUMPARTIDA"" AS numpartida, 
                        CASE 
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '05001' THEN '05999'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '05083' THEN '05998'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '07016' THEN '07996'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '07017' THEN '07997'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '07028' THEN '07998'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '07006' THEN '07999' 
                           ELSE T1.""U_SO1_NUMEROARTICULO""
                        END AS  numeroarticulo, 
                    T1.""U_SO1_CANTIDAD"" AS cantidad, 
                    T1.""U_SO1_CANTIDADAB"" AS cantidadab, 
                    T1.""U_SO1_PBSD"" AS pbsd, 
                    T1.""U_SO1_PBCD"" AS pbcd, 
                    T1.""U_SO1_PNSD"" AS pnsd, 
                    T1.""U_SO1_PNCD"" AS pncd, 
                    T1.""U_SO1_DESCUENTO"" AS descuento,
                    T1.""U_SO1_IMPUESTO"" AS impuesto, 
                    T1.""U_SO1_IMPORTENETO"" AS importeneto, 
                    T1.""U_SO1_MONEDA"" AS detalle_moneda, 
                    T1.""U_SO1_TIPOCAMBIO"" AS detalle_tipocambio, 
                    T1.""U_SO1_LISTAMAT"" AS listamat, 
                    T1.""U_SO1_ESPADRE"" AS espadre, 
                    T1.""U_SO1_TIPOLM"" AS tipolm, 
                    T1.""U_SO1_CANTIDADORIG"" AS cantidadorig, 
                    T1.""U_SO1_CODIGOALT"" AS codigoalt, 
                    T1.""U_SO1_DOCUMENTOBASE"" AS documentobase, 
                    T1.""U_SO1_PARTIDABASE"" AS partidabase, 
                    T1.""U_SO1_COMENTARIOS"" AS comentarios, 
                    T1.""U_SO1_ALMACEN"" AS almacen, 
                    T1.""U_SO1_INDICAIMPUESTO"" AS detalle_indicaimpuesto, 
                    T1.""U_SO1_TASAIMPUESTO"" AS detalle_tasaimpuesto, 
                    T1.""U_SO1_CANTIDADSC"" AS cantidadsc, 
                    T1.""U_SO1_LISTAPRECIOS"" AS listaprecios, 
                    T1.""U_SO1_PRECIOMANUAL"" AS preciomanual, 
                    T1.""U_SO1_MEDIDAINVENTA"" AS medidainventa, 
                    T1.""U_SO1_UNIDADCOMPRA"" AS unidadcompra, 
                    T1.""U_SO1_PARTIDASBO"" AS partidasbo, 
                    T1.""U_SO1_CANTIDADEMPAQ"" AS cantidadempaq, 
                    T1.""U_SO1_CODIGOEMPAQ"" AS codigoempaq, 
                    T1.""U_SO1_CODIGODESEMPAQ"" AS codigodesempaq, 
                    T1.""U_SO1_UNIDADMEDIDAIN"" AS unidadmedidain, 
                    T1.""U_SO1_CODIUNIMEDINV"" AS codiunimedinv, 
                    T1.""U_SO1_CANTUNIMEDINV"" AS cantunimedinv, 
                    T1.""U_SO1_DESCRIPCION"" AS descripcion, 
                    T1.""U_SO1_CANTEXISTENCIA"" AS cantexistencia, 
                    T1.""U_SO1_CANTPROPUESTA"" AS cantpropuesta, 
                    T1.""U_SO1_RETENCION"" AS retencion,
                    T2.""U_SATCLAVEARTICULO"" AS CODIGOSAT,
                        CASE
                        WHEN T1.""U_SO1_INDICAIMPUESTO"" = 'C00IEP8' THEN (T1.""U_SO1_IMPORTENETO"" * 0.08)
                        WHEN T1.""U_SO1_INDICAIMPUESTO"" = 'C16IEP26'  THEN (T1.""U_SO1_IMPORTENETO"" * 0.265) 
                        WHEN T1.""U_SO1_INDICAIMPUESTO"" = 'C16IEP30'  THEN (T1.""U_SO1_IMPORTENETO"" * 0.30)
                        WHEN T1.""U_SO1_INDICAIMPUESTO"" = 'C16IEP53'  THEN (T1.""U_SO1_IMPORTENETO"" * 0.53)
                        ELSE 0 
                    END AS IEPS,
                        CASE 
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '05001' THEN '05999'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '05083' THEN '05998'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '07016' THEN '07996'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '07017' THEN '07997'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '07028' THEN '07998'
                            WHEN T1.""U_SO1_NUMEROARTICULO"" = '07006' THEN '07999' 
                           ELSE T1.""U_SO1_NUMEROARTICULO""
                        END AS CODART,
                    T3.""CardName"" AS NomProv,
                    T3.""LicTradNum"" AS RFC

                    FROM ""SBO_ELVALOR_PRODUCTIVA"".""@SO1_01COMPRA"" T0
                    JOIN ""SBO_ELVALOR_PRODUCTIVA"".""@SO1_01COMPRADETALLE"" T1 ON T0.""Code"" = T1.""U_SO1_FOLIO"" 
                    JOIN OITM T2 ON T1.""U_SO1_NUMEROARTICULO"" = T2.""ItemCode""
                    JOIN OCRD T3 ON T3.""CardCode"" = T0.""U_SO1_PROVEEDOR""
                    WHERE T0.""U_SO1_FECHA"" = TO_DATE(?, 'YYYY-MM-DD') AND T1.""U_SO1_ALMACEN"" = '300'
                    ORDER BY code;";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    // El orden aquí importa: es el mismo que el de los "?" en la query
                    cmd.Parameters.AddWithValue("", fecha.ToString("yyyy-MM-dd"));

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new R2QPurchaseInvoices
                            {
                                Code = reader["code"].ToString(),
                                Name = reader["name"].ToString(),
                                Proveedor = reader["proveedor"].ToString(),
                                Fecha = reader["fecha"].ToString(),
                                Hora = reader["hora"].ToString(),
                                HoraCadena = reader["horacadena"].ToString(),
                                Sucursal = reader["sucursal"].ToString(),
                                Estacion = reader["estacion"].ToString(),
                                Usuario = reader["usuario"].ToString(),
                                Tipo = reader["tipo"].ToString(),
                                Status = reader["status"].ToString(),
                                DocDestino = reader["docdestino"].ToString(),
                                Referencia = reader["referencia"].ToString(),
                                CondCredito = reader["condcredito"].ToString(),
                                DiasCredito = reader["diascredito"] == DBNull.Value ? null : Convert.ToInt32(reader["diascredito"]),
                                FechaEntrega = reader["fechaentrega"].ToString(),
                                DescAcumulable1 = reader["descacumulable1"] == DBNull.Value ? null : Convert.ToDecimal(reader["descacumulable1"]),
                                DescAcumulable2 = reader["descacumulable2"] == DBNull.Value ? null : Convert.ToDecimal(reader["descacumulable2"]),
                                ConceptoGastos = reader["conceptogastos"].ToString(),
                                ImporteGastos = reader["importegastos"] == DBNull.Value ? null : Convert.ToDecimal(reader["importegastos"]),
                                Comentario = reader["comentario"].ToString(),
                                Moneda = reader["moneda"].ToString(),
                                TipoCambio = reader["tipocambio"] == DBNull.Value ? null : Convert.ToDecimal(reader["tipocambio"]),
                                DctoPorcentaje = reader["dctoporcentaje"] == DBNull.Value ? null : Convert.ToDecimal(reader["dctoporcentaje"]),
                                DctoMonto = reader["dctomonto"] == DBNull.Value ? null : Convert.ToDecimal(reader["dctomonto"]),
                                TotalNeto = reader["totalneto"] == DBNull.Value ? null : Convert.ToDecimal(reader["totalneto"]),
                                TotalNetoMoned = reader["totalnetomoned"] == DBNull.Value ? null : Convert.ToDecimal(reader["totalnetomoned"]),
                                ImporteNetoGastos = reader["importenetogastos"] == DBNull.Value ? null : Convert.ToDecimal(reader["importenetogastos"]),
                                IndicadorImpuestos = reader["indicadorimpuestos"].ToString(),
                                TasaImpuesto = reader["tasaimpuesto"] == DBNull.Value ? null : Convert.ToDecimal(reader["tasaimpuesto"]),
                                Autorizado = reader["autorizado"].ToString(),
                                Sincronizado = reader["sincronizado"].ToString(),
                                FolioCortex = reader["foliocortex"].ToString(),
                                Prefijo = reader["prefijo"].ToString(),
                                Folio = reader["folio"].ToString(),
                                FechaContabili = reader["fechacontabili"].ToString(),
                                FechaDocumento = reader["fechadocumento"].ToString(),
                                FechaVencimiento = reader["fechavencimiento"].ToString(),
                                ArchivoPdf = reader["archivopdf"].ToString(),
                                ArchivoXml = reader["archivoxml"].ToString(),
                                TotalFisico = reader["totalfisico"] == DBNull.Value ? null : Convert.ToDecimal(reader["totalfisico"]),
                                FolioUuid = reader["foliouuid"].ToString(),
                                Repartido = reader["repartido"].ToString(),
                                VersionR1 = reader["versionr1"].ToString(),
                                FechaCancelOc = reader["fechacanceloc"].ToString(),
                                ProveedorSec = reader["proveedorsec"].ToString(),
                                FolioRefOc = reader["foliorefoc"].ToString(),
                                RecepcionParcial = reader["recepcionparcial"].ToString(),
                                FolioFaInter = reader["foliofainter"].ToString(),
                                SucFilProve = reader["sucfilprove"].ToString(),
                                CompRocCentral = reader["comproccentral"].ToString(),
                                VersComproCent = reader["verscomprocent"].ToString(),
                                RedondeoMoneda = reader["redondeomoneda"].ToString(),
                                Parametro01 = reader["parametro01"].ToString(),
                                Parametro02 = reader["parametro02"].ToString(),
                                ComponenOrigen = reader["componenorigen"].ToString(),
                                DetalleCode = reader["detalle_code"].ToString(),
                                DetalleName = reader["detalle_name"].ToString(),
                                DetalleFolio = reader["detalle_folio"].ToString(),
                                NumPartida = reader["numpartida"] == DBNull.Value ? null : Convert.ToInt32(reader["numpartida"]),
                                NumeroArticulo = reader["numeroarticulo"].ToString(),
                                Cantidad = reader["cantidad"] == DBNull.Value ? null : Convert.ToDecimal(reader["cantidad"]),
                                CantidadAb = reader["cantidadab"] == DBNull.Value ? null : Convert.ToDecimal(reader["cantidadab"]),
                                Pbsd = reader["pbsd"] == DBNull.Value ? null : Convert.ToDecimal(reader["pbsd"]),
                                Pbcd = reader["pbcd"] == DBNull.Value ? null : Convert.ToDecimal(reader["pbcd"]),
                                Pnsd = reader["pnsd"] == DBNull.Value ? null : Convert.ToDecimal(reader["pnsd"]),
                                Pncd = reader["pncd"] == DBNull.Value ? null : Convert.ToDecimal(reader["pncd"]),
                                Descuento = reader["descuento"] == DBNull.Value ? null : Convert.ToDecimal(reader["descuento"]),
                                Impuesto = reader["impuesto"] == DBNull.Value ? null : Convert.ToDecimal(reader["impuesto"]),
                                ImporteNeto = reader["importeneto"] == DBNull.Value ? null : Convert.ToDecimal(reader["importeneto"]),
                                DetalleMoneda = reader["detalle_moneda"].ToString(),
                                DetalleTipoCambio = reader["detalle_tipocambio"] == DBNull.Value ? null : Convert.ToDecimal(reader["detalle_tipocambio"]),
                                ListaMat = reader["listamat"].ToString(),
                                EsPadre = reader["espadre"].ToString(),
                                TipoLm = reader["tipolm"].ToString(),
                                CantidadOrig = reader["cantidadorig"] == DBNull.Value ? null : Convert.ToDecimal(reader["cantidadorig"]),
                                CodigoAlt = reader["codigoalt"].ToString(),
                                DocumentoBase = reader["documentobase"].ToString(),
                                PartidaBase = reader["partidabase"].ToString(),
                                Comentarios = reader["comentarios"].ToString(),
                                Almacen = reader["almacen"].ToString(),
                                DetalleIndicaImpuesto = reader["detalle_indicaimpuesto"].ToString(),
                                DetalleTasaImpuesto = reader["detalle_tasaimpuesto"] == DBNull.Value ? null : Convert.ToDecimal(reader["detalle_tasaimpuesto"]),
                                CantidadSc = reader["cantidadsc"] == DBNull.Value ? null : Convert.ToDecimal(reader["cantidadsc"]),
                                ListaPrecios = reader["listaprecios"].ToString(),
                                PrecioManual = reader["preciomanual"].ToString(),
                                MedidaInventa = reader["medidainventa"].ToString(),
                                UnidadCompra = reader["unidadcompra"].ToString(),
                                PartidaSbo = reader["partidasbo"].ToString(),
                                CantidadEmpaq = reader["cantidadempaq"] == DBNull.Value ? null : Convert.ToDecimal(reader["cantidadempaq"]),
                                CodigoEmpaq = reader["codigoempaq"].ToString(),
                                CodigoDesempaq = reader["codigodesempaq"].ToString(),
                                UnidadMedidaIn = reader["unidadmedidain"].ToString(),
                                CodiUniMedInv = reader["codiunimedinv"].ToString(),
                                CantUniMedInv = reader["cantunimedinv"] == DBNull.Value ? null : Convert.ToDecimal(reader["cantunimedinv"]),
                                Descripcion = reader["descripcion"].ToString(),
                                CantExistencia = reader["cantexistencia"] == DBNull.Value ? null : Convert.ToDecimal(reader["cantexistencia"]),
                                CantPropuesta = reader["cantpropuesta"] == DBNull.Value ? null : Convert.ToDecimal(reader["cantpropuesta"]),
                                Retencion = reader["retencion"] == DBNull.Value ? null : Convert.ToDecimal(reader["retencion"]),
                                CodigoSAT = reader["codigosat"].ToString(),
                                Ieps = reader["ieps"] == DBNull.Value ? null : Convert.ToDecimal(reader["ieps"]),
                                CodArt = reader["codart"].ToString(),
                                NomProv = reader["nomprov"].ToString(),
                                RFC = reader["rfc"].ToString()
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<List<R2QSalesInvoices>> GetSalesInvoices(int environment, DateTime fecha)
        {
            var result = new List<R2QSalesInvoices>();

            using (HanaConnection conn = dbConnection(environment))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT 
                    T1.""Code"" AS id,
                    T0.""Code"" AS codigo,
                    T0.""Name"" AS nombre,
                    T0.""U_SO1_TOTALNETOMONED"" AS total_neto_moneda,
                    T0.""U_SO1_FECHA"" AS fecha,
                    T0.""U_SO1_HORA"" AS hora,
                    T0.""U_SO1_ESTACION"" AS estacion,
                    T0.""U_SO1_USUARIO"" AS usuario,
                    T0.""U_SO1_VENDEDOR"" AS vendedor,
                    T0.""U_SO1_CLIENTE"" AS cliente,
                    T0.""U_SO1_TOTALNETO"" AS total_neto,
                    T0.""U_SO1_COMENTARIO"" AS comentario,
                    T0.""U_SO1_FACTURA"" AS factura,
                    T0.""U_SO1_TIPO"" AS tipo,
                    T0.""U_SO1_STATUS"" AS status,
                    T0.""U_SO1_SUCURSAL"" AS sucursal,
                    T0.""U_SO1_FOLIODESTINO"" AS folio_destino,
                    T0.""U_SO1_FECHAVENC"" AS fecha_vencimiento,
                    T0.""U_SO1_FOLIOCORTEX"" AS folio_cortex,
                    T0.""U_SO1_CONDICIONPAGO"" AS condicion_pago,
                    T0.""U_SO1_LISTAPRECIO"" AS lista_precio,
                    T0.""U_SO1_DESCUENTMANUAL"" AS descuento_manual,
                    T0.""U_SO1_FOLIOCONSOLID"" AS folio_consolidado,
                    T0.""U_SO1_TIPOARCHIVOXML"" AS tipo_archivo_xml,
                    T0.""U_SO1_VERSIONR1"" AS version_r1,
                    T0.""U_SO1_DIRFISCAL"" AS direccion_fiscal,
                    T0.""U_SO1_IMPUESTO"" AS impuesto,
                    T0.""U_SO1_COMPROCCENTRAL"" AS comprobacion_central,
                    T0.""U_SO1_VERSCOMPROCENT"" AS version_comp_central,
                    T0.""U_SO1_COMPONENORIGEN"" AS componente_origen,
                    T1.""Name"" AS nombre_detalle,
                    T1.""U_SO1_PRECIOMANUAL"" AS precio_manual,
                    T1.""U_SO1_FOLIO"" AS folio,
                    CASE 
                        WHEN T1.""U_SO1_NUMEROARTICULO"" = '05001' THEN '05999'
                        WHEN T1.""U_SO1_NUMEROARTICULO"" = '05083' THEN '05998'
                        WHEN T1.""U_SO1_NUMEROARTICULO"" = '07016' THEN '07996'
                        WHEN T1.""U_SO1_NUMEROARTICULO"" = '07017' THEN '07997'
                        WHEN T1.""U_SO1_NUMEROARTICULO"" = '07028' THEN '07998'
                        WHEN T1.""U_SO1_NUMEROARTICULO"" = '07006' THEN '07999' 
                        ELSE T1.""U_SO1_NUMEROARTICULO""
                    END AS numero_articulo,
                    T1.""U_SO1_NUMPARTIDA"" AS num_partida,
                    T1.""U_SO1_CANTIDADFAC"" AS cantidad,
                    T1.""U_SO1_PBSD"" AS pbsd,
                    T1.""U_SO1_PBCD"" AS pbcd,
                    T1.""U_SO1_PNSD"" AS pnsd,
                    T1.""U_SO1_PNCD"" AS pncd,
                    T1.""U_SO1_DESCUENTO"" AS descuento,
                    T1.""U_SO1_CANTIDADFAC"" * T1.""U_SO1_PNCD"" AS importe_neto,
                    T1.""U_SO1_IMPUESTOFAC"" AS impuesto_det,
                    T1.""U_SO1_VENDEDOR"" AS vendedor_det,
                    T1.""U_SO1_CANTIDADAB"" AS cantidad_abierta,
                    T1.""U_SO1_CANTIDADORIG"" AS cantidad_origen,
                    T1.""U_SO1_DOCUMENTOBASE"" AS documento_base,
                    T1.""U_SO1_PARTIDABASE"" AS partida_base,
                    T1.""U_SO1_DESCRIPCION"" AS descripcion,
                    T1.""U_SO1_ALMACEN"" AS almacen,
                    T1.""U_SO1_CODIGOBARRAS"" AS codigo_barras,
                    T1.""U_SO1_CODIGOIMPUESTO"" AS codigo_impuesto,
                    T1.""U_SO1_IMPUESTOPRCNT"" AS impuesto_porcentaje,
                    T1.""U_SO1_RETENCION"" AS retencion,
                    T1.""U_SO1_LISTAPRECIO"" AS lista_precio_det,
                    T1.""U_SO1_PESOTEORICO"" AS peso_teorico,
                    T1.""U_SO1_PESOREAL"" AS peso_real,
                    T1.""U_SO1_LISTAPRECIOESP"" AS lista_precio_esp,
                    T1.""U_SO1_COSTO"" AS costo,
                    T1.""U_SO1_PARTIDASBO"" AS partida_sbo,
                    T1.""U_SO1_TIPOPROMOBASE"" AS tipo_promo_base,
                    T1.""U_SO1_CODIPROMOBASE"" AS codi_promo_base,
                    T1.""U_SO1_TIPOPROMOPRIN"" AS tipo_promo_principal,
                    T1.""U_SO1_CODIPROMOPRIN"" AS codi_promo_principal,
                    T1.""U_SO1_CODIUNIMEDINV"" AS cod_uni_med_inv,
                    T1.""U_SO1_CANTUNIMEDINV"" AS cant_uni_med_inv,
                    T1.""U_SO1_IMPUESTONETO"" AS impuesto_neto,
                    T1.""U_SO1_CANTPENDENT"" AS cantidad_pendiente,
                    T1.""U_SO1_CANTENTABIERTA"" AS cantidad_entrada_abierta
                FROM ""@SO1_01VENTA"" T0
                JOIN ""@SO1_01VENTADETALLE"" T1
                    ON T0.""Code"" = T1.""U_SO1_FOLIO""
                WHERE 
                    T0.""Name"" LIKE '%RF%' 
                    AND T0.""U_SO1_FECHA"" = TO_DATE(?, 'YYYY-MM-DD')";

                using (HanaCommand cmd = new HanaCommand(query, conn))
                {
                    // El orden aquí importa: es el mismo que el de los "?" en la query
                    cmd.Parameters.AddWithValue("", fecha.ToString("yyyy-MM-dd"));

                    using (HanaDataReader reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new R2QSalesInvoices
                            {
                                id = reader["id"].ToString(),
                                codigo = reader["codigo"].ToString(),
                                nombre = reader["nombre"].ToString(),
                                total_neto_moneda = reader["total_neto_moneda"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["total_neto_moneda"]),
                                fecha = reader["fecha"].ToString(),
                                hora = reader["hora"].ToString(),
                                estacion = reader["estacion"].ToString(),
                                usuario = reader["usuario"].ToString(),
                                vendedor = reader["vendedor"].ToString(),
                                cliente = reader["cliente"].ToString(),
                                total_neto = reader["total_neto"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["total_neto"]),
                                comentario = reader["comentario"].ToString(),
                                factura = reader["factura"].ToString(),
                                tipo = reader["tipo"].ToString(),
                                status = reader["status"].ToString(),
                                sucursal = reader["sucursal"].ToString(),
                                folio_destino = reader["folio_destino"].ToString(),
                                fecha_vencimiento = reader["fecha_vencimiento"].ToString(),
                                folio_cortex = reader["folio_cortex"].ToString(),
                                condicion_pago = reader["condicion_pago"].ToString(),
                                lista_precio = reader["lista_precio"].ToString(),
                                descuento_manual = reader["descuento_manual"].ToString(),
                                folio_consolidado = reader["folio_consolidado"].ToString(),
                                tipo_archivo_xml = reader["tipo_archivo_xml"].ToString(),
                                version_r1 = reader["version_r1"].ToString(),
                                direccion_fiscal = reader["direccion_fiscal"].ToString(),
                                impuesto = reader["impuesto"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["impuesto"]),
                                comprobacion_central = reader["comprobacion_central"].ToString(),
                                version_comp_central = reader["version_comp_central"].ToString(),
                                componente_origen = reader["componente_origen"].ToString(),
                                nombre_detalle = reader["nombre_detalle"].ToString(),
                                precio_manual = reader["precio_manual"].ToString(),
                                folio = reader["folio"].ToString(),
                                numero_articulo = reader["numero_articulo"].ToString(),
                                num_partida = reader["num_partida"] as int?,
                                cantidad = reader["cantidad"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["cantidad"]),
                                pbsd = reader["pbsd"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["pbsd"]),
                                pbcd = reader["pbcd"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["pbcd"]),
                                pnsd = reader["pnsd"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["pnsd"]),
                                pncd = reader["pncd"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["pncd"]),
                                descuento = reader["descuento"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["descuento"]),
                                importe_neto = reader["importe_neto"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["importe_neto"]),
                                impuesto_det = reader["impuesto_det"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["impuesto_det"]),
                                vendedor_det = reader["vendedor_det"].ToString(),
                                cantidad_abierta = reader["cantidad_abierta"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["cantidad_abierta"]),
                                cantidad_origen = reader["cantidad_origen"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["cantidad_origen"]),
                                documento_base = reader["documento_base"].ToString(),
                                partida_base = reader["partida_base"].ToString(),
                                descripcion = reader["descripcion"].ToString(),
                                almacen = reader["almacen"].ToString(),
                                codigo_barras = reader["codigo_barras"].ToString(),
                                codigo_impuesto = reader["codigo_impuesto"].ToString(),
                                impuesto_porcentaje = reader["impuesto_porcentaje"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["impuesto_porcentaje"]),
                                retencion = reader["retencion"].ToString(),
                                lista_precio_det = reader["lista_precio_det"].ToString(),
                                peso_teorico = reader["peso_teorico"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["peso_teorico"]),
                                peso_real = reader["peso_real"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["peso_real"]),
                                lista_precio_esp = reader["lista_precio_esp"].ToString(),
                                costo = reader["costo"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["costo"]),
                                partida_sbo = reader["partida_sbo"].ToString(),
                                tipo_promo_base = reader["tipo_promo_base"].ToString(),
                                codi_promo_base = reader["codi_promo_base"].ToString(),
                                tipo_promo_principal = reader["tipo_promo_principal"].ToString(),
                                codi_promo_principal = reader["codi_promo_principal"].ToString(),
                                cod_uni_med_inv = reader["cod_uni_med_inv"].ToString(),
                                cant_uni_med_inv = reader["cant_uni_med_inv"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["cant_uni_med_inv"]),
                                impuesto_neto = reader["impuesto_neto"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["impuesto_neto"]),
                                cantidad_pendiente = reader["cantidad_pendiente"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["cantidad_pendiente"]),
                                cantidad_entrada_abierta = reader["cantidad_entrada_abierta"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["cantidad_entrada_abierta"])
                            });

                        }
                    }
                }
            }
            return result;
        }
    }
}
