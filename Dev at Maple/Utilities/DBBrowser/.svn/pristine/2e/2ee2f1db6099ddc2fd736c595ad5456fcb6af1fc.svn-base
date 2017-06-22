using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DBBrowser
{

    public class DbBrowserManager
    {
        private List<DBObject> _objects = new List<DBObject>();

        public bool Loaded;
        public void Load()
        {
            _objects = DataLayer.GetDbObjectList();
            Loaded = true;
        }
        public List<DBObject> GetFilteredList(string s)
        {
            return _objects.FindAll(x => x.Name.ToLower().Contains(s.ToLower())).OrderBy(x => x.Name).ToList();
        }

        public List<DBObject> GetFilteredExtendedList(string s, bool wholeWordOnly, bool ignoreComments)
        {
            var matchingObjects = DataLayer.GetDbObjectExtendedList(s);
            _objects.Clear();

            if (ignoreComments || wholeWordOnly)
            {

                var regex = s;
                if (wholeWordOnly) regex = string.Format(@"\b{0}\b", s);

                //go through all objects and remove those where the search term only appears in comments.
                var utils = new SqlParsingService();
                foreach (var dbObject in matchingObjects)
                {
                    var sqlDefinition = GetObjectDefinition(dbObject);
                    if (ignoreComments && !dbObject.Server.Contains("SSRS")) sqlDefinition = utils.StripCommentsFromSql(sqlDefinition);
                    if (Regex.Match(sqlDefinition, regex, RegexOptions.IgnoreCase).Success) _objects.Add(dbObject);
                }
            }
            else
                _objects = matchingObjects;

            Loaded = false;
            return _objects;

        }
        public string GetObjectDefinition(DBObject o)
        {
            return DataLayer.GetDbObjectDefinition(o);

        }

    }
}
