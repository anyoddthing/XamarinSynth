using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace SynthTest
{
    public unsafe static class Memory
    {
        const int ZeroBufferSize = 2048;
        static byte[] _zeroBuffer = new byte[ZeroBufferSize];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            if (typeof(T) == typeof(float))
            {
                return sizeof(float);
            }
            else if (typeof(T) == typeof(int))
            {
                return sizeof(int);
            }
            else if (typeof(T) == typeof(char))
            {
                return sizeof(char);
            }
            else if (typeof(T) == typeof(byte))
            {
                return sizeof(byte);
            }
            else
            {
                throw new NotImplementedException(typeof(T).ToString());
            }
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemCopy(void* source, void* destination, int length)
        {
            memcpy(destination, source, length);
        }

        [DllImport("__Internal")]
        private static extern void* memcpy(void* destination, void* source, int length);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ZeroMem<T>(void* ptr, int length) where T : struct
        {
            memset(ptr, 0, length * SizeOf<T>());
        }

        [DllImport("__Internal")]
        private static extern IntPtr memset(void* destination, int c, int length);


        // Alternative implementations

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void _ZeroMem(IntPtr ptr, int length)
        {
            while (length > 0)
            {
                if (length >= ZeroBufferSize)
                {
                    Marshal.Copy(_zeroBuffer, 0, ptr, ZeroBufferSize);
                    ptr += ZeroBufferSize;
                    length -= ZeroBufferSize;    
                }
                else
                {
                    Marshal.Copy(_zeroBuffer, 0, ptr, length);
                    break;
                }
            }
        }
    }
}

