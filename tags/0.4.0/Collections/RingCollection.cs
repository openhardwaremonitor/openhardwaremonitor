/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2011
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenHardwareMonitor.Collections {
  public class RingCollection<T> : IEnumerable<T> {

    private T[] array;

    // first item of collection
    private int head;

    // index after the last item of the collection
    private int tail;

    // number of items in the collection
    private int size;

    public RingCollection() : this(0) { }

    public RingCollection(int capacity) {
      if (capacity < 0)
        throw new ArgumentOutOfRangeException("capacity");
      this.array = new T[capacity];
      this.head = 0; 
      this.tail = 0;
      this.size = 0;
    }

    public int Capacity {
      get {
        return array.Length;
      }
      set {
        T[] newArray = new T[value];
        if (size > 0) {
          if (head < tail) {
            Array.Copy(array, head, newArray, 0, size);
          } else {
            Array.Copy(array, head, newArray, 0, array.Length - head);
            Array.Copy(array, 0, newArray, array.Length - head, tail);
          }
        }
        this.array = newArray;
        this.head = 0;
        this.tail = size == value ? 0 : size;
      }
    }

    public void Clear() {
      
      // remove potential references 
      if (head < tail) {
        Array.Clear(array, head, size);
      } else {
        Array.Clear(array, 0, tail);
        Array.Clear(array, head, array.Length - head);
      }

      this.head = 0;
      this.tail = 0;
      this.size = 0;
    }

    public void Append(T item) {
      if (size == array.Length) {
        int newCapacity = array.Length * 3 / 2;
        if (newCapacity < array.Length + 8)
          newCapacity = array.Length + 8;
        Capacity = newCapacity;
      }

      array[tail] = item;
      tail = tail + 1 == array.Length ? 0 : tail + 1;
      size++;
    }

    public T Remove() {
      if (size == 0)
        throw new InvalidOperationException();

      T result = array[head];
      array[head] = default(T);
      head = head + 1 == array.Length ? 0 : head + 1;
      size--;

      return result;
    }

    public int Count {
      get {
        return size;
      }
    }

    public T this[int index] {
      get {
        if (index < 0 || index >= size)
          throw new IndexOutOfRangeException();
        int i = head + index;
        if (i >= array.Length)
          i -= array.Length;
        return array[i];
      }
      set {
        if (index < 0 || index >= size)
          throw new IndexOutOfRangeException();
        int i = head + index;
        if (i >= array.Length)
          i -= array.Length;
        array[i] = value;
      }
    }

    public T First {
      get {
        if (size == 0)
          throw new InvalidOperationException();
        return array[head];
      }
      set {
        if (size == 0)
          throw new InvalidOperationException();
        array[head] = value;
      }
    }

    public T Last {
      get {
        if (size == 0)
          throw new InvalidOperationException();
        return array[tail == 0 ? array.Length - 1 : tail - 1];
      }
      set {
        if (size == 0)
          throw new InvalidOperationException();
        array[tail == 0 ? array.Length - 1 : tail - 1] = value;
      }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
      return new RingCollection<T>.Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return new RingCollection<T>.Enumerator(this);
    }

    private struct Enumerator : IEnumerator<T>, IEnumerator {

      private RingCollection<T> collection;
      private int index;

      public Enumerator(RingCollection<T> collection) {
        this.collection = collection;
        this.index = -1;
      }

      public void Dispose() {
        this.index = -2;
      }

      public void Reset() {
        this.index = -1;
      }

      public T Current {
        get {
          if (index < 0)
            throw new InvalidOperationException();
          return collection[index];
        }
      }

      object IEnumerator.Current {
        get {
          if (index < 0)
            throw new InvalidOperationException();
          return collection[index];
        }
      }

      public bool MoveNext() {
        if (index == -2)
          return false;

        index++;

        if (index == collection.size) {
          index = -2;
          return false;
        }

        return true;
      }
    }
  }
}
