using System.Diagnostics.CodeAnalysis;

namespace BoardRepresentations;
readonly struct Fen
{
    public readonly string FenString;
    public Fen(string Fen)
    {
        FenString = Fen;
    }
    public override int GetHashCode()
    {
        return FenString.GetHashCode();
    }
    public static bool operator ==(Fen lhs, Fen Rhs)
    {
        return lhs.FenString == Rhs.FenString;
    }
    public static bool operator !=(Fen lhs, Fen Rhs) => !(lhs == Rhs);
    public override bool Equals(object? obj)
    {
        return obj is Fen fenObj && fenObj == this;
    }
    public static explicit operator Fen(string fen) => new Fen(fen);
}