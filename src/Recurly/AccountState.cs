using System;

namespace Recurly
{
    // The currently valid account states
    // Corrected to allow multiple states, per https://dev.recurly.com/docs/get-account
    [Flags]
    public enum AccountState : short
    {
        Closed = 1,
        Active = 2,
        PastDue = 4
    }
}