using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Int Game Event")]
    public sealed class IntGameEvent : BaseGameEvent<int>
    {
#if UNITY_EDITOR
        public override object DrawParameterField(object currentValue)
        {
            int intValue = currentValue is int ? (int)currentValue : 0;
            return EditorGUILayout.IntField("Value", intValue);
        }
#endif
    }
}