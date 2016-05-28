using System;
using System.Collections.Generic;

namespace SynthTest
{
    public struct AudioDeviceSetup
    {
        public double SampleRate;
        public int BufferSize;
        public int NumInputChannels;
        public int NumOutputChannels;
    }

    public interface AudioIODeviceFactory
    {
        AudioIODevice CreateDevice();
    }

    public unsafe class AudioDeviceManager : DisposableObject, AudioIODeviceCallback
    {
        private readonly object _lock = new object();

        public AudioIODevice Device { get; private set; }
        SampleBuffer _tempBuffer;

        List<AudioIODeviceCallback> _callbacks = new List<AudioIODeviceCallback>();

        public AudioDeviceManager(AudioIODeviceFactory deviceFactory, AudioDeviceSetup setup)
        {
            var audioIODevice = deviceFactory.CreateDevice();
            audioIODevice.Open(
                setup.NumInputChannels,
                setup.NumOutputChannels,
                setup.SampleRate,
                setup.BufferSize
            );

            Device = audioIODevice;
            Device.Start(this);

            _tempBuffer = new SampleBuffer(1, 1);
        }

        protected override void DisposeManagedResources()
        {
            if (Device != null)
            {
                Device.Dispose();
                Device = null;
            }

            if (_tempBuffer != null)
            {
                _tempBuffer.Dispose();
                _tempBuffer = null;
            }
        }

        #region AudioIODeviceCallback implementation

        public void AudioDeviceIOCallback(
            float*[] inputChannelData, int numInputChannels, 
            float*[] outputChannelData, int numOutputChannels, int numSamples)
        {
            if (_callbacks.Count > 0)
            {
                _tempBuffer.SetSize(Math.Max(1, numOutputChannels), Math.Max(1, numSamples));

                _callbacks[0].AudioDeviceIOCallback(
                    inputChannelData, numInputChannels,
                    outputChannelData, numOutputChannels, numSamples);

                var tempChans = _tempBuffer.Channels;
                for (var i = _callbacks.Count; --i > 0;)
                {
                    _callbacks[i].AudioDeviceIOCallback(
                        inputChannelData, numInputChannels,
                        tempChans, numOutputChannels, numSamples);
                
                    for (var chan = 0; chan < numOutputChannels; ++chan)
                    {
                        var src = tempChans[chan];
                        var dst = outputChannelData[chan];
                        // TODO: use simd?
                        for (int j = 0; j < numSamples; ++j)
                        {
                            dst[j] += src[j];
                        }
                    }
                }
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
            lock (_lock)
            {
                foreach (var callback in _callbacks)
                {
                    callback.AudioDeviceAboutToStart(audioDevice);
                }
            }
        }

        public void AudioDeviceStopped()
        {
            lock (_lock)
            {
                foreach (var callback in _callbacks)
                {
                    callback.AudioDeviceStopped();
                }
            }
        }

        #endregion

        public void AddAudioCallback(AudioIODeviceCallback callback)
        {
            callback.AudioDeviceAboutToStart(Device);

            lock (_lock)
            {
                _callbacks.Add(callback);
            }
        }

        public void RemoveAudioCallback(AudioIODeviceCallback callback)
        {
            lock (_lock)
            {
                _callbacks.Remove(callback);
            }
            callback.AudioDeviceStopped();
        }

    }
}

