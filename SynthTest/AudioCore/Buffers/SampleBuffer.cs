using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace SynthTest
{
    public unsafe class SampleBuffer : IDisposable
    {
        FloatHeapBlock _allocatedBlock;

        int _numSamples;
        public int NumSamples { get { return _numSamples; } }

        int _numChannels;
        public int NumChannels { get { return _numChannels; } }

        float*[] _channels;
        public float*[] Channels { get { return _channels; } }

        public SampleBuffer()
        {
            // uninitialized buffer
        }

        public SampleBuffer(int numChannels, int numSamples)
        {
            SetSize(numChannels, numSamples);
        }

        public SampleBuffer(float*[] channels, int numChannels, int numSamples)
        {
            Wrap(channels, numChannels, numSamples);
        }

        #region IDisposable implementation

        public void Dispose()
        {            
            SetSize(0, 0);
        }

        #endregion

        public void Wrap(float*[] channels, int numChannels, int numSamples)
        {
            _numChannels = numChannels;
            _numSamples = numSamples;
            _channels = channels;
        }

        public void SetSize(int numChannels, int numSamples)
        {
            if (numChannels > _numChannels || numSamples > _numSamples || _allocatedBlock == FloatHeapBlock.Zero)
            {
                _allocatedBlock.Dispose();
                _numSamples = numSamples;
                _numChannels = numChannels;

                var size = numChannels * numSamples;
                if (size > 0)
                {
                    _allocatedBlock = new FloatHeapBlock(size);

                    _channels = new float*[numChannels];
                    for (var i = 0; i < numChannels; ++i)
                    {
                        _channels[i] = _allocatedBlock + i * _numSamples;
                    }
                }
                else
                {
                    _channels = null;
                }
            }
        }

        public void Clear()
        {
            foreach (var channel in _channels)
            {
                Memory.ZeroMem<float>(channel, _numSamples);    
            }
        }

        public void Clear(int startSample, int numSamples)
        {
            for (var chan = 0; chan < _numChannels; ++chan)
            {
                var start = _channels[chan];
                Memory.ZeroMem<float>(start, numSamples);
            }    
        }

        public void ClearChannel(int channel)
        {
            Memory.ZeroMem<float>(_channels[channel], _numSamples);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float* GetPointer(int channel = 0, int sampleIndex = 0)
        {
            return _channels[channel] + sampleIndex;
        }

        public float this [int channel, int index]
        {
            get { return _channels[channel][index]; }
            set { _channels[channel][index] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSample(int channel, int index, float newValue)
        {
            _channels[channel][index] = newValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSample(int channel, int index, float newValue)
        {
            _channels[channel][index] += newValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSample(int channel, int index)
        {
            return _channels[channel][index];
        }
    }
}

