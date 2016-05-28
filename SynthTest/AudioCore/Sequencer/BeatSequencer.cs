using System;

namespace SynthTest
{
    public class BeatSequencer
    {
        private Sequencer _sequencer = new Sequencer();
        public Sequencer Sequencer { get { return _sequencer; } }

        private float _bpm;
        public float BPM { get { return _bpm; } }

        private float _sampleRate;
        public float SampleRate { get { return _sampleRate; } }

        public int BeatsPerBar { get { return 4; } }

        public BeatSequencer(float sampleRate, float bpm)
        {
            _sampleRate = sampleRate;
            _bpm = bpm;

            EnableLoop();
        }

        void EnableLoop()
        {
            _sequencer.AddCommand(SeqCommand.SetLoop());
        }

        public float BarsToSeconds(float bars)
        {
            return bars * (BeatsPerBar / BPM) * 60.0f;
        }

        public int RatioToSampleTime(int bar, float sub)
        {
            return (int)((BarsToSeconds(bar) + BarsToSeconds(sub)) * SampleRate);
        }

        public void SetLength(int bars)
        {
            _sequencer.AddCommand(
                SeqCommand.SetLength((int)Math.Ceiling(BarsToSeconds(bars) * SampleRate))
            );
        }

        public void AddNote(int bar, float sub, float length, float frequency, float velocity)
        {
            _sequencer.AddCommand(
                SeqCommand.InsertNote(
                    position:  RatioToSampleTime(bar, sub),
                    length:    RatioToSampleTime(0, length),
                    frequency: frequency,
                    velocity:  velocity
                )
            );
        }
    }
}

