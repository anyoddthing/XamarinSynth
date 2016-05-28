using System;
using NUnit.Framework;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Util
{
    [TestFixture]
    public class SIMDTest
    {

        [Test]
        public unsafe void Can_Add_Scalar_To_Vector()
        {
            var mem = Marshal.AllocHGlobal(16 * sizeof(float));
            var ptr = (float*)mem.ToPointer();

            for (var i = 0; i < 8; ++i)
            {
                *ptr++ = i;
            }

            SIMD.Add((ptr - 8), 10, ptr, 8);
            for (var i = 0; i < 8; ++i)
            {
                Assert.AreEqual((i + 10.0f).ToString(), (*ptr++).ToString());
            }
        }
    }
}

