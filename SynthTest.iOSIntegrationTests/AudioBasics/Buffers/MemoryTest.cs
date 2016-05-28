using System;
using NUnit.Framework;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Util;

namespace AudioBasics
{
    [TestFixture]
    public unsafe class MemoryTest
    {

        [Test]
        public void Can_Zero_Out_Memory()
        {
            foreach (var size in new int[] {387, 1024, 1378, 2048, 2051})
            {
                ZeroMemTest(size);
            }
        }

        private void ZeroMemTest(int size)
        {
            var intPtr = Marshal.AllocHGlobal(size + 1);
            var ptr = (byte*) intPtr.ToPointer();

            *(ptr + size) = 1;
            Memory.ZeroMem(ptr, size);

            for (var i = 0; i < size; ++i)
            {
                Assert.AreEqual(0, *ptr++, "at " + i);
            }

            ptr = (byte*) intPtr.ToPointer();
            Assert.AreEqual(1, *(ptr + size), "at " + size);

            Marshal.FreeHGlobal(intPtr);
        }

        [Test]
        public void Can_Use_Native_Memcpy()
        {
            var count = 64;
            var ptr1 = Marshal.AllocHGlobal(count);
            var ptr2 = Marshal.AllocHGlobal(count);

            var b1 = (byte*)ptr1.ToPointer();
            for (var i = 0; i < count; ++i)
            {
                *b1++ = (byte)i;
            }

            b1 = (byte*)ptr1.ToPointer();
            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(i, *b1++, "at " + i);
            }

            Memory.MemCopy(ptr1.ToPointer(), ptr2.ToPointer(), count);

            b1 = (byte*)ptr1.ToPointer();
            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(i, *b1++, "at " + i);
            }

            var b2 = (byte*)ptr2.ToPointer();
            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(i, *b2++, "at " + i);
            }

            Marshal.FreeHGlobal(ptr1);
            Marshal.FreeHGlobal(ptr2);
        }


        // performance
        [Test]
        [Ignore]
        public void Compare_C_Lib()
        {
            var ptr1 = Marshal.AllocHGlobal(1024);   

            var clib = Clock.BenchmarkTime(() => Memory.ZeroMem(ptr1.ToPointer(), 1024), 100000);
            var alt  = Clock.BenchmarkTime(() => Memory._ZeroMem(ptr1, 1024), 100000);

            Console.WriteLine("clib: {0} vs alt: {1}", clib, alt);

            Marshal.FreeHGlobal(ptr1);
        }

    }
}

