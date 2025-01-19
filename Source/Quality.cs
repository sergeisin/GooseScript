namespace GooseScript
{
    public enum Validity : byte
    {
        Good,
        Invalid,
        Reserved,
        Questionable
    }

    public struct Quality
    {
        public Validity Validity { get; set; }
        public bool Overflow { get; set; }
        public bool OutofRange { get; set; }
        public bool BadReference { get; set; }
        public bool Oscillatory { get; set; }
        public bool Failure { get; set; }
        public bool OldData { get; set; }
        public bool Inconsistent { get; set; }
        public bool Inaccurate { get; set; }
        public bool Source { get; set; }
        public bool Test { get; set; }
        public bool OperatorBlocked { get; set; }
    }
}