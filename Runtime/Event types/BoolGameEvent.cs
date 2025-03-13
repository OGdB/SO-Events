using UnityEngine;

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Bool Game Event")]
    public sealed class BoolGameEvent : BaseGameEvent<bool>
    {
    }
}