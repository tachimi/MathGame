using System;
using System.Collections.Generic;
using System.Linq;
using ScreenManager.Enums;
using ScreenManager.Events;
using SimpleBus.Extensions;
using UnityEditor;
using UnityEngine;

namespace ScreenManager.Editor
{
    public class ScreenManagerDebugger : EditorWindow
    {
        private bool _showStack = true;
        private bool _showCache = true;
        private bool _showQueue = true;
        private bool _showAppearingRequests = true;
        private readonly Dictionary<ScreenId, bool> _queueFoldouts = new Dictionary<ScreenId, bool>();
        private readonly Dictionary<int, bool> _appearingRequestsFoldouts = new Dictionary<int, bool>();
        private IDisposable _subscription;
        private ScreenId _topScreen;

        [MenuItem("Window/ScreenManager/ScreenManager Debugger", false, 3000)]
        private static void OpenWindow()
        {
            var screenManagerDebugger = GetWindow<ScreenManagerDebugger>();
            screenManagerDebugger.Show();
        }

        private void OnEnable()
        {
            _subscription = EventStreams.Game.Subscribe<TopScreenChangedEvent>(TopScreenChangedHandler);
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
        }

        private void TopScreenChangedHandler(TopScreenChangedEvent eventData)
        {
            _topScreen = eventData.Id;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                _topScreen = ScreenId.None;
                EditorGUILayout.LabelField($"The debugger is available only when the game is started.");
                return;
            }

            var screenManagerData = new GetScreenDebugDataEvent();
            screenManagerData.Publish(EventStreams.Game);

            var getScreenManagerState = new GetScreenManagerStateEvent();
            getScreenManagerState.Publish(EventStreams.Game);

            if (getScreenManagerState.IsInProgress == null)
            {
                EditorGUILayout.LabelField($"The debugger is available only when the game is started.");
                return;
            }

            EditorGUILayout.LabelField($"[Top Screen] {_topScreen} | Is In Progress : {getScreenManagerState?.IsInProgress?.CurrentValue}");

            GUILayout.BeginVertical("box");

            _showStack = EditorGUILayout.Foldout(_showStack, "Screen Stack:");
            var screenQueue = screenManagerData.ScreenQueue;
            if (_showStack && screenManagerData.ScreenStack != null)
            {
                EditorGUI.indentLevel++;

                int index = 0;
                foreach (var screenData in screenManagerData.ScreenStack)
                {
                    var type = screenData.Id;
                    var state = screenManagerData.ScreensCache.ContainsKey(type) ?
                        screenManagerData.ScreensCache[type].State.CurrentValue.ToString() :
                        "Unknown";

                    GUILayout.BeginHorizontal();

                    var label = $"[{index}] {type} [HidePrev:{screenData.ShouldHidePrevious}, State:{state}] ({screenData.ScreenGuid})";
                    DrawQueueFoldout(type, screenQueue.ContainsKey(type) ? screenQueue[type] : null, label);

                    if (GUILayout.Button("Close", GUILayout.Width(100)))
                    {
                        CloseScreensByTypeEvent.Create(type).Publish(EventStreams.Game);
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical();
                    DrawQueue(type, screenQueue.ContainsKey(type) ? screenQueue[type] : null);
                    GUILayout.EndVertical();


                    index++;
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            _showQueue = EditorGUILayout.Foldout(_showQueue, "Screen Queue:");
            if (_showQueue && screenManagerData.ScreenQueue != null)
            {
                EditorGUI.indentLevel++;

                foreach (var pair in screenManagerData.ScreenQueue)
                {
                    // if there is no such screen in the stack then there is no reason to display it in the debugger
                    // except ScreenId.None, it we should display always
                    var isInStack = screenManagerData.ScreenStack?.Any(screen => screen.Id == pair.Key);
                    if (pair.Key != ScreenId.None && isInStack == false && pair.Value?.Count == 0)
                    {
                        continue;
                    }
                    
                    DrawQueueFoldout(pair.Key, pair.Value, null);
                    DrawQueue(pair.Key, pair.Value);
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");

            _showCache = EditorGUILayout.Foldout(_showCache, "Screen Cache:");
            if (_showCache && screenManagerData.ScreensCache != null)
            {
                EditorGUI.indentLevel++;

                foreach (var pair in screenManagerData.ScreensCache)
                {
                    var screenState = pair.Value == null ? "null" : pair.Value.State.CurrentValue.ToString();

                    EditorGUILayout.LabelField($"{pair.Key} - [{screenState}]");
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");

            _showAppearingRequests = EditorGUILayout.Foldout(_showAppearingRequests, "Appearing requests:");
            if (_showAppearingRequests && screenManagerData.AppearingScreensRequests != null)
            {
                EditorGUI.indentLevel++;

                if (screenManagerData.IsAppearingProcessorWorking)
                {
                    EditorGUILayout.LabelField("Appearing Processor is working...");
                    DrawCurrentAppearingRequestInformation(screenManagerData);
                }
                
                int index = 0;
                foreach (var request in screenManagerData.AppearingScreensRequests)
                {
                    var foldoutState = _appearingRequestsFoldouts.ContainsKey(index) && _appearingRequestsFoldouts[index];

                    _appearingRequestsFoldouts[index] = EditorGUILayout.Foldout(foldoutState,
                        $"{index} - [H:{request.ScreensToHide.Count}, S:{request.ScreensToShow.Count}, U:{request.ScreensToUnload.Count}]");

                    if (_appearingRequestsFoldouts[index])
                    {
                        EditorGUILayout.LabelField("ShowPanel:");
                        DrawList(request.ScreensToShow);
                        EditorGUILayout.LabelField("HidePanel:");
                        DrawList(request.ScreensToHide);
                        EditorGUILayout.LabelField("Unload:");
                        DrawList(request.ScreensToUnload);
                    }

                    index++;
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
        }

        private void DrawCurrentAppearingRequestInformation(GetScreenDebugDataEvent screenManagerData)
        {
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Details:");

            EditorGUILayout.LabelField("ShowPanel:");
            EditorGUI.indentLevel++;
            DrawList(screenManagerData.LastRequestInformation.Request.ScreensToShow);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("HidePanel:");
            EditorGUI.indentLevel++;
            DrawList(screenManagerData.LastRequestInformation.Request.ScreensToHide);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("Unload:");
            EditorGUI.indentLevel++;
            DrawList(screenManagerData.LastRequestInformation.Request.ScreensToUnload);
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField($"Unload screens processed: {screenManagerData.LastRequestInformation.UnloadScreensProcessed}");
            EditorGUILayout.LabelField($"HidePanel screens processed: {screenManagerData.LastRequestInformation.HideScreensProcessed}");
            EditorGUILayout.LabelField($"ShowPanel screens processed: {screenManagerData.LastRequestInformation.ShowScreensProcessed}");
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("");
        }

        private void DrawQueue(ScreenId id, List<QueueScreenData> screenQueue)
        {
            if (_queueFoldouts[id] && screenQueue != null)
            {
                EditorGUI.indentLevel++;

                var isBlocked = false;
                foreach (var queueElement in screenQueue)
                {
                    var isReadyToShow = queueElement.IsReadyToShow();
                    var isExpired = queueElement.IsExpired();

                    var defaultColor = GUI.color;
                    if (isBlocked)
                    {
                        GUI.color = Color.red;
                    }
                    
                    EditorGUILayout.LabelField($"{queueElement.ScreenData.Id}|P:{queueElement.Priority}|B:{queueElement.IsBlockingQueue} [Ready:{isReadyToShow}, Expired:{isExpired}] ({queueElement.ScreenData.ScreenGuid})");

                    GUI.color = defaultColor;
                    
                    isBlocked = isBlocked || queueElement.IsBlockingQueue;
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawQueueFoldout(ScreenId id, List<QueueScreenData> screenQueue, string customTitle)
        {
            var foldout = _queueFoldouts.ContainsKey(id) && _queueFoldouts[id];
            _queueFoldouts[id] = EditorGUILayout.Foldout(foldout, customTitle ?? $"[{id}] (Count:{screenQueue.Count})");
        }

        private void DrawList(IEnumerable<ScreenData> screens)
        {
            foreach (var screenData in screens)
            {
                EditorGUILayout.LabelField($"{screenData.Id} [HidePrev:{screenData.ShouldHidePrevious}] ({screenData.ScreenGuid})");
            }
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
