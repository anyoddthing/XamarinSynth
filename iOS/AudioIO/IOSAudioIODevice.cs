using System;
using AudioToolbox;
using CoreFoundation;
using AudioUnit;
using System.Diagnostics;
using System.IO;

namespace SynthTest
{
    public unsafe class IOSAudioIODevice : AudioIODevice, IDisposable
    {
        const float ShortToFloat = 1.0f / 32768.0f;
        const float FloatToShort = 32768.0f;

        int _preferredBufferSize;
        int _actualBufferSize;
        int _numInputChannels;
        int _numOutputChannels;

        AudioUnit.AudioUnit _audioUnit;
        AudioStreamBasicDescription _format;

        int _sampleRate;
        bool _audioInputIsAvailable;
        bool _isRunning;

        AudioIODeviceCallback _callback;
        SampleBuffer _sampleBuffer;
        float*[] _inputChannels = new float*[3];
        float*[] _outputChannels = new float*[3];


        public IOSAudioIODevice()
        {
            _numInputChannels = 2;
            _numOutputChannels = 2;
            _preferredBufferSize = 0;

            AudioSession.Initialize();
            UpdateDeviceInfo();
        }

        #region IDisposable implementation

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region AudioIODevice implementation

        public int CurrentBufferSize { get { return _actualBufferSize; } }

        public int CurrentSampleRate { get { return _sampleRate; } }

        public bool IsOpen      { get { return _isRunning; } }

        int DefaultBufferSize   { get { return 1024; } }

        FileStream _out;

        public void Open(int inputChannelsWanted, int outputChannelsWanted, double targetSampleRate, int bufferSize = 0)
        {
            Console.WriteLine("Request ios audio device with: inputs: {0}, outputs: {1}, sampleRate: {2}, buffer: {3}",
                inputChannelsWanted, outputChannelsWanted, targetSampleRate, bufferSize);

            Close();

            var file = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/out.pcm";
            Console.WriteLine("Saving to {0}", file);
//            _out = new FileStream(file, FileMode.Create, FileAccess.Write);

            _preferredBufferSize = (bufferSize <= 0) ? DefaultBufferSize : bufferSize;

            _numInputChannels = inputChannelsWanted;
            _numOutputChannels = outputChannelsWanted;

            AudioSession.SetActive(false);

            if (_numInputChannels > 0 && _audioInputIsAvailable)
            {
                AudioSession.Category = AudioSessionCategory.PlayAndRecord;
                AudioSession.OverrideCategoryEnableBluetoothInput = true;
            }
            else
            {
                AudioSession.Category = AudioSessionCategory.PlayAndRecord;
            }

            FixRouteIfSetToReceiver();

            AudioSession.PreferredHardwareSampleRate = targetSampleRate;
            UpdateDeviceInfo();
            UpdateCurrentBufferSize();
            PrepareFloatBuffers(_actualBufferSize);

            CreateAudioUnit();
            _format = _audioUnit.GetAudioFormat(AudioUnitScopeType.Output, 1);
            _audioUnit.Stop();
            _audioUnit.Start();

            AudioSession.SetActive(true);
            _isRunning = true;

            Console.WriteLine("Opened ios audio device with: inputs: {0}, outputs: {1}, sampleRate: {2}, buffer: {3}",
                _numInputChannels, _numOutputChannels, _sampleRate, _actualBufferSize);
        }

        public void Close()
        {
            if (_isRunning)
            {
                _isRunning = false;

                AudioSession.Category = AudioSessionCategory.MediaPlayback;
                AudioSession.SetActive(false);

                if (_audioUnit != null)
                {
                    _audioUnit.Dispose();
                    _audioUnit = null;
                }

                if (_out != null)
                {
                    _out.Close();
                }
            }
        }

        public void Start(AudioIODeviceCallback callback)
        {
            if (_isRunning && _callback != callback)
            {
                callback.AudioDeviceAboutToStart(this);
                _callback = callback;
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                var lastCallback = _callback;
                _callback = null;

                if (lastCallback != null)
                {
                    lastCallback.AudioDeviceStopped();
                }
            }
        }

        void FixRouteIfSetToReceiver()
        {
            var audioRoute = AudioSession.AudioRoute;
            if (audioRoute.StartsWith("Receiver"))
            {
                AudioSession.RoutingOverride = AudioSessionRoutingOverride.Speaker;
            }
        }

        void UpdateDeviceInfo()
        {
            _sampleRate = (int)Math.Round(AudioSession.CurrentHardwareSampleRate);  
            _audioInputIsAvailable = AudioSession.AudioInputAvailable;
        }

        void UpdateCurrentBufferSize()
        {
            var bufferDuration = _sampleRate > 0 ? (float)(_preferredBufferSize / (double)_sampleRate) : 0.0f;
            AudioSession.PreferredHardwareIOBufferDuration = bufferDuration;
            _actualBufferSize = (int)(_sampleRate * AudioSession.CurrentHardwareIOBufferDuration + 0.5f);

            Console.WriteLine("UpdateBufferSize: preferred: {0} current: {1}",
                AudioSession.PreferredHardwareIOBufferDuration, AudioSession.CurrentHardwareIOBufferDuration);
        }

        void ResetFormat(int numChannels)
        {
            var format = new AudioStreamBasicDescription(AudioFormatType.LinearPCM);
            format.FormatFlags = AudioFormatFlags.IsSignedInteger | AudioFormatFlags.IsPacked;
            format.BitsPerChannel = 8 * sizeof(short);
            format.ChannelsPerFrame = numChannels;
            format.FramesPerPacket = 1;
            format.BytesPerFrame = format.BytesPerPacket = numChannels * sizeof(short);
            _format = format;
        }

        void CreateAudioUnit()
        {
            if (_audioUnit != null)
            {
                _audioUnit.Dispose();
                _audioUnit = null;
            }

            ResetFormat(2);

            var audioUnit = new AudioUnit.AudioUnit(AudioComponent.FindComponent(AudioTypeOutput.Remote));

            if (_numInputChannels > 0)
            {
                audioUnit.SetEnableIO(true, AudioUnitScopeType.Input, 1);
            }

            audioUnit.SetRenderCallback(AudioUnit_RenderCallback, AudioUnitScopeType.Input);

            AudioUnitStatus status;
            status = audioUnit.SetFormat(_format, AudioUnitScopeType.Input, 0);
            if (status != AudioUnitStatus.OK)
            {
                throw new Exception("Could not initialize audio unit: " + status);
            }
            status = audioUnit.SetFormat(_format, AudioUnitScopeType.Output, 1);
            if (status != AudioUnitStatus.OK)
            {
                throw new Exception("Could not initialize audio unit: " + status);
            }

            var osStatus = audioUnit.Initialize();
            if (osStatus != 0)
            {
                throw new Exception("Could not initialize audio unit: " + osStatus);
            }

            _audioUnit = audioUnit;  
        }


        #endregion

        void PrepareFloatBuffers(int bufferSize)
        {
            var deviceBufferSize = (int)(AudioSession.CurrentHardwareSampleRate * AudioSession.CurrentHardwareIOBufferDuration + 0.5f);
            Console.WriteLine("Prepare buffers: {0}, deviceBuffer: {1}, latency: {2}, {3}", bufferSize, deviceBufferSize, 
                AudioSession.CurrentHardwareOutputLatency, AudioSession.CurrentHardwareOutputLatency * AudioSession.CurrentHardwareSampleRate);
            if (_sampleBuffer != null)
            {
                _sampleBuffer.Dispose();
            }

            var numChannels = _numInputChannels + _numOutputChannels;
            if (numChannels > 0)
            {
                _sampleBuffer = new SampleBuffer(numChannels, bufferSize);

                for (var i = 0; i < _numInputChannels; ++i)
                {
                    _inputChannels[i] = _sampleBuffer.GetPointer(i);
                }

                for (var i = 0; i < _numOutputChannels; ++i)
                {
                    _outputChannels[i] = _sampleBuffer.GetPointer(i + _numInputChannels);
                }
            }
        }

        Stopwatch _clock = new Stopwatch();

        unsafe AudioUnitStatus AudioUnit_RenderCallback(AudioUnitRenderActionFlags actionFlags, 
                                                        AudioTimeStamp timeStamp, uint busNumber, uint numberFrames, AudioBuffers data)
        {
            _clock.Restart();
            AudioUnitStatus err = AudioUnitStatus.OK;
            if (_audioInputIsAvailable && _numInputChannels > 0)
            {
                err = _audioUnit.Render(ref actionFlags, timeStamp, 1, numberFrames, data);
            }

            var dataPtr = data[0].Data;
            if (_callback != null)
            {
                if (numberFrames > _sampleBuffer.NumSamples)
                {
                    PrepareFloatBuffers((int)numberFrames);
                }

                if (_audioInputIsAvailable && _numInputChannels > 0)
                {
                    var shortData = (short*)dataPtr.ToPointer();
                    if (_numInputChannels >= 2)
                    {
                        float* leftInput = _inputChannels[0];
                        float* rightInput = _inputChannels[1];
                        for (var i = 0; i < numberFrames; ++i)
                        {
                            *leftInput++ = *shortData++ * ShortToFloat;
                            *rightInput++ = *shortData++ * ShortToFloat;
                        }
                    }
                    else
                    {
                        float* leftInput = _inputChannels[0];
                        for (var i = 0; i < numberFrames; ++i)
                        {
                            *leftInput++ = *shortData++ * ShortToFloat;
                            ++shortData;
                        }
                    }
                }
                else
                {
                    for (var i = _numInputChannels; --i >= 0;)
                    {
                        _sampleBuffer.ClearChannel(i);
                    }
                }

                _callback.AudioDeviceIOCallback(
                    _inputChannels, _numInputChannels,
                    _outputChannels, _numOutputChannels,
                    (int)numberFrames
                );

                if (_out != null)
                {
                    byte* bytes = (byte*)_outputChannels[0];
                    for (var i = 0; i < numberFrames * sizeof(float); ++i)
                    {
                        _out.WriteByte(*bytes++);   
                    }
                }

                {
                    var shortData = (short*)dataPtr.ToPointer();
                    float* leftOutput = _outputChannels[0];
                    float* rightOutput = _outputChannels[1];
                    if (_numOutputChannels >= 2)
                    {
                        for (var i = 0; i < numberFrames; ++i)
                        {
                            *shortData++ = (short)(*leftOutput++ * FloatToShort);
                            *shortData++ = (short)(*rightOutput++ * FloatToShort);
                        }
                    }
                    else if (_numOutputChannels == 1)
                    {
                        float* output = _outputChannels[0];
                        for (var i = 0; i < numberFrames; ++i)
                        {
                            short sample = (short)(*output++ * FloatToShort);
                            *shortData++ = sample;
                            *shortData++ = sample;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < numberFrames; ++i)
                        {
                            *shortData++ = 0;
                            *shortData++ = 0;
                        }
                    }
                }
            }
            else
            {
                var shortData = (short*)dataPtr.ToPointer();
                for (var i = 0; i < numberFrames; ++i)
                {
                    *shortData++ = 0;
                    *shortData++ = 0;
                }
            }

            _clock.Stop();

            return err;
        }
    }
}

