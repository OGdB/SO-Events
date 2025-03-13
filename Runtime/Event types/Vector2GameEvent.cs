using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Vector2 Game Event")]
    public sealed class Vector2GameEvent : BaseGameEvent<Vector2>
    {
#if UNITY_EDITOR
        public override object DrawParameterField(object currentValue)
        {
            Vector2 vec2Value = currentValue is Vector2 ? (Vector2)currentValue : Vector2.zero;
            return EditorGUILayout.Vector2Field("Value", vec2Value);
        }
#endif
    }
}