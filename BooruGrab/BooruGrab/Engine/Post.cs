using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scarlett.Danbooru.Boorugrab.Engine
{
    class Post
    {
        public string WebLocation;
        public string[] Tags;
        public string Hash;

        public string FileName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(WebLocation) + ".jpg";
            }
        }

    }
}
