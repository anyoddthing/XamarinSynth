using System;

namespace SynthTest
{
    public struct AudioSourceChannelInfo
    {
        public SampleBuffer Buffer;
        public int StartSample;
        public int NumSamples;

        public AudioSourceChannelInfo(SampleBuffer buffer, int startSample, int numSamples)
        {
            Buffer = buffer;
            StartSample = startSample;
            NumSamples = numSamples;
        }

        public void ClearActiveBufferRegion()
        {
            Buffer.Clear(StartSample, NumSamples);
        }
    }
}

