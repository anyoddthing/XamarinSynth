using System;
using NUnit.Framework;
using System.Diagnostics;
using Util;

namespace AudioBasics
{
    [TestFixture]
    public class FloatHeapBufferTest
    {

        [Test]
        public void Unitialized_Buffer_Equals_To_Zero()
        {
            Assert.AreEqual(FloatHeapBlock.Zero, FloatHeapBlock.Zero);
            Assert.AreEqual(FloatHeapBlock.Zero, new FloatHeapBlock());
        }

        [Test]
        public void Disposed_Buffer_Equals_To_Zero()
        {
            var buffer = new FloatHeapBlock(1024);
            Assert.AreNotEqual(FloatHeapBlock.Zero, buffer);
        }

        // performance test
        [Test]
        [Ignore]
        public void Heap_Buffer_Index_Access_Is_Fast_Enough()
        {
            var array = new float[1024];
            var buffer = new FloatHeapBlock(1024);

            var arrayTime = Clock.BenchmarkTime(() =>
                {
                    for (var i = 0; i != 1024; ++i)
                    {
                        if (array[i] != 0)
                        {
                            Assert.Fail("What?!?");
                        }    
                    }
                });

            var bufferTime = Clock.BenchmarkTime(() =>
                {
                    for (var i = 0; i != 1024; ++i)
                    {
                        if (buffer[i] != 0)
                        {
                            Assert.Fail("What?!?");
                        }    
                    }
                });

            Console.WriteLine("array: {0} vs buffer: {1}", arrayTime, bufferTime);
        }
    }
}

