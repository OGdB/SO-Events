using _Scripts.Events_system;
using UnityEngine;

namespace Event_Bus_System.Event_types
{
    [CreateAssetMenu(menuName = "Events/Ulong Game Event")]
    public class UlongGameEvent : BaseGameEvent<ulong>
    {
    }
}