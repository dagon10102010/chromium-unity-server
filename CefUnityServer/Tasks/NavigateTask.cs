using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefUnityServer.Tasks
{
    public class NavigateTask: ITaskRunnable
    {
        protected string url;

        public NavigateTask(string url)
        {
            this.url = url;
        }
        public void Run(BrowserHost host, PipeServer server)
        {
            Logr.Log("Received NavigateTask request from network.");
            host.LoadPageAsync(url);
        }
    }
}
