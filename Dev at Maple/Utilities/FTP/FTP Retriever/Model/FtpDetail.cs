using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTP_Retriever
{
    internal class FtpDetail
    {
        public FtpDetail(int id, string name)
        {
            ID = id;
            Name = name;

        }

        public enum eFtpType { ftp, sftp, UnixShare, Https }
        public enum eColumnIdentifierType { Name, Index }

        public int ID { get; private set; }
        public string Name { get; private set; }
        public string FtpLookupValue { get; set; }
        public string Folder { get; set; }
        public eFtpType FptType { get; set; }
        public eColumnIdentifierType ColumnIdentifierType { get; set; }
        public string ColumnsRequired { get; set; }
        public string TargetTable { get; set; }
        public string ViewName { get; set; }
        public string TargetColumnNames { get; set; }
        public string TargetColumnTypes { get; set; }

        public bool PGP { get; set; }
        public bool ZIP { get; set; }
        public bool DES { get; set; }  // Data Encryption Standard developed in the early 1970's

        public Dictionary<int, RetrieveItem> RetrieveItems = new Dictionary<int, RetrieveItem>();

        public string Delimiter { get; set; }
    }

    internal class RetrieveItem
    {
        public RetrieveItem(FtpDetail detailItem, int id, string filename, DateTime fileDate, DateTime timeToRetrieve)
        {
            Parent = detailItem;
            ID = id;
            FileName = filename;
            FileDate = fileDate;
            RetrieveTime = timeToRetrieve;
        }

        FtpDetail Parent = null;

        public int ID { get; private set; }
        public string FileName { get; set; }
        public DateTime FileDate { get; private set; }
        public DateTime RetrieveTime { get; private set; }
        public int Attempt { get; set; }
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Tell the system not to fetch the file from the FTP site and only to parse an existing local file
        /// </summary>
        public bool ParseOnly { get; set; }
    }

    internal class ScheduledItem
    {
        public int DetailID { get; set; }
        public DateTime FileDate { get; set; }
        public string FileNameMask { get; set; }
        public DateTime FetchTime { get; set; }
        public override string ToString()
        {
            return string.Format("{0}.  DetailID {1}. {2}", base.ToString(), DetailID, FileDate.ToString("dd MMM yy"));
        }
    }
}
