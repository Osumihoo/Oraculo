namespace Oraculo.Dictionary
{
    public class BinMapsFamilyWarehouse
    {
        public static Dictionary<string, List<string>> Get()
        {
            return new Dictionary<string, List<string>>
            {
                ["CERVEZA"] = new List<string> { "300", "301", "305", "336" },
                ["VINOS, LICORES Y DESTILADOS"] = new List<string> { "300", "301", "305", "336" },
                ["CIGARROS"] = new List<string> { "309", "313" },
                ["BEBIDAS CON ALCOHOL"] = new List<string> { "309", "300", "301", "305", "336" },
                ["BEBIDAS SIN ALCOHOL"] = new List<string> { "309", "300", "301", "305", "336" },
                ["ABARROTES"] = new List<string> { "309", "300", "301", "305", "336" },
                ["MASCOTAS"] = new List<string> { "309", "300", "301", "305", "336" }
            };
        }
    }
}
