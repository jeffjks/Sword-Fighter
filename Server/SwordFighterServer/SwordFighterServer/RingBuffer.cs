using System;
using System.Collections.Generic;
using System.Text;

namespace SwordFighterServer
{
    public class RingBuffer<T>
    {
        private readonly T[] _buffer;
        private readonly int _capacity;
        private int _head = 0;
        private int _count = 0;

        public RingBuffer(int _capacity)
        {
            this._capacity = _capacity;
            _buffer = new T[_capacity];
        }

        public void Add(T item)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % _capacity;
            _count = Math.Min(_count + 1, _capacity);
        }

        public T Get(int indexFromOldest)
        {
            if (indexFromOldest >= _count)
                throw new IndexOutOfRangeException();

            int index = (_head - _count + indexFromOldest + _capacity) % _capacity;
            return _buffer[index];
        }

        public int Count => _count;
    }
}
