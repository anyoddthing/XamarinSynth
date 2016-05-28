using System;

namespace SynthTest
{
    public interface IAudioSource : IDisposable
    {
        void PrepareToPlay(int samplesPerBlock, int sampleRate);

        void GetNextAudioBlock(ref AudioSourceChannelInfo bufferToFill);
    }
}

