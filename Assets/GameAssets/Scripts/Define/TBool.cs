namespace NKPB
{
    public struct boolean
    {
        private readonly byte _value;
        public boolean(bool value) { _value = (byte)(value ? 1 : 0); }
        public static implicit operator boolean(bool value) { return new boolean(value); }
        public static implicit operator bool(boolean value) { return value._value != 0; }
        public override string ToString() { return (_value != 0) ? "true" : "false"; }
    }
}
