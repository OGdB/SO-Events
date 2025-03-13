using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Bool Game Event")]
    public sealed class BoolGameEvent : BaseGameEvent<bool>
    {
#if UNITY_EDITOR
        public override object DrawParameterField(object currentValue)
        {
            bool boolValue = currentValue is bool ? (bool)currentValue : false;
            return EditorGUILayout.Toggle("Value", boolValue);
        }
#endif
    }
}