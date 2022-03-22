using UnityEngine;
using UnityEngine.Events;

namespace CYAN4S
{
    public abstract class ReadonlyChannelSO<TValue, TChannel> : ScriptableObject where TChannel : ChannelSO<TValue>
    {
        public TChannel channel;
        
        public TValue Value => channel.value;

        public UnityAction Subscribe(UnityAction<TValue> e)
        {
            channel.OnEventRaised += e;
            return () => channel.OnEventRaised -= e;
        }
    }
}