using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Events_system
{
    // Single type event with parameter
    [Serializable]
    public abstract class BaseGameEvent<TParameter> : ScriptableObject
    {
        protected List<Action<TParameter>> _listeners = new();
        public TParameter LastValue { get; protected set; }

        protected void OnEnable()
        {
            SceneManager.sceneLoaded -= OnSceneChange;
            SceneManager.sceneLoaded += OnSceneChange;

            LastValue = default;
        }

        protected void OnDisable()
        {
            Clean();
        }

        private void OnDestroy()
        {
            Clean();
        }

        public virtual void Raise(TParameter t)
        {
            for (var i = _listeners.Count - 1; i >= 0; i--) _listeners[i]?.Invoke(t);

            LastValue = t;
        }

        public TParameter RegisterListener(Action<TParameter> listener)
        {
            if (!_listeners.Contains(listener)) _listeners.Add(listener);

            return LastValue;
        }

        public void UnRegisterListener(Action<TParameter> listener)
        {
            if (_listeners.Contains(listener)) _listeners.Remove(listener);
        }

        protected void OnSceneChange(Scene arg0, LoadSceneMode arg1)
        {
            LastValue = default;
        }

        protected void Clean()
        {
            LastValue = default;
            SceneManager.sceneLoaded -= OnSceneChange;
        }
    }

    // Event without parameter
    [Serializable]
    public abstract class BaseGameEvent : ScriptableObject
    {
        private List<Action> _listeners = new();

        public void Raise()
        {
            for (var i = _listeners.Count - 1; i >= 0; i--) _listeners[i]?.Invoke();
        }

        public void RegisterListener(Action listener)
        {
            if (!_listeners.Contains(listener)) _listeners.Add(listener);
        }

        public void UnRegisterListener(Action listener)
        {
            if (_listeners.Contains(listener)) _listeners.Remove(listener);
        }
    }
}