namespace OpenMedStack.Infrastructure.Bootstrapping.Tailf
{
    using System;

    internal class TailEventArgs : EventArgs
    {
        public TailEventArgs(string line)
        {
            Line = line;
        }

        public string Line { get; }
    }
}