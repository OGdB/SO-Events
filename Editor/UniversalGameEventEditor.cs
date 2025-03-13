using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using _Scripts.Events_system;

/// <summary>
/// A universal CustomEditor for parameterless (BaseGameEvent)
/// and parameterized (BaseGameEvent<TParameter>) events.
/// </summary>
[CustomEditor(typeof(ScriptableObject), true)]
public class UniversalGameEventEditor : Editor
{
    // We store test parameter values in a dictionary keyed by the ScriptableObject.
    // This lets us preserve user inputs across Inspector refreshes, until a domain reload.
    private static Dictionary<UnityEngine.Object, object> s_TestValues = new Dictionary<UnityEngine.Object, object>();

    // We'll reflect to see if the inspected ScriptableObject inherits from BaseGameEvent
    // or BaseGameEvent<T>. If not, we won't do anything special.
    public override void OnInspectorGUI()
    {
        // Draws the fields from the actual event script (debug toggles, etc.).
        DrawDefaultInspector();
        EditorGUILayout.Space();

        // Check if the target is parameterless or parameterized by reflection.
        // Get the base class that might be BaseGameEvent or BaseGameEvent<TParameter>.
        var scriptType = target.GetType();
        var paramType = GetParameterGenericType(scriptType);

        if (!IsBaseGameEvent(scriptType))
        {
            // Not a BaseGameEvent, let default inspector handle it.
            return;
        }

        // Show number of currently registered listeners
        int count = GetListenersCount(target, paramType);
        EditorGUILayout.LabelField("Registered Listeners", count.ToString());

        // If parameterless, display a simple Raise button
        if (paramType == null)
        {
            if (GUILayout.Button("Raise Event"))
            {
                InvokeParameterlessRaise(target);
            }
        }
        else
        {
            // Parameter-based event: show a field to set the test value and then Raise it.
            object testValue = GetOrCreateTestValue(target, paramType);

            // Draw a suitable control for known parameter types
            object updatedValue = DrawParameterField(paramType, testValue);

            // Save back if changed
            if (updatedValue != null && updatedValue != testValue)
            {
                s_TestValues[target] = updatedValue;
            }

            // Raise button
            if (GUILayout.Button("Raise Event with Parameter"))
            {
                InvokeParameterizedRaise(target, paramType, updatedValue);
            }
        }
    }

    #region Reflection & Utility

    /// <summary>
    /// Determines if the provided type is derived from BaseGameEvent or BaseGameEvent<T>.
    /// Adjust the name "BaseGameEvent" if you have a different naming convention.
    /// </summary>
    private bool IsBaseGameEvent(Type t)
    {
        if (t == null || t == typeof(object)) return false;
        if (t.Name.StartsWith("BaseGameEvent")) return true;
        return IsBaseGameEvent(t.BaseType);
    }

    /// <summary>
    /// If this is BaseGameEvent<T>, returns typeof(T).
    /// Otherwise, returns null (parameterless or some unrelated type).
    /// </summary>
    private Type GetParameterGenericType(Type type)
    {
        if (type == null || type == typeof(object)) return null;

        if (type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("BaseGameEvent"))
        {
            // Return the first generic argument, e.g., float, bool, int, etc.
            var args = type.GetGenericArguments();
            if (args.Length > 0)
                return args[0];
        }

        return GetParameterGenericType(type.BaseType);
    }

    /// <summary>
    /// Gets or creates a default test-value for the specified paramType.
    /// This value is stored in a static dictionary, so it persists across
    /// multiple redraws of the Inspector, until a domain reload.
    /// </summary>
    private object GetOrCreateTestValue(UnityEngine.Object key, Type paramType)
    {
        if (!s_TestValues.TryGetValue(key, out object currentValue) || currentValue == null)
        {
            currentValue = GetDefaultValueFor(paramType);
            s_TestValues[key] = currentValue;
        }
        return currentValue;
    }

    /// <summary>
    /// Returns a default value for certain common types.
    /// Adjust as needed for your own use-cases.
    /// </summary>
    private object GetDefaultValueFor(Type paramType)
    {
        if (paramType == typeof(bool)) return false;
        if (paramType == typeof(int)) return 0;
        if (paramType == typeof(float)) return 0f;
        if (paramType == typeof(ulong)) return (ulong)0;
        if (paramType == typeof(Vector2)) return Vector2.zero;
        if (paramType == typeof(Vector3)) return Vector3.zero;
        // Fallback (null) for unsupported or complex types
        return null;
    }

    /// <summary>
    /// Draws the parameter field in the Inspector.
    /// We handle a few known types. 
    /// For advanced usage, you could attempt a more generic approach or create custom drawers.
    /// </summary>
    private object DrawParameterField(Type paramType, object currentValue)
    {
        EditorGUILayout.LabelField("Test Parameter Value");

        if (paramType == typeof(bool))
        {
            bool boolVal = (bool)currentValue;
            return EditorGUILayout.Toggle("Value", boolVal);
        }
        else if (paramType == typeof(int))
        {
            int intVal = (int)currentValue;
            return EditorGUILayout.IntField("Value", intVal);
        }
        else if (paramType == typeof(float))
        {
            float floatVal = (float)currentValue;
            return EditorGUILayout.FloatField("Value", floatVal);
        }
        else if (paramType == typeof(ulong))
        {
            ulong ulongVal = (ulong)currentValue;
            // Editor does not have a direct "UlongField", so parse from a text field:
            string strVal = EditorGUILayout.TextField("Value", ulongVal.ToString());
            if (ulong.TryParse(strVal, out ulong parsedVal))
                return parsedVal;
            return ulongVal;
        }
        else if (paramType == typeof(Vector2))
        {
            Vector2 vec2Val = (Vector2)currentValue;
            return EditorGUILayout.Vector2Field("Value", vec2Val);
        }
        else if (paramType == typeof(Vector3))
        {
            Vector3 vec3Val = (Vector3)currentValue;
            return EditorGUILayout.Vector3Field("Value", vec3Val);
        }

        // If we get here, it's an unhandled type in this sample
        EditorGUILayout.HelpBox(
            $"Unsupported parameter type: {paramType}\n" +
            "You can add a custom case for it in DrawParameterField().",
            MessageType.Warning);
        return currentValue;
    }

    /// <summary>
    /// Invokes a parameterless "Raise()" on the target event.
    /// </summary>
    private void InvokeParameterlessRaise(UnityEngine.Object gameEvent)
    {
        MethodInfo method = gameEvent.GetType().GetMethod("Raise", new Type[] { });
        if (method != null) method.Invoke(gameEvent, null);
    }

    /// <summary>
    /// Invokes a parameterized "Raise(TParameter)" on the target event using reflection.
    /// </summary>
    private void InvokeParameterizedRaise(UnityEngine.Object gameEvent, Type paramType, object parameterValue)
    {
        MethodInfo method = gameEvent.GetType().GetMethod("Raise", new[] { paramType });
        if (method != null) method.Invoke(gameEvent, new object[] { parameterValue });
    }

    /// <summary>
    /// Retrieves the count of the private "_listeners" field via reflection.
    /// Adjust for your naming if it's different.
    /// </summary>
    private int GetListenersCount(UnityEngine.Object gameEvent, Type paramType)
    {
        Type baseType;
        if (paramType != null)
        {
            // For parameterized events
            baseType = GetEventBaseGenericType(gameEvent.GetType(), paramType);
        }
        else
        {
            // For parameterless events
            baseType = typeof(BaseGameEvent);
        }

        if (baseType == null) return 0;

        FieldInfo fieldInfo = baseType.GetField("_listeners",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo == null) return 0;

        var value = fieldInfo.GetValue(gameEvent) as System.Collections.ICollection;
        if (value == null) return 0;

        return value.Count;
    }

    /// <summary>
    /// Helper to get the actual specialized type of "BaseGameEvent<TParameter>"
    /// for reflection. For example, if paramType is int, we want "BaseGameEvent<int>".
    /// </summary>
    private Type GetEventBaseGenericType(Type eventType, Type paramType)
    {
        while (eventType != null && eventType != typeof(object))
        {
            if (eventType.IsGenericType &&
                eventType.GetGenericTypeDefinition().Name.StartsWith("BaseGameEvent"))
            {
                var args = eventType.GetGenericArguments();
                if (args.Length == 1 && args[0] == paramType)
                {
                    return eventType;
                }
            }
            eventType = eventType.BaseType;
        }
        return null;
    }

    #endregion
}