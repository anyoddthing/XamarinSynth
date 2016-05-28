using System;
using NUnit.Framework;
using System.Diagnostics;

namespace AudioBasics
{
    [TestFixture]
    public unsafe class AudioSampleBufferTest
    {
        [Test]
        public void Can_Allocate_New_Buffer()
        {
            using (var fixture = new AudioSampleBuffer(2, 1024))
            {
                Assert.NotNull(fixture);
            }
        }

        [Test]
        public void Initial_Buffer_Is_Zeroed_Out()
        {
            var numSamples = 1024;
            using (var fixture = new AudioSampleBuffer(2, numSamples))
            {
                var readPtr = fixture.GetReadPointer(0);
                while (numSamples-- > 0)
                {
                    Assert.AreEqual(0, *(readPtr++));
                }
                numSamples = 1024;
                readPtr = fixture.GetReadPointer(1);
                while (numSamples-- > 0)
                {
                    Assert.AreEqual(0, *(readPtr++));
                }
            }
        }

        [Test]
        public void Can_Clear_Single_Channel()
        {
            var numSamples = 1024;

            using (var fixture = new AudioSampleBuffer(3, numSamples))
            {
                var readPtr = fixture.GetReadPointer(1);
                var writePtr = fixture.GetWritePointer(0);
                for (var i = 0; i < 3 * 1024; ++i)
                {
                    *writePtr++ = 127.0f;
                }
                Assert.AreEqual(127.0f, *readPtr);
                fixture.ClearChannel(1);
                Assert.AreEqual(0f, *readPtr);
            }
        }

        [Test]
        public void Can_Write_And_Read_Back()
        {
            var fixture = new AudioSampleBuffer(2, 1024);
            var writePtr = fixture.GetWritePointer(0);
            (*writePtr) = 126.0f;

            Assert.AreEqual(126.0f, *fixture.GetReadPointer(0));
            fixture.Dispose();
        }

        [Test]
        public void Set_Sample_Stores_Value()
        {
            using (var fixture = new AudioSampleBuffer(2, 1024))
            {
                var readPtr = fixture.GetReadPointer(1);

                fixture.SetSample(1, 20, 127);

                Assert.AreEqual(127.0f, *(readPtr + 20));
            }
        }

        [Test]
        public void Add_Sample_Updates_Prior_Value()
        {
            using (var fixture = new AudioSampleBuffer(2, 1024))
            {
                var readPtr = fixture.GetReadPointer(1);

                fixture.SetSample(1, 20, 127.0f);
                fixture.AddSample(1, 20, 23.0f);

                Assert.AreEqual(127.0f + 23.0f, *(readPtr + 20));
            }
        }


        // Performance Tests

        static float[][] _testBuffer;

        static AudioSampleBufferTest()
        {
            _testBuffer = new float[2][];
            _testBuffer[0] = new float[1024];
            _testBuffer[1] = new float[1024];
        }

        [Test]
        [Ignore]
        public void Compare_Access_Patterns()
        {
            int runs = 1000;
            var stopWatch = new Stopwatch();

            var numSamples = 1024;

            var fixture = new AudioSampleBuffer(1, numSamples);

            stopWatch.Start();
            for (int i = 0; i != runs; ++i)
            {
                for (int j = 0; j < numSamples; j++)
                {
                    if (_testBuffer[0][j] != 0)
                    {
                        Assert.Fail();
                    }    
                }
            }

            stopWatch.Stop();
            var arrayTime = stopWatch.ElapsedTicks;

            stopWatch.Restart();
            for (int i = 0; i != runs; ++i)
            {
                var readPtr = fixture.GetReadPointer(0);
                for (int j = 0; j < numSamples; j++)
                {
                    if (*(readPtr++) != 0)
                    {
                        Assert.Fail();
                    }
                }
            }

            stopWatch.Stop();
            var pointerTime = stopWatch.ElapsedTicks;

            stopWatch.Restart();
            for (int i = 0; i != runs; ++i)
            {                
                for (int j = 0; j < numSamples; j++)
                {
                    if (fixture.GetSample(0, j) != 0)
                    {
                        Assert.Fail();
                    }
                }
            }

            stopWatch.Stop();
            var accessorTime = stopWatch.ElapsedTicks;
            fixture.Dispose();

            Console.WriteLine("Pointer: {0} vs Accessor: {1} vs Array: {2}", pointerTime, accessorTime, arrayTime);
        }
    }
}

