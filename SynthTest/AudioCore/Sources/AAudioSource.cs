using System;

namespace SynthTest
{
    public abstract class AAudioSource : IAudioSource
    {
        public float[] Params { get; }

        public AAudioSource(int parameterCount = 0)
        {
            Params = new float[parameterCount];
        }

        public virtual void PrepareToPlay(int samplesPerBlock, int sampleRate) { }

        public abstract void GetNextAudioBlock(ref AudioSourceChannelInfo bufferToFill);

        public virtual void Dispose() { }
    }
}

