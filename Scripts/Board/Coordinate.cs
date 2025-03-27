using System.Diagnostics.CodeAnalysis;

namespace Chess;

public readonly struct Coordinate {
    public readonly int x;
    public readonly int y;

    public Coordinate(int x, int y) {
        this.x = x;
        this.y = y;
    }
    
    public Coordinate(int squareIndex) {
        x = squareIndex % Board.Dimensions;
        y = squareIndex / Board.Dimensions;
    }
    
    public readonly int ConvertToSquareIndex() {
        // this assumes that the board dimensions are 8
        return x + y * Board.Dimensions;
    }
    
    public static Coordinate operator +(Coordinate a, Coordinate b) {
        return new Coordinate(a.x + b.x, a.y + b.y);
    }
    
    public static bool operator ==(Coordinate a, Coordinate b) {
        return a.x == b.x && a.y == b.y;
    }
    
    public static bool operator !=(Coordinate a, Coordinate b) {
        return !(a == b);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is not Coordinate coordinate)
            return false;
        return coordinate == this;
    }

    // copied from Unity using https://forum.unity.com/threads/does-vector3-implement-gethashcode.362499/
    public override readonly int GetHashCode() {
        return HashCode.Combine(x, y);
    }
}
