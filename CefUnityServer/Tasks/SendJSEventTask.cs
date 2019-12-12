using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefUnityLib;
namespace CefUnityServer.Tasks
{
    public class SendJSEventTask : ITaskRunnable
    {
        protected string gameObject;
        protected string method;
        protected string param;
        public SendJSEventTask(string msg)
        {
            this.method = msg;
        }
        public SendJSEventTask(string gameObject, string method, string param = null)
        {
            this.gameObject = gameObject;
            this.method = method;
            this.param = param;
        }
        public void Run(BrowserHost host, PipeServer server)
        {
            string data = "{\"gameObject\" : \"" + gameObject+ "\",\"method\" :\"" + method+ "\",\"param\" :\"" + param + "\"}";
            server.SendData(PipeProto.BytesToProtoMessage(Encoding.UTF8.GetBytes(data), PipeProto.OPCODE_SCRIPT));
        }
    }
}
