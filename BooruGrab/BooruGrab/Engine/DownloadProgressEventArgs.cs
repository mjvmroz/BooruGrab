using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scarlett.Danbooru.Boorugrab.Engine
{
    delegate void DownloadProgressEvent(DownloadProgressEventArgs e);
    class DownloadProgressEventArgs
    {
        public int Complete { get; private set; }
        public int Total { get; private set; }
        public string Message { get; private set; }
        public bool MessageIsLiteral { get; private set; }

        public DownloadProgressEventArgs(int complete=0, int total=0, string message="", bool literal=false)
        {
            Complete = complete;
            Total = total;
            Message = message;
            MessageIsLiteral = literal;
        }

        public override string ToString()
        {
            string result = "";
            if (!MessageIsLiteral)
                result = String.Format("{0} complete, {1} in queue. Just finished {2}", Complete, Total, Message);
            else result = Message;
            return result;
        }

        public static implicit operator string(DownloadProgressEventArgs e)
        {
            return e.ToString();
        }
    }
}
