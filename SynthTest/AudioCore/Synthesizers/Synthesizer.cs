using System;

namespace SynthTest
{
    public unsafe class Synthesizer : AudioIODeviceCallback, IDisposable
    {
        float*[] _channels = new float*[32];
        SampleBuffer _renderBuffer = new SampleBuffer();

        int _sampleRate;
        int _bufferSize;

        IAudioSource _source;
        public IAudioSource Source
        {
            get { return _source; }
            set 
            {
                if (_source != value)
                {
                    if (value != null)
                        value.PrepareToPlay(_bufferSize, _sampleRate);

                    if (_source != null)
                        _source.Dispose();

                    _source = value;
                }
            }
        }

        float _gain;
        public float Gain
        {
            get { return _gain; }
            set { _gain = value; }
        }

        public Synthesizer()
        {
        }

        public void PrepareToPlay(int sampleRate, int buffeSize)
        {
            _sampleRate = sampleRate;
            _bufferSize = buffeSize;

            if (_source != null)
                _source.PrepareToPlay(_bufferSize, _sampleRate);
        }

        #region AudioIODeviceCallback implementation

        public void AudioDeviceIOCallback(float*[] inputChannelData, int numInputChannels, float*[] outputChannelData, int numOutputChannels, int numSamples)
        {
            if (_source != null)
            {
                if (numInputChannels > numOutputChannels)
                    throw new NotImplementedException();
                
                var numActiveChans = 0;
                var channelSize = numSamples * sizeof(float);
                for (var i = 0; i < numInputChannels; ++i)
                {
                    _channels[numActiveChans] = outputChannelData[i];
                    Memory.MemCopy(_channels[numActiveChans], inputChannelData[i], channelSize);
                    ++numActiveChans;
                }
                
                for (var i = 0; i < numOutputChannels; ++i)
                {
                    _channels[numActiveChans] = outputChannelData[i];
                    Memory.ZeroMem<float>(_channels[numActiveChans], numSamples);
                    ++numActiveChans;
                }
                
                _renderBuffer.Wrap(_channels, numActiveChans, numSamples);
                
                var info = new AudioSourceChannelInfo(_renderBuffer, 0, numSamples);
                _source.GetNextAudioBlock(ref info);

                // TODO: gain ramp
            }
            else
            {
                for (var i = 0; i < numOutputChannels; ++i)
                {
                    Memory.ZeroMem<float>(outputChannelData[i], numSamples);
                }
            }
        }

        public void AudioDeviceAboutToStart(AudioIODevice audioDevice)
        {
            PrepareToPlay(audioDevice.CurrentSampleRate, audioDevice.CurrentBufferSize);
        }

        public void AudioDeviceStopped()
        {
            _sampleRate = 0;
            _bufferSize = 0;

            if (_source != null)
                _source.PrepareToPlay(_bufferSize, _sampleRate);
        
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (_source != null)
                _source.Dispose();
        }

        #endregion
    }
}

