////////////////////////////////////////////////////////////////////////////
//  EditorPro Source File.
//  Copyright (C), Rolling Ant.
// -------------------------------------------------------------------------
//  File name:	CefIntegrationBehavior.cs
//  Version:	v1.00
//  Created:	12/11/2019 1:26:06 PM
//  Author:		NAMIPROJ\quoctrung
//  Description:	
////////////////////////////////////////////////////////////////////////////
using CefUnityLib;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
namespace RACef
{
    public class CefBehavior : MonoBehaviour
    {
        public RawImage targetImage;
        protected CefController cef;
        protected Texture2D browserTexture;

        protected byte[] frameBuffer;
        protected bool frameBufferChanged;
        protected bool sizeChanged = true;
        protected int width = 1024;
        protected int height = 768;
        protected int tempImageLength;

        public string PipeName = "default";
        private System.Diagnostics.Process process;
        void Start()
        {
            // targetImage = GetComponent<RawImage>();
            if(!targetImage.GetComponent<CefInputSystem>()){
                targetImage.gameObject.AddComponent<CefInputSystem>();
            }
            browserTexture = new Texture2D(width, height, TextureFormat.BGRA32, false);
            tempImageLength = browserTexture.GetRawTextureData().Length;
            cef = new CefController(PipeName);

            if (StartAndConnectServer())
            {
                CefInputSystem.current.SetController(cef);
                UnityEngine.Debug.Log("[CEF] Connected to proxy server process.");
            }

            targetImage.texture = browserTexture;
            targetImage.uvRect = new Rect(0f, 0f, 1f, -1f);
            targetImage.rectTransform.pivot = new Vector2(0, 1);
            cef.MessageReceived += OnCefMessage;

            frameBuffer = new byte[0];
            frameBufferChanged = false;
        }

        protected bool StartAndConnectServer()
        {
            // First connection attempt
            try
            {
                cef.Connect();
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("[CEF] Proxy server not responding, attempting to start server executable. Connection error details: " + e.Message);
            }

            // Determine path to CEF Unity Server
            //string cefPath = Application.dataPath;
            string cefPath = System.IO.Path.Combine(Application.streamingAssetsPath,"Cef");
            string cefPathExec = cefPath + "/CefUnityServer.exe";

            // Start the process, hide it, and listen to its output
            var processInfo = new System.Diagnostics.ProcessStartInfo();
            processInfo.Arguments = string.Format("{0} {1} {2}",cef.PipeName,"google.com",width+"x"+height);
            processInfo.CreateNoWindow = true;
            processInfo.FileName = cefPathExec;
            processInfo.WorkingDirectory = cefPath;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            process = System.Diagnostics.Process.Start(processInfo);
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;

            // Basic wait time to let the server start (usually takes a quarter second or so on a reasonable machine)
            Thread.Sleep(250);

            // Wait for the app to start - as long as it doesn't fail and we don't exceed a certain timeout
            int attemptsRemaining = 10;
            Exception lastEx = null;

            do
            {
                try
                {
                    // Connect - if okay, break out and proceed
                    cef.Connect();
                    return true;
                }
                catch (Exception ex)
                {
                    // Connect failed, wait a bit and try again
                    UnityEngine.Debug.Log("[CEF] Proxy server not responding. {0} attempt(s) remaining. Connection error details: " + ex.Message);

                    attemptsRemaining--;
                    lastEx = ex;

                    if (attemptsRemaining <= 0)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            while (true);

            UnityEngine.Debug.Log("[CEF] Proxy server failed to start! (Hard failure)");
            throw lastEx;
        }
        private void OnApplicationQuit()
        {
            if (process != null && !process.HasExited)
                process.Kill();
        }
        private void Process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
            // Debug.Log(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
            // Debug.Log(e.Data);
        }

        protected void OnCefMessage(object sender, PipeProtoMessage p)
        {
            switch (p.Opcode)
            {
                case PipeProto.OPCODE_FRAME:

                    frameBuffer = p.Payload;
                    frameBufferChanged = true;
                    break;
                case PipeProto.OPCODE_SCRIPT:
                    var msg = Encoding.UTF8.GetString(p.Payload);
                    Debug.Log(msg);
                    break;
            }
        }
        void Update()
        {
            if (frameBufferChanged)
            {
                if (sizeChanged)
                {
                    if (frameBuffer.Length == tempImageLength)
                    {
                        browserTexture = new Texture2D(width, height, TextureFormat.BGRA32, false);
                        targetImage.texture = browserTexture;
                        CefInputSystem.current.SetResolution(width,height);
                        sizeChanged = false;
                    }
                    // else
                    // Debug.Log(frameBuffer.Length + " == " + tempImageLength);

                }
                else
                {
                    browserTexture.LoadRawTextureData(frameBuffer);
                    browserTexture.Apply();
                    frameBufferChanged = false;
                }

            }
        }
        public void LoadPage(string url)
        {
            var msg = new PipeProtoMessage() { Opcode = PipeProto.OPCODE_NAVIGATE, Payload = System.Text.Encoding.UTF8.GetBytes(url) };
            cef.SendMessage(msg);
        }
        public void TestResizePage()
        {
            ResizePage(800, 600);
        }
        public void ResizePage(int width, int height)
        {
            var msg = new PipeProtoMessage() { Opcode = PipeProto.OPCODE_RESIZE, Payload = System.Text.Encoding.UTF8.GetBytes(width + "x" + height) };
            cef.SendMessage(msg);
            this.width = width;
            this.height = height;
            var tex = new Texture2D(width, height, TextureFormat.BGRA32, false);
            tempImageLength = tex.GetRawTextureData().Length;
            tex = null;
            sizeChanged = true;
        }
        public void ExecJSScript(string data)
        {
            var msg = new PipeProtoMessage() { Opcode = PipeProto.OPCODE_SCRIPT, Payload = System.Text.Encoding.UTF8.GetBytes(data) };
            cef.SendMessage(msg);
        }
    }
}