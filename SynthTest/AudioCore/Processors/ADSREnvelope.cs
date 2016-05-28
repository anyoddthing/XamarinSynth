using System;

namespace SynthTest
{
    // TODO: find reference where I got this from...

    public unsafe class ADSREnvelope
    {
        private static readonly Logger LOG = Logger.Debug<ADSREnvelope>();

        const float DB90 = (1.0f / (1 << 15));

        public enum State { IDLE, ATTACKING, DECAYING, SUSTAINING, RELEASING };

        private float Attack;
        private float Decay;
        private float Sustain;
        private float Release;
        private float TargetRatioA;
        private float TargetRatioDR;

        private State _state;
        private float _level;
        private float _attackCoef;
        private float _decayCoef;
        private float _releaseCoef;
        private float _attackBase;
        private float _decayBase;
        private float _releaseBase;

        private Sequencer _sequencer;
        private int _frameRate;

        public ADSREnvelope(Sequencer sequencer)
        {
            _sequencer = sequencer;
        }

        public void PrepareToPlay(int frameRate)
        {
            Attack = 0.01f;
            Decay = 0.2f;
            Sustain = 0.8f;
            Release = 0.3f;
            TargetRatioA = 0.3f;
            TargetRatioDR = 0.001f;

            _state = State.IDLE;
            _frameRate = frameRate;

            UpdateParams();
        }

        public void GetNextAudioBlock(SampleBuffer buffer, int start, int length)
        {
            var dest = buffer.GetPointer(0, start);

            var bufferNote = _sequencer.GetBufferNote();
            if (_state != State.RELEASING && (!bufferNote.IsValid || bufferNote.Start > length))
            {
                Memory.ZeroMem<float>(dest, length);
                return;
            }

            var bufferPos = 0;
            while (bufferPos < length)
            {
                if (bufferNote.IsTriggered(bufferPos))
                {
                    LOG.Debug("Trigger");
                    _state = State.ATTACKING;
                }
                else if (!bufferNote.IsPlaying(bufferPos) && _state != State.IDLE)
                {
                    if (_state != State.RELEASING) LOG.Debug("Start Release");
                    _state = State.RELEASING;
                }

                switch (_state)
                {
                    case State.IDLE:
                        _level = 0;
                        break;
                    case State.ATTACKING:
                        _level = _attackBase + _level * _attackCoef;
                        if (_level >= 1.0f)
                        {
                            _level = 1.0f;
                            _state = State.DECAYING;
                            LOG.Debug("Start Decay");
                        }
                        break;
                    case State.DECAYING:
                        _level = _decayBase + _level * _decayCoef;
                        if (_level <= Sustain)
                        {
                            _level = Sustain;
                            _state = State.SUSTAINING;
                            LOG.Debug("Start Sustain");
                        }
                        break;
                    case State.RELEASING:
                        _level = _releaseBase + _level * _releaseCoef;
                        if (_level < DB90)
                        {
                            _level = 0;
                            _state = State.IDLE;
                            LOG.Debug("Start Idle");
                        }
                        break;
//                    default:
//                        break;
                }

                dest[bufferPos++] = _level;

//                if (bufferPos > bufferNote.End)
//                {
//                    bufferNote = _sequencer.GetBufferNote(bufferPos);
//                    if (bufferNote.Start > length)
//                    {
//                        Memory.ZeroMem(dest + bufferPos, length - bufferPos);
//                        return;
//                    }
//                }
            }
        }

        void UpdateParams()
        {
            _attackCoef = calcCoef(Attack, TargetRatioA);
            _attackBase = (1.0f + TargetRatioA) * (1.0f - _attackCoef);

            _decayCoef = calcCoef(Decay, TargetRatioDR);
            _decayBase = (Sustain - TargetRatioDR) * (1.0f - _decayCoef);

            _releaseCoef = calcCoef(Release, TargetRatioDR);
            _releaseBase = -TargetRatioDR * (1.0f - _decayCoef);

            _decayBase = (Sustain - TargetRatioDR) * (1.0f - _decayCoef);

            LOG.Debug("Attack {0}*{1}", _attackBase, _attackCoef);
            LOG.Debug("Release {0}*{1}", _releaseBase, _releaseCoef);
        }

        float calcCoef(float rate, float targetRatio)
        {
            var numSamples = rate * _frameRate;
            return (float)Math.Exp(-Math.Log((1.0f + targetRatio) / targetRatio) / numSamples);
        }
    }
}

