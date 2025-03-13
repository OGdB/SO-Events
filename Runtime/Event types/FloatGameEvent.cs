using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Float Game Event")]
    public sealed class FloatGameEvent : BaseGameEvent<float>
    {
#if UNITY_EDITOR
        public override object DrawParameterField(object currentValue)
        {
            float floatValue = currentValue is float ? (float)currentValue : 0f;
            return EditorGUILayout.FloatField("Value", floatValue);
        }
#endif
    }
}