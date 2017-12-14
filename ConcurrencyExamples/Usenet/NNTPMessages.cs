using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet
{
    internal class NNTPMessages
    {
        //NOTE: constant strings here that do not include the \r\n in the string
        //need to be constructed by calling a function below.
        //NOTE: Any Messages that are added here also need to be added to the supported
        //Messages Hash in the NewsServer Class  in the function ResetSupportedCommandList.
        internal const string Quit = "QUIT\r\n";
        internal const string ListNewsGroups = "LIST\r\n";
        internal const string Next = "NEXT\r\n";
        internal const string Slave = "SLAVE\r\n";
        internal const string Help = "HELP\r\n";
        internal const string Date = "DATE\r\n";
        internal const string Ihave = "IHAVE\r\n";

        internal const string ListGroup = "LISTGROUP";
        internal const string NewsGroups = "NEWSGROUPS";
        internal const string NewNews = "NEWNEWS";
        internal const string Group = "GROUP";
        internal const string Article = "ARTICLE";
        internal const string Post = "POST";
        internal const string Head = "HEAD";
        internal const string Body = "BODY";
        internal const string AuthInfoUser = "AUTHINFO USER";
        internal const string AuthInfoPass = "AUTHINFO PASS";
        internal const string AuthInfoGeneric = "AUTHINFO GENERIC"; //not currently supported.

        internal static string ConvertTimeToNNTPTimeString(DateTime time)
        {

            time = time.ToUniversalTime();
            //TODO figure out if this is in the correct format YYMMDD HHMMSS
            string year, month, day, hour, minute, second;

            hour = time.Hour.ToString(); if (hour.Length == 1) hour = "0" + hour;
            minute = time.Minute.ToString(); if (minute.Length == 1) minute = "0" + minute;
            second = time.Second.ToString(); if (second.Length == 1) second = "0" + second;

            year = time.Year.ToString().Substring(2, 2);
            month = time.Month.ToString(); if (month.Length == 1) month = "0" + month;
            day = time.Day.ToString(); if (day.Length == 1) day = "0" + day;


            string dateString = year + month + day;
            string timeString = hour + minute + second;

            return dateString + " " + timeString + " GMT ";
        }

        internal static string ListGroups(string newsgroup)
        {
            return ListGroup + " " + newsgroup + "\r\n";
        }

        internal static string ListNewsGroupsSince(DateTime time)
        {
            //NEWSGROUPS date time GMT
            return NewsGroups + " " + ConvertTimeToNNTPTimeString(time) + "\r\n";
        }

        internal static string GetNewArticles(string newsgroups, DateTime time)
        {
            return NewNews + " " + ConvertTimeToNNTPTimeString(time) + "\r\n";
        }

        internal static string SelectGroup(string newsgroup)
        {
            return Group + " " + newsgroup + "\r\n";
        }

        internal static string GetArticle(long articleIndex)
        {
            return Article + " " + articleIndex + "\r\n";
        }

        internal static string GetArticle(string articleIndex)
        {
            return Article + " " + articleIndex + "\r\n";
        }

        internal static string GetArticleHead(string articleIndex)
        {
            return Head + " " + articleIndex + "\r\n";
        }

        internal static string GetArticleBody(string articleIndex)
        {
            return Body + " " + articleIndex + "\r\n";
        }

        internal static string GetAuthInfoUser(string Username)
        {
            return AuthInfoUser + " " + Username + "\r\n";
        }

        internal static string GetAuthInfoPass(string password)
        {
            return AuthInfoPass + " " + password + "\r\n";
        }

        internal static string RequestToPostArticle(string newsgroup)
        {
            return Post + " " + newsgroup + "\r\n";
        }

        internal static string PostArticle(string from, string newsgroup, string subject, string content)
        {
            //TODO: make this alot more robust!!!
            return "From: " + from + "\r\n"
               + "Newsgroups: " + newsgroup + "\r\n"
               + "Subject: " + subject + "\r\n\r\n"
               + content + "\r\n.\r\n";
        }
    }
}
