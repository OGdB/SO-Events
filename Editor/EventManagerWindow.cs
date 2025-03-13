using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SO_Events.Editor
{
    /// <summary>
    /// A custom Editor window to manage ScriptableObject Game Events.
    /// This allows creating new events, deleting them, and dragging them to the Inspector for assignment.
    /// </summary>
    public class EventManagementWindow : EditorWindow
    {
        private const string DefaultFolderName = "Event Instances";
        private const string DefaultTargetFolder = "Assets/Packages/Event System/Event Instances";
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
            // Determine the default folder path
            if (string.IsNullOrEmpty(_targetFolderPath))
            {
                _targetFolderPath = DefaultTargetFolder;

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

    // Allow the user to manually edit the folder path
    string newFolderPath = EditorGUILayout.TextField(_targetFolderPath);

    // Validate and save the new path
    if (newFolderPath != _targetFolderPath)
    {
        if (newFolderPath.StartsWith("Assets"))
        {
            _targetFolderPath = newFolderPath;
            EditorPrefs.SetString(FolderPathPrefKey, _targetFolderPath); // Persist the path
        }
        else
        {
            Debug.LogError("The folder path must be inside the 'Assets' folder.");
        }
    }

    // Button to open folder picker
    if (GUILayout.Button("Change Target Folder"))
    {
        // Force the current UI element (e.g., TextField) to lose focus
        GUI.FocusControl(null);

        // Open the folder selection panel
        string selectedPath = EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, "");

        // Check if a valid path was selected
        if (!string.IsNullOrEmpty(selectedPath))
        {
            // Convert absolute path to a Unity-relative path (e.g., "Assets/...").
            if (selectedPath.StartsWith(Application.dataPath))
            {
                // Strip off the Application.dataPath part to get relative path
                _targetFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length).Replace("\\", "/");
                EditorPrefs.SetString(FolderPathPrefKey, _targetFolderPath); // Save to EditorPrefs
                Debug.Log($"Target folder changed to: {_targetFolderPath}");
            }
            else
            {
                // Path is outside the project
                Debug.LogError("Target folder must be inside the project’s `Assets` folder.");
            }
        }
    }

    EditorGUILayout.Space(10);

    // Rest of the GUI (event creation and list)
    GUILayout.Label("Create a New Event", EditorStyles.miniBoldLabel);
    using (new EditorGUILayout.HorizontalScope())
    {
        _newEventName = EditorGUILayout.TextField("Event Name", _newEventName);
        _selectedEventTypeIndex = EditorGUILayout.Popup(_selectedEventTypeIndex, _eventTypeNames);
    }

    if (GUILayout.Button("Create Event"))
    {
        GUI.FocusControl(null);
        CreateNewEvent(_newEventName, _eventConcreteTypes[_selectedEventTypeIndex]);
        RefreshEventList();
    }

    EditorGUILayout.Space(10);

    GUILayout.Label("All Existing Events", EditorStyles.miniBoldLabel);
    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
    {
        foreach (var evt in _allEvents)
        {
            if (!evt) continue;
            using (new EditorGUILayout.HorizontalScope())
            {
                Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight);

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width - 70, rect.height), evt, typeof(ScriptableObject), false);
                EditorGUI.EndDisabledGroup();

                if (GUI.Button(new Rect(rect.xMax - 60, rect.y, 60, rect.height), "Delete"))
                {
                    if (EditorUtility.DisplayDialog("Confirm Deletion", $"Delete event '{evt.name}' permanently?", "Delete", "Cancel"))
                    {
                        DeleteEvent(evt);
                        RefreshEventList();
                        break;
                    }
                }

                if (Event.current.type == EventType.MouseDrag && new Rect(rect.x, rect.y, rect.width - 70, rect.height).Contains(Event.current.mousePosition))
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new Object[] { evt };
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
            // Ensure the target folder exists
            EnsureFolderExists(_targetFolderPath);

            // Create the ScriptableObject instance
            ScriptableObject newEvent = CreateInstance(eventType);
            newEvent.name = eventName;

            // Save it as an asset in the target folder
            string path = Path.Combine(_targetFolderPath, $"{eventName}.asset");
            AssetDatabase.CreateAsset(newEvent, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created new event: {path}");
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
                return Path.GetDirectoryName(path); // Path under "Assets/"
            }

            // Check if the script is inside the "Packages" folder
            string packagePath = GetPackagePath("com.oeds.eventmanagement");
            if (!string.IsNullOrEmpty(packagePath))
            {
                return Path.Combine(packagePath, "Editor"); // Editor folder inside package
            }

            // Default fallback
            Debug.LogError("Could not find EventManagementWindow script location.");
            return "Assets";
        }
        
        private string GetPackagePath(string packageName)
        {
            string packageInfoFile = Path.Combine("Packages", packageName, "package.json");
            if (File.Exists(packageInfoFile))
            {
                return Path.GetDirectoryName(packageInfoFile);
            }

            Debug.LogError($"Package '{packageName}' not found!");
            return null;
        }
        
        /// <summary>
        /// Ensures that the target folder exists by creating it if it doesn't.
        /// </summary>
        /// <param name="folderPath">The full Unity asset path (e.g., "Assets/MyFolder/SubFolder")</param>
        private void EnsureFolderExists(string folderPath)
        {
            // Start with a normalized path to prevent issues with slashes
            folderPath = folderPath.Replace("\\", "/");

            // Check if the folder already exists
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            // Split the folder path into individual parts (e.g., "Assets", "MyFolder", "SubFolder")
            string[] folders = folderPath.Split('/');

            // Start building the folder hierarchy from "Assets"
            string currentPath = folders[0]; // This should always be "Assets" as the root

            for (int i = 1; i < folders.Length; i++)
            {
                string newFolder = $"{currentPath}/{folders[i]}";

                // Create the folder if it doesn't already exist
                if (!AssetDatabase.IsValidFolder(newFolder))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                    AssetDatabase.SaveAssets();
                }

                // Update the current path
                currentPath = newFolder;
            }
        }
    }
}