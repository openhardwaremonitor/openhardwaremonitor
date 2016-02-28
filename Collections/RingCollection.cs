/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenHardwareMonitor.Collections
{
    public class RingCollection<T> : IEnumerable<T>
    {
        private T[] array;

        // first item of collection
        private int head;

        // number of items in the collection

        // index after the last item of the collection
        private int tail;

        public RingCollection() : this(0)
        {
        }

        public RingCollection(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");
            array = new T[capacity];
            head = 0;
            tail = 0;
            Count = 0;
        }

        public int Capacity
        {
            get { return array.Length; }
            set
            {
                var newArray = new T[value];
                if (Count > 0)
                {
                    if (head < tail)
                    {
                        Array.Copy(array, head, newArray, 0, Count);
                    }
                    else
                    {
                        Array.Copy(array, head, newArray, 0, array.Length - head);
                        Array.Copy(array, 0, newArray, array.Length - head, tail);
                    }
                }
                array = newArray;
                head = 0;
                tail = Count == value ? 0 : Count;
            }
        }

        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();
                var i = head + index;
                if (i >= array.Length)
                    i -= array.Length;
                return array[i];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();
                var i = head + index;
                if (i >= array.Length)
                    i -= array.Length;
                array[i] = value;
            }
        }

        public T First
        {
            get
            {
                if (Count == 0)
                    throw new InvalidOperationException();
                return array[head];
            }
            set
            {
                if (Count == 0)
                    throw new InvalidOperationException();
                array[head] = value;
            }
        }

        public T Last
        {
            get
            {
                if (Count == 0)
                    throw new InvalidOperationException();
                return array[tail == 0 ? array.Length - 1 : tail - 1];
            }
            set
            {
                if (Count == 0)
                    throw new InvalidOperationException();
                array[tail == 0 ? array.Length - 1 : tail - 1] = value;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void Clear()
        {
            // remove potential references 
            if (head < tail)
            {
                Array.Clear(array, head, Count);
            }
            else
            {
                Array.Clear(array, 0, tail);
                Array.Clear(array, head, array.Length - head);
            }

            head = 0;
            tail = 0;
            Count = 0;
        }

        public void Append(T item)
        {
            if (Count == array.Length)
            {
                var newCapacity = array.Length*3/2;
                if (newCapacity < array.Length + 8)
                    newCapacity = array.Length + 8;
                Capacity = newCapacity;
            }

            array[tail] = item;
            tail = tail + 1 == array.Length ? 0 : tail + 1;
            Count++;
        }

        public T Remove()
        {
            if (Count == 0)
                throw new InvalidOperationException();

            var result = array[head];
            array[head] = default(T);
            head = head + 1 == array.Length ? 0 : head + 1;
            Count--;

            return result;
        }

        private struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly RingCollection<T> collection;
            private int index;

            public Enumerator(RingCollection<T> collection)
            {
                this.collection = collection;
                index = -1;
            }

            public void Dispose()
            {
                index = -2;
            }

            public void Reset()
            {
                index = -1;
            }

            public T Current
            {
                get
                {
                    if (index < 0)
                        throw new InvalidOperationException();
                    return collection[index];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (index < 0)
                        throw new InvalidOperationException();
                    return collection[index];
                }
            }

            public bool MoveNext()
            {
                if (index == -2)
                    return false;

                index++;

                if (index == collection.Count)
                {
                    index = -2;
                    return false;
                }

                return true;
            }
        }
    }
}