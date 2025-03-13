using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SO_Events.Runtime.Editor
{
    /// <summary>
    /// A custom Editor window to manage ScriptableObject Game Events.
    /// This allows creating new events, deleting them, and dragging them to the Inspector for assignment.
    /// </summary>
    public class EventManagementWindow : EditorWindow
    {
        private const string DefaultFolderName = "Event Instances";
        private const string FolderPathPrefKey = "EventManagementWindow_TargetPath";

        // The target folder where new event instances will be created.
        private string _targetFolderPath;
    
        private Vector2 _scrollPos;
        private List<ScriptableObject> _allEvents = new List<ScriptableObject>();

        private string _newEventName = "NewEvent";
        private int _selectedEventTypeIndex = 0;
        private string[] _eventTypeNames;
        private Type[] _eventConcreteTypes;

        [MenuItem("Window/Event Management")]
        public static void OpenWindow()
        {
            GetWindow<EventManagementWindow>("Event Manager");
        }

        private void OnEnable()
        {
            // Determine the default folder path (sibling to the script folder)
            if (string.IsNullOrEmpty(_targetFolderPath))
            {
                var scriptPath = GetScriptFolderPath();
                _targetFolderPath = System.IO.Path.Combine(scriptPath, DefaultFolderName);

                // Check if a saved path exists in EditorPrefs and use it
                if (EditorPrefs.HasKey(FolderPathPrefKey))
                {
                    _targetFolderPath = EditorPrefs.GetString(FolderPathPrefKey);
                }
            }

            PopulateEventTypes();
            RefreshEventList();
        }

        private void OnGUI()
        {
            GUILayout.Label("Event Management", EditorStyles.boldLabel);

            // -------------------------
            // Target Folder Section
            // -------------------------
            GUILayout.Label("Target Folder", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Events will be created here:");
            EditorGUILayout.LabelField(_targetFolderPath, EditorStyles.textField);

            if (GUILayout.Button("Change Target Folder"))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Target Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Convert the path to a Unity-relative asset path
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        _targetFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        EditorPrefs.SetString(FolderPathPrefKey, _targetFolderPath); // Save to EditorPrefs
                    }
                    else
                    {
                        Debug.LogError("Target folder must be inside the project’s Assets folder.");
                    }
                }
            }

            EditorGUILayout.Space(10);

            // -------------------------
            // Creation Section
            // -------------------------
            GUILayout.Label("Create a New Event", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                _newEventName = EditorGUILayout.TextField("Event Name", _newEventName);
                _selectedEventTypeIndex = EditorGUILayout.Popup(_selectedEventTypeIndex, _eventTypeNames);
            }

            if (GUILayout.Button("Create Event"))
            {
                CreateNewEvent(_newEventName, _eventConcreteTypes[_selectedEventTypeIndex]);
                RefreshEventList();
            }

            EditorGUILayout.Space(10);

            // -------------------------
            // Existing Events List
            // -------------------------
            GUILayout.Label("All Existing Events", EditorStyles.miniBoldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            {
                foreach (var evt in _allEvents)
                {
                    if (evt == null)
                        continue;
                
                    // Begin a horizontal area for each event.
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // Reserve a rect for the object field.
                        Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);
                    
                        // Draw the ObjectField as a disabled field so it appears read-only.
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width - 70, rect.height), evt, typeof(ScriptableObject), false);
                        EditorGUI.EndDisabledGroup();
                    
                        // Draw the delete button.
                        if (GUI.Button(new Rect(rect.xMax - 60, rect.y, 60, rect.height), "Delete"))
                        {
                            if (EditorUtility.DisplayDialog("Confirm Deletion",
                                    $"Delete event '{evt.name}' permanently?",
                                    "Delete", "Cancel"))
                            {
                                DeleteEvent(evt);
                                RefreshEventList();
                                break;
                            }
                        }
                    
                        // Make the left part of the rect draggable.
                        // We listen for a MouseDrag event within this rectangle.
                        if (Event.current.type == EventType.MouseDrag &&
                            new Rect(rect.x, rect.y, rect.width - 70, rect.height).Contains(Event.current.mousePosition))
                        {
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new UnityEngine.Object[] { evt };
                            // The title shown in the visual drag proxy.
                            DragAndDrop.StartDrag("Dragging " + evt.name);
                            Event.current.Use();
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Refresh List"))
            {
                RefreshEventList();
            }
        }

        /// <summary>
        /// Finds all derived event types (e.g., FloatGameEvent, IntGameEvent) for the creation dropdown.
        /// </summary>
        private void PopulateEventTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> foundTypes = new List<Type>();

            foreach (var asm in assemblies)
            {
                var types = asm.GetTypes();
                foreach (var t in types)
                {
                    if (t.IsAbstract) continue;
                    if (!t.IsSubclassOf(typeof(ScriptableObject))) continue;
                    if (t.BaseType != null && t.BaseType.Name.StartsWith("BaseGameEvent"))
                    {
                        foundTypes.Add(t);
                    }
                }
            }

            _eventConcreteTypes = foundTypes.ToArray();
            _eventTypeNames = foundTypes.Select(t => t.Name).ToArray();
        }

        /// <summary>
        /// Refresh the list of existing events.
        /// </summary>
        private void RefreshEventList()
        {
            _allEvents.Clear();
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                if (asset.GetType().BaseType != null && asset.GetType().BaseType.Name.StartsWith("BaseGameEvent"))
                {
                    _allEvents.Add(asset);
                }
            }
            _allEvents = _allEvents.OrderBy(a => a.name).ToList();
        }

        /// <summary>
        /// Create a new ScriptableObject event instance.
        /// </summary>
        private void CreateNewEvent(string eventName, Type eventType)
        {
            if (!AssetDatabase.IsValidFolder(_targetFolderPath))
            {
                Debug.LogError($"Target folder '{_targetFolderPath}' is invalid. Ensure the folder exists.");
                return;
            }

            ScriptableObject newEvent = ScriptableObject.CreateInstance(eventType);
            newEvent.name = eventName;

            string path = Path.Combine(_targetFolderPath, $"{eventName}.asset");
            AssetDatabase.CreateAsset(newEvent, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Deletes the selected event from the project.
        /// </summary>
        private void DeleteEvent(ScriptableObject evt)
        {
            string assetPath = AssetDatabase.GetAssetPath(evt);
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Returns the folder path of this script file (Editor folder location).
        /// </summary>
        private string GetScriptFolderPath()
        {
            // Try to locate the script within the "Assets" folder first
            string thisScriptGUID = AssetDatabase.FindAssets("EventManagementWindow").FirstOrDefault();
            if (!string.IsNullOrEmpty(thisScriptGUID))
            {
                string path = AssetDatabase.GUIDToAssetPath(thisScriptGUID);
                return System.IO.Path.GetDirectoryName(path); // Path under "Assets/"
            }

            // Check if the script is inside the "Packages" folder
            string packagePath = GetPackagePath("com.yourcompany.eventmanagement");
            if (!string.IsNullOrEmpty(packagePath))
            {
                return System.IO.Path.Combine(packagePath, "Editor"); // Editor folder inside package
            }

            // Default fallback
            Debug.LogError("Could not find EventManagementWindow script location.");
            return "Assets";
        }
        
        private string GetPackagePath(string packageName)
        {
            string packageInfoFile = System.IO.Path.Combine("Packages", packageName, "package.json");
            if (System.IO.File.Exists(packageInfoFile))
            {
                return System.IO.Path.GetDirectoryName(packageInfoFile);
            }

            Debug.LogError($"Package '{packageName}' not found!");
            return null;
        }
    }
}