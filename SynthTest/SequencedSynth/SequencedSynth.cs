using System;

namespace SynthTest
{
    public unsafe class SequencedSynth : IAudioSource
    {
        private static readonly Logger LOG = Logger.Debug<SequencedSynth>();

        class BufferPool : IDisposable
        {
            public SampleBuffer Env1Buffer { get; private set; }

            public SampleBuffer GainAmpBuffer { get; private set; }

            public BufferPool(int samplesPerBlock, double sampleRate)
            {
                Console.WriteLine("Initializing buffer pool: samples:{0} sampleRate:{1}", samplesPerBlock,sampleRate);
                Env1Buffer = new SampleBuffer(1, samplesPerBlock);

                GainAmpBuffer = new SampleBuffer(new float*[3], 3, samplesPerBlock);
                GainAmpBuffer.Channels[0] = Env1Buffer.GetPointer();
            }

            public AudioSourceChannelInfo SetupGainBuffer(ref AudioSourceChannelInfo bufferToFill)
            {
                for (var channel = 0; channel < bufferToFill.Buffer.NumChannels; ++channel)
                {
                    GainAmpBuffer.Channels[channel + 1] = bufferToFill.Buffer.Channels[channel];
                }

                return new AudioSourceChannelInfo(GainAmpBuffer, bufferToFill.StartSample, bufferToFill.NumSamples);
            }

            public void Dispose()
            {
                Env1Buffer.Dispose();
                GainAmpBuffer.Dispose();
            }
        }

        private BeatSequencer _beatSeqencer;
        private BufferPool _bufferPool;

        #region Components

        private ADSREnvelope _env1;
        private Amplifier _gainAmp;
        private ToneGeneratorAudioSource _generator;
        private int _sampleTime;

        #endregion

        public void PrepareToPlay(int samplesPerBlock, int sampleRate)
        {
            LOG.Debug("PrepareToPlay");
            _sampleTime = 0;
            _beatSeqencer = new BeatSequencer(sampleRate, 120);
            _bufferPool = new BufferPool(samplesPerBlock, sampleRate);

            _env1 = new ADSREnvelope(_beatSeqencer.Sequencer);
            _env1.PrepareToPlay(sampleRate);

            _gainAmp = new Amplifier();

            _generator = new ToneGeneratorAudioSource();
            _generator.PrepareToPlay(samplesPerBlock, sampleRate);

            CreateSomeNotes();
        }

        private void CreateSomeNotes()
        {
            _beatSeqencer.SetLength(1);
            for (var i = 0; i < 1; ++i)
            {
                _beatSeqencer.AddNote(0, i / 4.0f, 0.1f, 440, 0.8f);
            }
        }

        public void GetNextAudioBlock(ref AudioSourceChannelInfo bufferToFill)
        {
            // commit commands from the ui
            _beatSeqencer.Sequencer.StartFrame(_sampleTime);

            _env1.GetNextAudioBlock(_bufferPool.Env1Buffer, bufferToFill.StartSample, bufferToFill.NumSamples);
//            TestBuffer(_bufferPool.Env1Buffer, bufferToFill.StartSample, bufferToFill.NumSamples);

            _generator.GetNextAudioBlock(ref bufferToFill);
//            TestBuffer(bufferToFill.Buffer, bufferToFill.StartSample, bufferToFill.NumSamples);

            var gainChannelInfo = _bufferPool.SetupGainBuffer(ref bufferToFill);
            _gainAmp.GetNextAudioBlock(ref gainChannelInfo);

            _sampleTime += bufferToFill.NumSamples;
        }

        private void TestBuffer(SampleBuffer buffer, int startSample, int numSamples)
        {
            for (int i = startSample; i < startSample + numSamples; i++)
            {
                if (buffer.Channels[0][i] > 0)
                {
                    LOG.Debug("Sample > 0 found");
                    break;
                }
            }
        }

        public void Dispose()
        {
            _bufferPool.Dispose();
        }
    }
}

