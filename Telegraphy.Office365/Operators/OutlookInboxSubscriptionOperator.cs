using Microsoft.Exchange.WebServices.Data;
using System;
using Telegraphy.Net;

namespace Telegraphy.Office365
{
    public class OutlookInboxSubscriptionOperator: OutlookEmailBaseOperator
    {
        public OutlookInboxSubscriptionOperator(string emailAddress, string password, bool unreadOnly = true, int maxDequeueCount = DefaultDequeueMaxCount)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), emailAddress, password, maxDequeueCount, GetSearchFilter(unreadOnly), WellKnownFolderName.Inbox)
        {
        }

        public OutlookInboxSubscriptionOperator(LocalConcurrencyType concurrencyType, string emailAddress, string password, bool unreadOnly = true, int maxDequeueCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), emailAddress, password,  maxDequeueCount, GetSearchFilter(unreadOnly), WellKnownFolderName.Inbox)
        {
        }

        public OutlookInboxSubscriptionOperator(ILocalSwitchboard switchBoard, string emailAddress, string password, bool unreadOnly = true, int maxDequeueCount = DefaultDequeueMaxCount)
               : base(switchBoard, emailAddress, password, maxDequeueCount, GetSearchFilter(unreadOnly), WellKnownFolderName.Inbox)
        {
        }

        private static SearchFilter GetSearchFilter(bool unreadOnly)
        {
            if (!unreadOnly)
                return null;

            return new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));
        }
    }
}
