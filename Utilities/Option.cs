using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikesExtraHotkey.Utilities
{
    public class OptionBase
    {
        public OptionBase() { }
    }

    public class Some<T>: OptionBase
    {
        private T Value;
        public Some(T value) 
        {
            Value = value;
        }

        public static implicit operator Option<T>(Some<T> value) => new Option<T>(value);
    }

    public class None<T>: OptionBase 
    { 
        public None() { }

        public static implicit operator Option<T>(None<T> value) => new Option<T>(value);
    }

    public class Option<T>
    {
        private OptionBase Value;

        public Option(None<T> value)
        {
            Value= value;
        }

        public Option(Some<T> value)
        {
            Value = value;
        }

    }
}
