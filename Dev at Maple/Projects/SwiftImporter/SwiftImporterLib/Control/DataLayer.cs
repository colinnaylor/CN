using Maple;
using Maple.Database;
using SwiftImporterLib.Model;

namespace SwiftImporterLib
{
    public class DataLayer
    {
        private IDatabaseController _databaseController;

        public DataLayer(IDatabaseController databaseController)
        {
            _databaseController = databaseController;
        }

        public string SaveSwiftMessages(SwiftFile swiftFile)
        {
            int saved = 0, total = 0;

            foreach (SwiftMessage message in swiftFile.Messages)
            {
                total++;
                var sql = message.SqlAlreadyExistsString(); NLogger.Instance.Debug("Exists? : {0}", sql);
                if (_databaseController.GetScalar<int>(sql) == 0)
                {
                    sql = message.SqlInsertString(); NLogger.Instance.Debug("Committing to db: {0}", sql);
                    _databaseController.ExecuteNonQuery(sql);
                    saved++;
                }
            }
            return "{0}/{1}".Args(saved, total);
        }
    }
}
