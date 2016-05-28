using System;

namespace SynthTest
{
    public class AudioException : Exception
    {
        public AudioException(string message) : base(message)
        {
        }
    }

    public interface AudioIODeviceCallback
    {
        unsafe void AudioDeviceIOCallback(
            float*[] inputChannelData,
            int numInputChannels,
            float*[] outputChannelData,
            int numOutputChannels,
            int numSamples);
        
        void AudioDeviceAboutToStart(AudioIODevice audioDevice);

        void AudioDeviceStopped();
    }

    public interface AudioIODevice : IDisposable
    {
        int CurrentBufferSize { get; }
        int CurrentSampleRate { get; }

        bool IsOpen { get; }

        void Open(int inputChannels, int outputChannels, double sampleRate, int bufferSizeSamples);
        void Close();

        void Start(AudioIODeviceCallback callback);
        void Stop();
    }
}

