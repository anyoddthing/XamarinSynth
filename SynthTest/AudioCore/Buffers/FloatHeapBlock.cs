using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace SynthTest
{
    public unsafe struct FloatHeapBlock : IDisposable
    {
        public static readonly FloatHeapBlock Zero;

        float* _data;
        int _size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(FloatHeapBlock b1, FloatHeapBlock b2)
        {
            return b1._data == b2._data;   
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(FloatHeapBlock b1, FloatHeapBlock b2)
        {
            return b1._data != b2._data;   
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float* operator +(FloatHeapBlock block, int offset)
        {
            return block._data + offset;
        }

        public static unsafe implicit operator float*(FloatHeapBlock block)
        {
            return block._data;
        }

        public FloatHeapBlock(int size)
        {
            _size = size;
            _data = (float*)Marshal.AllocHGlobal(size * sizeof(float)).ToPointer();

            Clear();
        }

            
        public float this [int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }            

        public override int GetHashCode()
        {
            return (int)_data;
        }

        public override bool Equals(object obj)
        {
            return obj is FloatHeapBlock && Equals((FloatHeapBlock)obj);
        }
            
        public bool Equals(FloatHeapBlock other)
        {
            return this == other;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Memory.ZeroMem<float>(_data, _size);
        }

        #region IDisposable implementation
        public void Dispose()
        {
            if (_size != 0)
            {
                Marshal.FreeHGlobal((IntPtr)_data);
                _size = 0;
                _data = (float*)0;
            }
        }
        #endregion
    }

}

