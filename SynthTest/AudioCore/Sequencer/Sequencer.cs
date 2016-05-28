using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SynthTest
{
    public struct SeqCommand
    {
        public enum CommandType
        {
            SetLength,
            Start, Stop, 
            SetLoop, UnsetLoop,
            InsertNote, RemoveNote,
            ScaleNotes
        }

        public static SeqCommand SetLength(int numSamples)
        {
            return new SeqCommand
            {
                Type = CommandType.SetLength,
                IntValue1 = numSamples
            };
        }

        public static SeqCommand Start()
        {
            return new SeqCommand { Type = CommandType.Start };
        }

        public static SeqCommand Stop()
        {
            return new SeqCommand { Type = CommandType.Stop };
        }

        public static SeqCommand SetLoop()
        {
            return new SeqCommand { Type = CommandType.SetLoop };
        }

        public static SeqCommand UnsetLoop()
        {
            return new SeqCommand { Type = CommandType.UnsetLoop };
        }

        public static SeqCommand InsertNote(int position, int length, float frequency, float velocity)
        {
            return new SeqCommand
            {
                Type        = CommandType.InsertNote,
                IntValue1   = position,
                IntValue2   = length,
                FloatValue1 = frequency,
                FloatValue2 = velocity
            };
        }

        public static SeqCommand RemoveNote(int index)
        {
            return new SeqCommand
            {
                Type = CommandType.RemoveNote,
                IntValue1 = index
            };
        }

        public static SeqCommand ScaleNotes(int newLength)
        {
            return new SeqCommand
            {
                Type = CommandType.ScaleNotes,
                IntValue1 = newLength
            };
        }

        public CommandType Type;
        public int IntValue1;
        public int IntValue2;
        public float FloatValue1;
        public float FloatValue2;

        public override string ToString()
        {
            return string.Format("[SeqCommand: {0}]", Type);
        }
    }

    public struct SeqNote
    {
        public int Length;
        public float Frequency;
        public float Velocity;

        public BufferNote ToBufferNote(int sampleTime)
        {
            return new BufferNote
            {
                Start     = sampleTime,
                End       = sampleTime + Length,
                Velocity  = Velocity,
                Frequency = Frequency,
            };
        }
    }

    public struct BufferNote
    {
        public int Start;
        public int End;
        public float Frequency;
        public float Velocity;

        public int Length { get { return End - Start; } }
        public bool IsValid { get { return Length > 0; } }

        public bool IsPlaying(int pos)
        {
            return pos > Start && pos < End;
        }

        public bool IsTriggered(int pos = 0)
        {
            return Start == pos;
        }
    }

    public class Sequencer
    {
        private static readonly Logger LOG = Logger.Debug<Sequencer>();

        private ConcurrentQueue<SeqCommand> _commands = new ConcurrentQueue<SeqCommand>();

        private readonly List<int> _notePosition = new List<int>();
        private readonly List<SeqNote> _notes = new List<SeqNote>();

        public int CurrentFrame { get; private set; }
        public int SampleTime { get; private set; }
        public int TotalLength { get; private set; }
        public bool IsLooping { get; private set; }
        public bool IsRunning { get; private set; }

        public void AddCommand(SeqCommand command)
        {
            _commands.Enqueue((command));
        }

        public bool Start()
        {
            return StartFrame(0);
        }

        public bool StartFrame(int sampleTime)
        {
            SampleTime = sampleTime;
            SeqCommand command;
            while (_commands.TryDequeue(out command))
            {
                LOG.Debug("Apply Command: {0}", command);
                ApplyCommand(ref command);
            }

            if (TotalLength <= 0)
                return false;

            if (sampleTime >= TotalLength)
            {
                if (IsLooping)
                {
                    CurrentFrame = (sampleTime % TotalLength);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                CurrentFrame = sampleTime;
            }

            return true;
        }

        public BufferNote GetBufferNote(int deltaT = 0)
        {
            if (_notePosition.Count == 0)
                return new BufferNote();

            var seqTime = CurrentFrame + deltaT;

            var pos = _notePosition.BinarySearch(seqTime);
            if (pos < 0)
            {
                pos = ~pos;
                if (pos == 0)
                {
                    return _notes[0].ToBufferNote(deltaT);
                }
                else
                {
                    // get previous note
                    var prevPos    = pos - 1;
//                    LOG.Debug("prev pos: {0}", prevPos);
                    if (prevPos < 0 || prevPos >= _notePosition.Count)
                    {
                        throw new Exception(prevPos.ToString());
                    }
                    var prevStart  = _notePosition[prevPos];
                    var prevLength = _notes[prevPos].Length;

                    if ((prevStart + prevLength) > seqTime)
                    {
                        // prev still paying
                        return _notes[prevPos].ToBufferNote(prevStart - seqTime);
                    }
                    else if (pos < _notes.Count)
                    {
                        // return next note
                        var nextStart = _notePosition[pos];
                        return _notes[pos].ToBufferNote(nextStart - seqTime);
                    }
                    else
                    {
                        // next note wrapps to first
                        var firstStart = _notePosition[0];
                        return _notes[0].ToBufferNote(TotalLength + firstStart - seqTime);
                    }
                }
            }
            else
            {
                // direct hit
                return _notes[pos].ToBufferNote(deltaT);
            }
        }

        void InsertNote(ref SeqCommand command)
        {
            var note = new SeqNote
            {
                Length    = command.IntValue2,
                Frequency = command.FloatValue1,
                Velocity  = command.FloatValue2,
            };

            var pos = _notePosition.BinarySearch(command.IntValue1);
            if (pos < 0)
            {
                _notePosition.Insert(~pos, command.IntValue1);
                _notes.Insert(~pos, note);
            }
            else
            {
                _notePosition[pos] = command.IntValue1;
                _notes[pos] = note;
            }
        }

        void RemoveNote(ref SeqCommand command)
        {
            var pos = command.IntValue1;
            if (pos >= 0)
            {
                _notePosition.RemoveAt(pos);
                _notes.RemoveAt(pos);
            }
        }

        void ScaleNotes(ref SeqCommand command)
        {
            var newLength = command.IntValue1;
            var factor = ((double)newLength) / TotalLength;
            for (int i = 0; i < _notePosition.Count; i++)
            {
                _notePosition[i] = (int)Math.Round(factor * _notePosition[i]);

                var note = _notes[i];
                note.Length = (int)Math.Round(factor * note.Length);
                _notes[i] = note;
            }
        }

        void ApplyCommand(ref SeqCommand command)
        {
            switch (command.Type)
            {
                case SeqCommand.CommandType.SetLength:
                    TotalLength = command.IntValue1;
                    break;
                case SeqCommand.CommandType.Start:
                    IsRunning = true;
                    break;
                case SeqCommand.CommandType.Stop:
                    IsRunning = false;
                    break;
                case SeqCommand.CommandType.SetLoop:
                    IsLooping = true;
                    break;
                case SeqCommand.CommandType.UnsetLoop:
                    IsLooping = false;
                    break;
                case SeqCommand.CommandType.InsertNote:
                    InsertNote(ref command);
                    break;
                case SeqCommand.CommandType.RemoveNote:
                    RemoveNote(ref command);
                    break;
                case SeqCommand.CommandType.ScaleNotes:
                    ScaleNotes(ref command);
                    break;
                default:
                    break;
            }
        }
    }
}

