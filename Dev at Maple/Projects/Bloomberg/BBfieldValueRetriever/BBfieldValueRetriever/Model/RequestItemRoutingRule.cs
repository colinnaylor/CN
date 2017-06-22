using System;
using System.Collections.Generic;

namespace BBfieldValueRetriever.Model
{
    public class RequestItemRoutingRule : IEquatable<RequestItemRoutingRule>
    {
        public string Datasource { get; set; }

        public string UserIdMatchRegex { get; set; }

        public string FieldListMatchRegex { get; set; }

        bool IEquatable<RequestItemRoutingRule>.Equals(RequestItemRoutingRule other)
        {
            return ((Datasource == other.Datasource) && (UserIdMatchRegex == other.UserIdMatchRegex) && (FieldListMatchRegex == other.FieldListMatchRegex));
        }
    }

    public class RequestItemRoutingRuleEqualityComparer : IEqualityComparer<RequestItemRoutingRule>
    {
        bool IEqualityComparer<RequestItemRoutingRule>.Equals(RequestItemRoutingRule x, RequestItemRoutingRule y)
        {
            return ((x.Datasource == y.Datasource) && (x.UserIdMatchRegex == y.UserIdMatchRegex) && (x.FieldListMatchRegex == y.FieldListMatchRegex));
        }

        int IEqualityComparer<RequestItemRoutingRule>.GetHashCode(RequestItemRoutingRule obj)
        {
            return string.Format("{0}|{1}|{2}", obj.FieldListMatchRegex, obj.UserIdMatchRegex, obj.Datasource).GetHashCode();
        }
    }
}