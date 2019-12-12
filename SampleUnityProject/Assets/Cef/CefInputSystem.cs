////////////////////////////////////////////////////////////////////////////
//  EditorPro Source File.
//  Copyright (C), Rolling Ant.
// -------------------------------------------------------------------------
//  File name:	UIInputSystem.cs
//  Version:	v1.00
//  Created:	12/12/2019 3:12:21 PM
//  Author:		NAMIPROJ\quoctrung
//  Description:	
////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CefUnityLib.Messages;
using CefUnityLib.Helpers;
using CefUnityLib;
namespace RACef
{
    public class CefInputSystem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
    {
		public static CefInputSystem current;
        protected RectTransform rectTransform;
        protected CefController cef;
        protected bool isFocus = false;
        protected Camera cam;
        public Vector2Int resolution;
        public Vector2Int screensize;
		private void Awake() {
            rectTransform = GetComponent<RectTransform>();
            cam = rectTransform.gameObject.GetComponentInParent<Canvas>().worldCamera;
            screensize = new Vector2Int((int)rectTransform.rect.width,(int)rectTransform.rect.height);
            resolution = new Vector2Int((int)rectTransform.rect.width,(int)rectTransform.rect.height);
			current = this;
		}
        public void SetController(CefController cef)
        {
            this.cef = cef;
        }
        public void SetResolution(int width,int height)
        {
            resolution = new Vector2Int(width,height);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (cef == null)
                return;
            CefMouseInput(MouseEventPipeMessage.TYPE_MOUSE_DOWN, eventData);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (cef == null)
                return;
            CefMouseInput(MouseEventPipeMessage.TYPE_MOUSE_UP, eventData);
        }
        public void OnScroll(PointerEventData eventData)
        {
            if (cef == null)
                return;
            Vector2Int pos = GetPointerPosition(eventData.position);            
            cef.SendMouseWheelEvent(pos.x, pos.y, (int)eventData.scrollDelta.y * 10);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (cef == null)
                return;
            isFocus = true;
            if (Input.mousePresent)
                StartCoroutine(TrackOnMouseMove());
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            isFocus = false;
        }
        Vector2Int GetPointerPosition(Vector2 position){
            Vector2 pos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, cam, out pos);
            pos.x = pos.x/screensize.x*resolution.x;
            pos.y = -pos.y/screensize.y*resolution.y;
            return new Vector2Int((int)pos.x,(int)pos.y);
        }
        void CefMouseInput(byte eventType, PointerEventData eventData)
        {
            MouseButtons btn = CefUnityLib.Helpers.MouseButtons.None;
            switch (eventData.pointerId)
            {
                case -1:
                    btn = CefUnityLib.Helpers.MouseButtons.Left;
                    break;
                case -2:
                    btn = CefUnityLib.Helpers.MouseButtons.Right;
                    break;
                case -3:
                    btn = CefUnityLib.Helpers.MouseButtons.Middle;
                    break;
                default:
                    break;
            }

            Vector2Int pos = GetPointerPosition(eventData.position);                        
            // Debug.Log(pos.ToString()+"------"+eventData.position.ToString());
            cef.SendMouseEvent(eventType, pos.x, pos.y, btn);
        }

        IEnumerator TrackOnMouseMove()
        {
            while (isFocus)
            {
                var position = Input.mousePosition;
                MouseButtons btn = CefUnityLib.Helpers.MouseButtons.None;
                if (Input.GetMouseButton(0))
                    btn = CefUnityLib.Helpers.MouseButtons.Left;
                if (Input.GetMouseButton(1))
                    btn = CefUnityLib.Helpers.MouseButtons.Right;
                if (Input.GetMouseButton(2))
                    btn = CefUnityLib.Helpers.MouseButtons.Middle;
                Vector2Int pos = GetPointerPosition(position);
                cef.SendMouseEvent(MouseEventPipeMessage.TYPE_MOVE,pos.x, pos.y, btn);
                yield return null;
            }
        }

        private void OnGUI()
        {
            if (!isFocus)
                return;
            Event e = Event.current;
            if (e.isKey || e.functionKey)
            {
                cef.SendKeyCharEvent(e.character);
                CefUnityLib.Helpers.UKeys key = CefUnityLib.Helpers.UKeys.None;
                CefUnityLib.Helpers.UKeys modifiers = CefUnityLib.Helpers.UKeys.None;
                Enum.TryParse(e.keyCode.ToString(), out key);
                Enum.TryParse(e.modifiers.ToString(), out modifiers);
                switch (e.type)
                {
                    case EventType.KeyDown:
                        // Debug.Log(e.keyCode+"---"+key+"---"+e.modifiers+"---"+modifiers);
                        cef.SendKeyEvent(KeyEventPipeMessage.TYPE_KEY_DOWN, (CefUnityLib.Helpers.Keys)key, (CefUnityLib.Helpers.Keys)modifiers);
                        break;
                    case EventType.KeyUp:
                        cef.SendKeyEvent(KeyEventPipeMessage.TYPE_KEY_UP, (CefUnityLib.Helpers.Keys)key, (CefUnityLib.Helpers.Keys)modifiers);
                        break;
                }
            }
        }
    }
}