using SO_Events.Runtime;
using UnityEditorInternal;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Input_Raycast_System.Runtime
{
    public class RaycastHitGameEvent : BaseGameEvent<RaycastHit>
    {
#if UNITY_EDITOR
        public override object DrawParameterField(object currentValue)
        {
            RaycastHit raycastHitValue = currentValue as RaycastHit? ?? default;

            // Draw an editable field for the hit position.
            Vector3 newPoint = EditorGUILayout.Vector3Field("Hit Position", raycastHitValue.point);

            // Retrieve the hit object's current name if available.
            string hitObjectName = "";
            if (raycastHitValue.collider && raycastHitValue.collider.gameObject)
            {
                hitObjectName = raycastHitValue.collider.gameObject.name;
            }
            // Draw a text field for hit object name.
            hitObjectName = EditorGUILayout.TextField("Hit Object Name", hitObjectName);

            // Update the RaycastHit value.
            // Note: Since RaycastHit is a struct, we create a copy and update the point.
            // However, the collider field cannot be changed here. This method is meant
            // for testing/demonstration of multi-field display.
            RaycastHit updatedHit = raycastHitValue;
            updatedHit.point = newPoint;

            // Optionally, you could use the hitObjectName string for additional logic or debugging.
            return updatedHit;
        }
#endif
    }
}