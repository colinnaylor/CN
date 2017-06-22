using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maple; 

namespace SystemsTestTool.Model {

    class LinkedServerUpdate {
        public static string RemoveLinkedServerCalls(string Server, string LinkedServerName) {
            string ret = "";
            string objectName = "";
            string database = "";
            try {
                Database db = new Database();

                List<Database.DbObject> objects = db.GetObjectsUsingLinkedServer(Server, LinkedServerName);

                foreach(Database.DbObject ob in objects){
                    objectName = ob.ObjectName;
                    database = ob.Database;

                    if (ob.ObjectName.Substring(0, 7) == "Error: ") {
                        ret += ob.ObjectName + "\r\n";
                    } else {
                        string objectText = db.GetObjectText(Server, ob.Database, ob.ObjectName);

                        //  Remove linked server, regardless of case
                        int pos = 0, start = 0;
                        string newObjectText = "";
                        while (pos > -1) {
                            pos = objectText.IndexOf(ob.LinkedServerName + ".",start, StringComparison.OrdinalIgnoreCase);
                            if (pos > -1) {
                                newObjectText += objectText.Substring(start, pos - start);
                                start = pos + ob.LinkedServerName.Length + 1;
                            }
                        }
                        newObjectText += objectText.Substring(start);


                        string r = db.AlterObject(ob, newObjectText);
                        if (r != "") {
                            ret += r + "\r\n";
                        }
                    }
                }

            } catch (Exception ex) {
                ret = "Problem removing {0} linked server usage on {1}\r\n{2}{3}\r\n{4}".Args(LinkedServerName,
                    Server,database,objectName,ex.Message);
            }

            return ret;
        }

    }
}
