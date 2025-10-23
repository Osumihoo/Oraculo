namespace Oraculo.Data
{
    public class HANAConfiguration
    {
        public HANAConfiguration(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
    }
}
