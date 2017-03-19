using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.ComponentModel;

namespace Scarlett.Danbooru.Boorugrab.Engine
{
    delegate void DownloadCompleteEvent();
    class Manager
    {
        public event DownloadProgressEvent DownloadProgressUpdate;
        public event DownloadCompleteEvent DownloadComplete;

        Slave slave;
        LinkedList<Post> posts = new LinkedList<Post>();

        public Manager(string username, string apiKey)
        {
            slave = new Slave(username, apiKey);
            slave.UserThrottled += () =>
                DownloadProgressUpdate(new DownloadProgressEventArgs(message: "Account throttled. Trying again in a while...", literal: true));
            slave.NewPostsAdded += () =>
                DownloadProgressUpdate(new DownloadProgressEventArgs(message: "Fetched another batch of posts.", literal: true));
            slave.FinishedAddingPosts += () =>
                DownloadProgressUpdate(new DownloadProgressEventArgs(message: "Finished fetching posts.", literal: true));
        }

        private void RunInBackground(DoWorkEventHandler action, object argument = null, RunWorkerCompletedEventHandler callback = null)
        {
            BackgroundWorker bgWorker;
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(action);
            if (callback != null)
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(callback);
            bgWorker.RunWorkerAsync(argument);
        }

        public void DownloadAsync(string saveFolder, string tagString)
        {
            if (Directory.Exists(saveFolder))
                RunInBackground((s, e) => DownloadLoop(saveFolder, tagString));
            else
                throw new ArgumentException("Directory does not exist");
        }

        private async Task DownloadLoop(string saveFolder, string tagString)
        {
            DownloadProgressUpdate(new DownloadProgressEventArgs(message: "Download started", literal: true));
            bool complete = false;
            int current = 0;
            slave.FinishedAddingPosts += () => complete = true;
            RunInBackground((s, e) => slave.GetPostsAsync(posts, tagString));
            while (posts.Count > 0 || !complete)
            {
                if(posts.Count > 0)
                {
                    Post post = posts.First();
                    try
                    {
                        slave.DownloadPost(post, saveFolder);
                        posts.RemoveFirst();
                        DownloadProgressUpdate(new DownloadProgressEventArgs(message: post.Hash, complete: ++current, total: posts.Count));
                    }
                    catch(Exception ex)
                    {
                        DownloadProgressUpdate(new DownloadProgressEventArgs(message: String.Format("Skipped #{0}; {1}", ++current, ex.Message), literal:true));
                        posts.RemoveFirst();
                    }
                }
                else
                    Thread.Sleep(1000);
            }
            DownloadProgressUpdate(new DownloadProgressEventArgs(message: "Download complete", literal: true));
            DownloadComplete();
        }
    }
}
