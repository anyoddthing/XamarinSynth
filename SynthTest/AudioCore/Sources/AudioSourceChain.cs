using System;
using System.Collections.Generic;

namespace SynthTest
{
    public class AudioSourceChain : AAudioSource
    {
        List<IAudioSource> _chain = new List<IAudioSource>();

        public void AddSource(IAudioSource source)
        {
            _chain.Add(source);
        }

        public override void PrepareToPlay(int samplesPerBlock, int sampleRate)
        {
            foreach (var item in _chain)
            {
                item.PrepareToPlay(samplesPerBlock, sampleRate);
            }
        }

        public override void GetNextAudioBlock(ref AudioSourceChannelInfo bufferToFill)
        {
            foreach (var item in _chain)
            {
                item.GetNextAudioBlock(ref bufferToFill);
            }
        }

        public override void Dispose()
        {
            foreach (var item in _chain)
            {
                item.Dispose();
            }
        }
    }
}

