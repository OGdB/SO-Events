using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Ulong Game Event")]
    public class UlongGameEvent : BaseGameEvent<ulong>
    {
#if UNITY_EDITOR
        public override object DrawParameterField(object currentValue)
        {
            ulong ulongValue = currentValue is ulong ? (ulong)currentValue : 0ul;
            string strVal = EditorGUILayout.TextField("Value", ulongValue.ToString());
            if (ulong.TryParse(strVal, out ulong parsedVal))
                return parsedVal;
            return ulongValue;
        }
#endif
    }
}