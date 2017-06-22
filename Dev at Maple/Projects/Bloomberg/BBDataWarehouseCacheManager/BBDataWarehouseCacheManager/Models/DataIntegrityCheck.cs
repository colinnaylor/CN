namespace BBDataWarehouseCacheManager.Models
{
    public class DataIntegrityCheck
    {
        public string Name;
        public string LongTitle;
        public string Description;
        public string FailText;

        
        public string SuccessSql;
        public string FailureSql;

        public double Threshold;
    }
}