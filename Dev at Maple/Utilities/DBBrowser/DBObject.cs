﻿using System;

namespace DBBrowser
{
    public class DBObject
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string ItemID { get; set; }

        /// <summary>
        /// This field is shown but not searched within
        /// </summary>
        public string ExtendedInfo { get; set; }

        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(ExtendedInfo)) return string.Format("{0} ({1})", Name, ExtendedInfo);
                return Name;
            }
        }
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Server, Database, Name);
        }
    }
}
