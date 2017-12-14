using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usenet
{
    public class NewsGroup
    {
        string _serverName = null;
        public string ServerName
        {
            get { return _serverName; }
        }

        string _name = null;
        public string Name
        {
            get { return _name; }
        }

        bool _acceptsPosts = false;
        public bool AcceptsPosts
        {
            get { return _acceptsPosts; }
        }

        string _startIndex = null;
        public string StartIndex
        {
            get { return _startIndex; }
        }

        string _stopIndex = null;
        public string StopIndex
        {
            get { return _stopIndex; }
        }

        public NewsGroup(string serverName, string response)
        {
            string[] values = response.Split(" ".ToCharArray());

            if (values.Length < 4)
            {
                return;
            }

            _serverName = serverName;
            _name = values[0];
            _startIndex = values[2];
            _stopIndex = values[1];
            _acceptsPosts = (values[3].ToLower() == "y");
        }

        private NewsGroup(string serverName, string name, string startIndex, string stopIndex, bool acceptsPosts)
        {
            _serverName = serverName;
            _acceptsPosts = acceptsPosts;
            _name = name;
            _startIndex = startIndex;
            _stopIndex = stopIndex;
        }
    }
}
