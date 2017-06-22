using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DBBrowser
{
    class SqlParsingService
    {
        public string StripCommentsFromSql(string sql)
        {

            TSql110Parser parser = new TSql110Parser(true);
            IList<ParseError> errors;
            var fragments = parser.Parse(new StringReader(sql), out errors);

            // clear comments
            string result = string.Join(
              string.Empty,
              fragments.ScriptTokenStream
                  .Where(x => x.TokenType != TSqlTokenType.MultilineComment)
                  .Where(x => x.TokenType != TSqlTokenType.SingleLineComment)
                  .Select(x => x.Text));

            return result;

        }

    }
}
