using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SwordFighterServer
{
    public class PositionHistory
    {
        private struct TimedPosition
        {
            public int tick;
            public Vector3 position;
        }

        private const int MaxHistoryTick = 120;

        private readonly RingBuffer<TimedPosition> _positionHistory = new RingBuffer<TimedPosition>(MaxHistoryTick);

        public void RecordPosition(Vector3 position)
        {   
            _positionHistory.Add(new TimedPosition { tick = GameLogic.CurrentTick, position = position });
        }

        public bool TryGetPositionAt(long targetTimestamp, out Vector3 pos)
        {
            pos = default;

            if (_positionHistory.Count == 0)
                return false;

            var targetTick = GameLogic.GetTickFromTimestamp(targetTimestamp);

            var afterIndex = -1;

            for (var i = 0; i < _positionHistory.Count; i++)
            {
                var cur = _positionHistory.Get(i);
                if (cur.tick == targetTick)
                {
                    pos = cur.position;
                    return true;
                }
                else if (cur.tick > targetTick)
                {
                    afterIndex = i;
                    break;
                }
            }

            var beforeIndex = afterIndex - 1;

            if (beforeIndex == -1)
            {
                pos = _positionHistory.Get(0).position;
                return true;
            }
            if (afterIndex == -1)
            {
                var lastIndex = _positionHistory.Count - 1;
                pos = _positionHistory.Get(lastIndex).position;
                return true;
            }

            var before = _positionHistory.Get(beforeIndex);
            var after = _positionHistory.Get(afterIndex);

            int t1 = before.tick;
            int t2 = after.tick;

            float lerpT = (float)(targetTick - t1) / (t2 - t1);

            pos = Vector3.Lerp(before.position, after.position, lerpT); // 보간
            return true;
        }
    }
}
