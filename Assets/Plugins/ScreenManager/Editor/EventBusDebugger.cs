using System;
using System.Collections.Generic;
using SimpleEventBus.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Plugins.ScreenManager.Editor
{
    public class EventBusDebugger : EditorWindow
    {
        [MenuItem("Window/ScreenManager/EventBus Debugger", false, 3000)]
        private static void OpenWindow()
        {
            var eventBusDebugger = GetWindow<EventBusDebugger>();
            eventBusDebugger.Show();
        }

        private Dictionary<Type, bool> _typeFoldouts = new Dictionary<Type, bool>();
        private Vector2 _scrollPosition;
        
        private void OnGUI()
        {
            var userInterface = EventStreams.Game as IDebuggableEventBus;
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var subscription in userInterface.Subscriptions)
            {
                if (subscription.Value.Count == 0)
                {
                    continue;
                }

                var foldoutName = $"[{subscription.Key.Name}] ({subscription.Value.Count})";
                var foldoutValue = _typeFoldouts.ContainsKey(subscription.Key) && _typeFoldouts[subscription.Key];
                _typeFoldouts[subscription.Key] = EditorGUILayout.Foldout(foldoutValue, foldoutName);
                
                if (_typeFoldouts[subscription.Key])
                {                
                    EditorGUI.indentLevel++;

                    foreach (var holder in subscription.Value)
                    {
                        //var action = holder.GetType().GetAllFields().Find(field => field.Name == "_action");
                        //var actionCallback = action.GetValue(holder) as Delegate;
                        //
                        //EditorGUILayout.LabelField($"{actionCallback.Target.GetType().Name} => {actionCallback.Method.Name} ");
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void Update()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                Repaint();
            }
        }
    }
}