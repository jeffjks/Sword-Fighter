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
            public long timestamp;
            public Vector3 position;
        }

        private readonly LinkedList<TimedPosition> _positions = new LinkedList<TimedPosition>();
        private const int MaxHistoryMs = 5000;

        public void RecordPosition(long timestamp, Vector3 position)
        {
            _positions.AddLast(new TimedPosition { timestamp = timestamp, position = position });

            // 오래된 데이터 제거
            long threshold = timestamp - MaxHistoryMs;
            while (_positions.Count > 0 && _positions.First.Value.timestamp < threshold)
            {
                _positions.RemoveFirst();
            }
        }

        public Vector3? GetPositionAt(long targetTimestamp)
        {
            if (_positions.Count == 0)
                return null;

            LinkedListNode<TimedPosition> before = null;
            LinkedListNode<TimedPosition> after = null;

            var node = _positions.First;
            while (node != null)
            {
                if (node.Value.timestamp <= targetTimestamp)
                {
                    before = node;
                }
                else if (node.Value.timestamp > targetTimestamp)
                {
                    after = node;
                    break;
                }
                node = node.Next;
            }

            if (before == null)
                return _positions.First.Value.position;
            if (after == null)
                return _positions.Last.Value.position;

            long t1 = before.Value.timestamp;
            long t2 = after.Value.timestamp;

            float lerpT = (float)(targetTimestamp - t1) / (t2 - t1);

            return Vector3.Lerp(before.Value.position, after.Value.position, lerpT);
        }
    }
}
