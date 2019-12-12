using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefUnityLib.Messages;
namespace CefUnityServer.Tasks
{
    public class ResizeTask : ITaskRunnable
    {
        public int width=1024;
        public int height=768;
        public ResizeTask(string ssize)
        {
            string[] arr = ssize.Split('x');
            if(arr.Length == 2)
            {
                int.TryParse(arr[0], out width);
                int.TryParse(arr[1], out height);
            }
            width = Math.Min(2048, Math.Max(256, width));
            height = Math.Min(2048, Math.Max(256, height));
        }
        public void Run(BrowserHost host, PipeServer server)
        {
            host.Resize(width, height);
        }
    }
}
