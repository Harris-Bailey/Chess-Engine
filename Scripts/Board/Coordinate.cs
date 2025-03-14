using System.Diagnostics.CodeAnalysis;

namespace Chess;

public struct Coordinate {
    public int x;
    public int y;

    public Coordinate(int x, int y) {
        this.x = x;
        this.y = y;
    }
    
    public int ConvertToSquareIndex() {
        // this assumes that the board dimensions are 8
        return x + y * 8;
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

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is not Coordinate coordinate)
            return false;
        return coordinate == this;
    }

    // copied from Unity using https://forum.unity.com/threads/does-vector3-implement-gethashcode.362499/
    public override int GetHashCode() {
        return HashCode.Combine(x, y);
    }
}
