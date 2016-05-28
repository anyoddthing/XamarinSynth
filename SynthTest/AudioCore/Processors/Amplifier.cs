using System;

namespace SynthTest
{
    public unsafe class Amplifier
    {
        public void GetNextAudioBlock(ref AudioSourceChannelInfo bufferToFill)
        {
            var buffer = bufferToFill.Buffer;
            for (var i = 1; i < buffer.NumChannels; ++i)
            {
//                for (var j = 0; j < bufferToFill.NumSamples; ++j)
//                {
//                    buffer.Channels[i][bufferToFill.StartSample + j] *= buffer.Channels[0][bufferToFill.StartSample + j];
//                }

                SIMD.Mul(
                    buffer.GetPointer(0, bufferToFill.StartSample),
                    buffer.GetPointer(i, bufferToFill.StartSample),
                    buffer.GetPointer(i, bufferToFill.StartSample),
                    bufferToFill.NumSamples
                );
            }
        }
    }
}

