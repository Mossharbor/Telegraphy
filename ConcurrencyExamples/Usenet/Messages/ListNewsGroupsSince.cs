using System;

namespace Usenet.Messages
{
    class ListNewsGroupsSince : ListNewsGroups
    {
        DateTime lastCheckedDate;
        public DateTime LastCheckedDate
        {
            get { return lastCheckedDate; }
            set { lastCheckedDate = value; }
        }

        public ListNewsGroupsSince(ServerConnection conn,DateTime lastCheckedDate):base(conn)
        {
            this.LastCheckedDate = lastCheckedDate;
            this.ThisType = typeof(ListNewsGroupsSince);
        }
    }
}
