using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet
{
    internal class NNTPResponseCodes
    {
        internal const string DebugOutput = "199";
        internal const string HelpTextFollows = "100";

        internal const string ServerReadyPostingAllowed = "200";
        internal const string ServerReadyNoPostingAllowed = "201";
        internal const string SlaveStatusNoted = "202";
        internal const string ClosingConnectionGoodbye = "205";
        internal const string GroupSelected = "211";
        internal const string ListOfArticleNumbersFollow = "211";
        internal const string ListOfNewsGroupsFollows = "215";
        internal const string ArticleRetrievedHeadAndBodyToFollow = "220";
        internal const string ArticleRetrievedHeadToFollow = "221";
        internal const string ArticleRetrievedBodyToFollow = "222";
        internal const string ArticleRetrievedRequestTextSeparatley = "223";
        internal const string ListOfNewArticlesByMessageIDFollows = "230";
        internal const string ListOfNewNewsgroupsFollows = "231";
        internal const string ArticleTransferedOK = "235";
        internal const string ArticlePostedOK = "240";
        internal const string AuthenticationOK = "281";

        internal const string SendArticleToBeTransferred = "335";
        internal const string SendArticleToBePosted = "340";
        internal const string PasswordRequired = "381";

        internal const string ServiceDiscontinued = "400";
        internal const string NoSuchNewsGroup = "411";
        internal const string NoNewsGroupSelected = "412";
        internal const string NoCurrentArticleHasBeenSelected = "420";
        internal const string NoNextArticleInThisGroup = "421";
        internal const string NoPreviouseArticleInThisGroup = "422";
        internal const string NoSuchArticleNumberInGroup = "423";
        internal const string NoSuchArticleFound = "430";
        internal const string ArticleNoWantedDoNotSend = "435";
        internal const string ArticleTransferFailed = "436";
        internal const string ArticleRejected = "437";
        internal const string PostingIsNotAllowed = "440";
        internal const string PostingFailed = "441";
        internal const string AuthenticationRequired = "480";

        internal const string CommandNotRecognized = "500";
        internal const string CommandSyntaxError = "501";
        internal const string PermissionDenied = "502";
        internal const string ProgramFault = "503";

        internal static bool IsResposeCode(string response, string responseCode)
        {
            if (3 > response.Length)
                return false;

            if (response.Substring(0, 3) == responseCode)
                return true;

            return false;
        }
    }
}
