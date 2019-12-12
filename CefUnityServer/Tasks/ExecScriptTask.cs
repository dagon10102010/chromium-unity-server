using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CefUnityServer.Tasks
{
    public class ExecScriptTask:ITaskRunnable
    {
        protected string scriptdata;
        public ExecScriptTask(string scriptdata)
        {
            this.scriptdata = scriptdata;
        }
        public void Run(BrowserHost host, PipeServer server)
        {
            host.RunScript(scriptdata);
        }
    }
}
