using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Vector3 Game Event")]
    public sealed class Vector3GameEvent : BaseGameEvent<Vector3>
    {
#if UNITY_EDITOR
        public override object DrawParameterField(object currentValue)
        {
            Vector3 vecValue = currentValue is Vector3 ? (Vector3)currentValue : Vector3.zero;
            return EditorGUILayout.Vector3Field("Value", vecValue);
        }
#endif
    }
}