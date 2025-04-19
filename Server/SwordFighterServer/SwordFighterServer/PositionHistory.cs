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

        private readonly LinkedList<TimedPosition> _positionHistory = new LinkedList<TimedPosition>();
        private const int MaxHistoryMs = 5000;

        public void RecordPosition(long timestamp, Vector3 position)
        {
            _positionHistory.AddLast(new TimedPosition { timestamp = timestamp, position = position });

            // 오래된 데이터 제거
            long threshold = timestamp - MaxHistoryMs;
            while (_positionHistory.Count > 0 && _positionHistory.First.Value.timestamp < threshold)
            {
                _positionHistory.RemoveFirst();
            }
        }

        public bool TryGetPositionAt(long targetTimestamp, out Vector3 pos)
        {
            pos = default;

            if (_positionHistory.Count == 0)
                return false;

            LinkedListNode<TimedPosition> before = null;
            LinkedListNode<TimedPosition> after = null;

            var node = _positionHistory.First;
            while (node != null) // before <= target < after 가 되는 timestamp 찾기
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
            {
                pos = _positionHistory.First.Value.position;
                return true;
            }
            if (after == null)
            {
                pos = _positionHistory.Last.Value.position;
                return true;
            }

            long t1 = before.Value.timestamp;
            long t2 = after.Value.timestamp;

            if (t1 == t2)
            {
                pos = after.Value.position;
                return true;
            }

            float lerpT = (float)(targetTimestamp - t1) / (t2 - t1);

            pos = Vector3.Lerp(before.Value.position, after.Value.position, lerpT); // 보간
            return true;
        }
    }
}
