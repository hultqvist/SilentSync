using System;
using System.Collections.Generic;
using System.Text;

namespace SilentOrbit.Tools
{
    public static class Expect
    {
        public static int ExpectOK(this int value)
        {
            if (value != 0)
                throw new Exception("Expected OK/0, got " + value);
            return value;
        }
    }
}
