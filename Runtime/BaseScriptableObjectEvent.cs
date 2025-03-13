using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SO_Events.Runtime
{
    // An interface that exposes the drawing method.
    public interface IEventDrawer
    {
#if UNITY_EDITOR
        /// <summary>
        /// Draws a parameter field in the custom editor.
        /// </summary>
        /// <param name="currentValue">The current test value.</param>
        /// <returns>The new value after drawing.</returns>
        object DrawParameterField(object currentValue);
#endif
    }

    // Single type event with parameter
    [Serializable]
    public abstract class BaseGameEvent<TParameter> : ScriptableObject, IEventDrawer
    {
        protected List<Action<TParameter>> Listeners = new();
        public TParameter LastValue { get; protected set; }

        protected virtual void OnEnable()
        {
            SceneManager.sceneLoaded -= OnSceneChange;
            SceneManager.sceneLoaded += OnSceneChange;
            LastValue = default;
        
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            #endif
        }

        protected virtual void OnDisable()
        {
            Clean();
        
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            #endif
        }

        private void OnDestroy()
        {
            Clean();
        }

        public virtual void Raise(TParameter t)
        {
            for (var i = Listeners.Count - 1; i >= 0; i--)
                Listeners[i]?.Invoke(t);
            LastValue = t;
            #if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            #endif
        }

        public TParameter RegisterListener(Action<TParameter> listener)
        {
            if (!Listeners.Contains(listener))
                Listeners.Add(listener);
            return LastValue;
        }

        public void UnRegisterListener(Action<TParameter> listener)
        {
            if (Listeners.Contains(listener))
                Listeners.Remove(listener);
        }

        protected void OnSceneChange(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode arg1)
        {
            LastValue = default;
        }

        protected void Clean()
        {
            LastValue = default;
            SceneManager.sceneLoaded -= OnSceneChange;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Virtual method for drawing the parameter field.
        /// Derived classes can override this to provide custom editor controls.
        /// </summary>
        public virtual object DrawParameterField(object currentValue)
        {
            EditorGUILayout.HelpBox(
                "No custom parameter field provided.\nOverride DrawParameterField in your event class to add one.",
                MessageType.Info);
            return currentValue;
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                LastValue = default;
            }
        }
#endif
    }

    // Event without parameter remains unchanged.
    [Serializable]
    public abstract class BaseGameEvent : ScriptableObject
    {
        private List<Action> _listeners = new();

        public void Raise()
        {
            for (var i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i]?.Invoke();
        }

        public void RegisterListener(Action listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnRegisterListener(Action listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }
    }
}