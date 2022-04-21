using System.Collections.Generic;
using Core;
using UnityEngine;

namespace CYAN4S
{
    public class TimeManager
    {
        private List<BpmData> _bpms;
        private readonly DoubleChannelSO _timeChannel;
        private readonly DoubleChannelSO _beatChannel;
        
        public TimeManager(List<BpmData> bpms, DoubleChannelSO timeChannel, DoubleChannelSO beatChannel)
        {
            _bpms = bpms;
            _timeChannel = timeChannel;
            _beatChannel = beatChannel;
        }

        public void Update()
        {
            _timeChannel.value = _timeChannel.initialValue + Time.timeAsDouble;
            _beatChannel.value = _timeChannel.value / 60d * 120d; // TODO MATH
        }
    }
}