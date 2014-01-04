using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Scarlett.Danbooru.Boorugrab.Engine
{
    delegate void PostFetchEvent();
    class Slave
    {
        const string danbooruBase = @"http://danbooru.donmai.us/";
        string postSearchUrl
        {
            get
            {
                return danbooruBase + @"post/index.xml?login={0}&api_key={1}&limit=100&page={3}&tags={2}";
            }
        }
        const int userThrottledCode = 421; //As per Danbooru API
        const int throttledSleepTime = 1200000; //20 minutes
        string[] supportedExtensions = { ".png", ".jpg", ".jpeg" };

        public event PostFetchEvent NewPostsAdded;
        public event PostFetchEvent FinishedAddingPosts;
        public event PostFetchEvent UserThrottled;

        string username;
        string apiKey;

        private string getExtension(string url)
        {
            return Path.GetExtension(url).ToLower();
        }

        public Slave(string username, string apiKey)
        {
            this.username = username;
            this.apiKey = apiKey;
        }

        public async Task GetPostsAsync(LinkedList<Post> postList, string tagString)
        {
            WebClient client = new WebClient();
            int currentPage = 1;
            bool complete = false;
            while(!complete)
            {
                try
                {
                    string xml = client.DownloadString(String.Format(postSearchUrl, username, apiKey, HttpUtility.UrlPathEncode(tagString), currentPage));
                    IEnumerable<XElement> xmlPosts = XElement.Parse(xml).Descendants("post");
                    foreach (var xmlPost in xmlPosts)
                    {
                        Post post = new Post()
                        {
                            Hash = xmlPost.Attribute("md5").Value.Trim(),
                            Tags = xmlPost.Attribute("tags").Value.Trim().Split(" ".ToCharArray()),
                            WebLocation = danbooruBase + xmlPost.Attribute("file_url").Value.Trim()
                        };
                        if (supportedExtensions.Contains(getExtension(post.WebLocation)))
                            postList.AddLast(post);
                    }
                    complete = xmlPosts.Count() == 0;
                    currentPage++;
                }
                catch (WebException ex)
                {
                    if ((int)(ex.Response as HttpWebResponse).StatusCode == 421)
                    {
                        UserThrottled();
                        Thread.Sleep(throttledSleepTime);
                    }
                    else throw;
                }
            }
            FinishedAddingPosts();
        }

        public void DownloadPost(Post post, string saveFolder)
        {
            var request = WebRequest.Create(post.WebLocation);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var tagCollection = new ReadOnlyCollection<string>(post.Tags.Select(t => t.Replace(';', '_')).ToList());
                BitmapDecoder decoder;
                if(Path.GetExtension(post.WebLocation).ToLower() == ".png")
                    decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                else
                    decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                var fs = new FileStream(Path.Combine(saveFolder, String.Format("{0}.jpg", post.Hash)), FileMode.Create);
                var encoder = new JpegBitmapEncoder();

                var meta = new BitmapMetadata("jpg");
                meta.Keywords = tagCollection;
                meta.Title = post.Hash;

                encoder.Frames.Add(
                    BitmapFrame.Create(
                    decoder.Frames[0],
                    decoder.Frames[0].Thumbnail,
                    meta,
                    decoder.Frames[0].ColorContexts
                    ));
                //0cb13ad7d74338831fee8141dbebec61
                encoder.Save(fs);
                fs.Close();
            }
        }
    }
}
