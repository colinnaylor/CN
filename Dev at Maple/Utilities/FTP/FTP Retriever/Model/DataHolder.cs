using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTP_Retriever
{
    public class DataRow
    {
        private Dictionary<int, string> columns = new Dictionary<int, string>();

        private int row = 0;
        public DataRow(int Row)
        {
            row = Row;
        }

        public void AddData(int Column, string Data)
        {
            columns.Add(Column, Data);
        }

        public string GetData(int Column)
        {
            return columns[Column];
        }

        public int Row
        {
            get { return row; }
        }

        public int ColumnCount
        {
            get { return columns.Count; }
        }
    }

    public class DataTable
    {
        private Dictionary<int, DataRow> rows = new Dictionary<int, DataRow>();

        public void AddRow(DataRow Row)
        {
            rows.Add(Row.Row, Row);
        }

        public DataRow GetRow(int Row)
        {
            return rows[Row];
        }

        public int RowCount
        {
            get { return rows.Count; }
        }
    }
}
