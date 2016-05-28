using System;
using System.Collections.Generic;

namespace SynthTest
{
    public class Parameter
    {
        public event Action<Parameter, float> WillChanged;

        float _value;
        public float Value
        {
            get { return _value; }
            set 
            {
                if (_value != value)
                {
                    var willChanged = WillChanged;
                    if (willChanged != null)
                    {
                        willChanged(this, value);
                    }
                    _value = value;
                }
            }
        }
    }

    public class Parameters
    {
        private List<Parameter> _parameters = new List<Parameter>();

        public Parameter this [int index]
        {
            get
            {
                return _parameters[index];
            }
        }
    }
}

