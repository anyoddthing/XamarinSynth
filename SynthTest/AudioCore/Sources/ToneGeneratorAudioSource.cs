using System;
using System.Diagnostics;

namespace SynthTest
{
    public unsafe class ToneGeneratorAudioSource : AAudioSource
    {
        int _sampleRate;
        float _currentPhase;

        float _frequency;
        public float Frequency
        {
            get { return _frequency; }
            set
            {
                _frequency = value;
            }
        }

        float _amplitude;
        public float Amplitude 
        { 
            get { return _amplitude; }
            set { _amplitude = value; }
        }

        public ToneGeneratorAudioSource()
        {
            Frequency = 440.0f;
            Amplitude = 0.5f;
        }

        #region AudioSource implementation

        public override void PrepareToPlay(int samplesPerBlock, int sampleRate)
        {
            _currentPhase = 0.0f;
            _sampleRate = sampleRate;         
        }

        public override void GetNextAudioBlock(ref AudioSourceChannelInfo bufferToFill)
        {
            var amplitude = _amplitude;
            var frequency = _frequency;
            var phasePerSample = (2.0f * frequency) / _sampleRate;

            var currentPhase = _currentPhase;

            var channels   = bufferToFill.Buffer.Channels;
            var numSamples = bufferToFill.NumSamples;

            for (var i = 0; i < numSamples; ++i)
            {
                var sample = amplitude * (float)Math.Sin(currentPhase * (float)Math.PI);
                for (var j = bufferToFill.Buffer.NumChannels; --j >= 0;)
                {
                    channels[j][bufferToFill.StartSample + i] = sample;
                }

                currentPhase += phasePerSample;
                if (currentPhase > 1)
                {
                    currentPhase -= 2;
                }
            }

            _currentPhase = currentPhase;
        }

        #endregion
    }
}

