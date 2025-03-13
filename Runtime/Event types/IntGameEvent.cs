using UnityEngine;

namespace SO_Events.Runtime.Event_types
{
    [CreateAssetMenu(menuName = "Events/Int Game Event")]
    public sealed class IntGameEvent : BaseGameEvent<int>
    {
    }
}