using System.Numerics;

namespace Chess; 

public static class BitboardHelper {
    
    public const ulong BottomRowMask = 0b11111111;
    public const ulong LeftColumnMask = 0x0101010101010101;
    public const ulong TopRowMask = 0xFF00000000000000;
    public const ulong RightColumnMask = 0x8080808080808080;
    public const ulong PositiveDiagonalMask = 0b1000000001000000001000000001000000001000000001000000001000000001;
    public const ulong NegativeDiagonalMask = 0b0000000100000010000001000000100000010000001000000100000010000000;
    public const ulong NotRank8 = ~(BottomRowMask << (7 * 8));
    public const ulong NotRank1 = ~BottomRowMask;
    public const ulong NotAFile = ~LeftColumnMask;
    public const ulong NotHFile = ~(LeftColumnMask << 7);
    private static ulong[][] DirectionalAttackBitboards;
    private static ulong[] AttackBitboards;
    
    static BitboardHelper() {
        int[] allDirections = CompassDirections.CardinalsAndDiagonals;
        DirectionalAttackBitboards = new ulong[64][];
        
        for (int squareIndex = 0; squareIndex < DirectionalAttackBitboards.Length; squareIndex++) {
            DirectionalAttackBitboards[squareIndex] = new ulong[allDirections.Length];
            for (int j = 0; j < allDirections.Length; j++) {
                int direction = allDirections[j];
                int directionIndex = CompassDirections.GetDirectionIndex(direction);
                DirectionalAttackBitboards[squareIndex][directionIndex] = GetAttackBitboard(squareIndex, direction);
            }
        }
        
        AttackBitboards = new ulong[64];
        for (int squareIndex = 0; squareIndex < AttackBitboards.Length; squareIndex++) {
            ulong attack = 0;
            foreach (int direction in allDirections) {
                attack |= GetAttackBitboard(squareIndex, direction);
            }
            AttackBitboards[squareIndex] = attack;
        }
    }
    
    public static bool BitboardContainsSquare(ulong bitboard, int squareIndex) {
        return (bitboard & (1ul << squareIndex)) != 0;
    }

    public static bool BitboardContainsAnyFromMask(ulong bitboard, ulong mask) {
        return (bitboard & mask) != 0;
    }

    public static void AddSquare(ref ulong bitboard, int squareIndex) {
        bitboard |= 1ul << squareIndex; 
    }
    
    public static void AddBitboard(ref ulong bitboard, ulong bitboardToAdd) {
        bitboard |= bitboardToAdd;
    }
    
    /// <summary>
    /// Clears the set bit that's the furthest right
    /// </summary>
    /// <returns>The position of the furthest right set bit</returns>
    public static int PopLeastSignificantBit(ref ulong value) {
        if (value == 0)
            return -1;
        
        int squareIndex = BitOperations.TrailingZeroCount(value);
        value &= value - 1;
        return squareIndex;
    }
    
    public static int PopMostSignificantBit(ref ulong value) {
        if (value == 0)
            return -1;
        
        int squareIndex = (sizeof(ulong) * 8) - BitOperations.LeadingZeroCount(value) - 1;
        // unlike LSB, there seems to not be a simple calculation of removing the most significant bit
        // without using any shifts, so I'm using this for now
        value ^= 1ul << squareIndex;
        return squareIndex;
    }
    
    public static int GetLeastSignificantBit(ulong value) {
        return BitOperations.TrailingZeroCount(value);
    }
    
    public static int GetMostSignificantBit(ulong value) {
        return (sizeof(ulong) * 8) - BitOperations.LeadingZeroCount(value) - 1;
    }

    public static int[] GetSquareIndexesFromBitboard(ulong bitboard) {
        int count = 0;
        Span<int> squareIndexes = stackalloc int[Board.Dimensions * Board.Dimensions];
        while (bitboard != 0) {
            int squareIndex = BitOperations.TrailingZeroCount(bitboard);
            squareIndexes[count++] = squareIndex;

            // Clear the least significant set bit
            bitboard &= bitboard - 1;
        }
        return squareIndexes[..count].ToArray();
    }

    public static void RemoveSquareFromBitboard(ref ulong bitboard, int squareIndex) {
        // creates a mask where only the squareIndex is a 1
        // then uses the not operator to switch all the 1s and 0s - the squareIndex is now the only 0 in the squareMask
        // then combines the mask with the bitboard so that it makes the squareIndex be a 0 in the original bitboard
        ulong squareMask = 1ul << squareIndex;
        squareMask = ~squareMask;
        bitboard &= squareMask;
    }

    public static void PrintBitboard(ulong bitboard, int dimensions) {
        string bitboardAsString = Convert.ToString((long)bitboard, 2).PadLeft(64, '0');
        for (int i = 0; i < dimensions; i++) {
            string attackedRow = bitboardAsString[(i * dimensions)..((i + 1) * dimensions)];
            string row = $"{dimensions - i} [ ";
            for (int j = attackedRow.Length - 1; j >= 0; j--) {
                row += $"{attackedRow[j]} ";
            }
            row += "]";
            Console.WriteLine(row);
        }
        Console.WriteLine("    A B C D E F G H");
    }

    /// <summary>
    /// Get the number of pieces in the bitboard
    /// </summary>
    /// <param name="bitboard">The bitboard that needs its bits counting</param>
    /// <returns>The number of bits equal to 1 in the given bitboard</returns>
    public static int GetPieceCount(ulong bitboard) {
        return BitOperations.PopCount(bitboard);
    }

    /// <summary>
    /// Get the number of pieces in the bitboard that are within the mask
    /// </summary>
    /// <param name="bitboard">The bitboard that needs its bits counting</param>
    /// <param name="mask">The mask that constraints which bits to check</param>
    /// <returns>The number of bits equal to 1 that match in both the bitboard and the mask</returns>
    public static int GetPieceCount(ulong bitboard, ulong mask) {
        return BitOperations.PopCount(bitboard & mask);
    }
    
    /// <summary>
    /// Creates a bitboard of columns from left to right
    /// </summary>
    /// <param name="numCols">The amount of columns that should be set to 1 in the bitboard</param>
    /// <returns></returns>
    public static ulong CreateColumns(int numCols) {
        ulong column = LeftColumnMask << numCols;
        return column - LeftColumnMask;
    }
    
    public static ulong OldColumnCreation(int numCols) {
        ulong columnMultiplier = (1ul << numCols);
        return LeftColumnMask * columnMultiplier - LeftColumnMask;
    }
    
    /// <summary>
    /// Creates a bitboard of rows from bottom to top
    /// </summary>
    /// <param name="numRows">The amount of rows that should be set to 1 in the bitboard</param>
    /// <returns></returns> 
    public static ulong CreateRows(int numRows) {
        if (numRows >= 8) {
            return ulong.MaxValue;
        }
        // setting the power of two
        ulong rowBitboard = 1ul << (numRows * 8);
        // miusing one to get all the bits before it set to one
        return rowBitboard - 1;
    }
    
    public static ulong GetAttackBitboardInSingleDirection(int squareIndex, int direction) {
        int directionIndex = CompassDirections.GetDirectionIndex(direction);
        return DirectionalAttackBitboards[squareIndex][directionIndex];
    }
    
    public static ulong GetAttackBitbardInAllDirections(int squareIndex) {
        return AttackBitboards[squareIndex];
    }
    
    public static ulong GetAttackBitboard(int squareIndex, int direction) {
        Coordinate squareCoord = new Coordinate(squareIndex);
        switch (direction) {
            case 8:
                // if the square is on the board edge
                if (squareCoord.y == 7)
                    return 0;
                ulong attack = LeftColumnMask << squareIndex + 8;
                return attack;
            case 1:
                if (squareCoord.x == 7)
                    return 0;
                attack = BottomRowMask << squareIndex + 1;
                attack &= BottomRowMask << squareCoord.y * 8;
                return attack;
            case -8:
                if (squareCoord.y == 0)
                    return 0;
                attack = RightColumnMask >> 63 - (squareIndex - 8);
                return attack;
            case -1:
                if (squareCoord.x == 0)
                    return 0;
                attack = TopRowMask >> 56 - (squareIndex - 8);
                attack &= BottomRowMask << squareCoord.y * 8;
                return attack;
            case 9:
                if (squareCoord.x == 7 || squareCoord.y == 7)
                    return 0;
                attack = PositiveDiagonalMask << squareIndex + 9;
                ulong columns = ~CreateColumns(squareCoord.x + 1);
                return attack & columns;
                // return attack;
            case -7:
                if (squareCoord.x == 7 || squareCoord.y == 0)
                    return 0;
                attack = NegativeDiagonalMask >> 63 - squareIndex;
                columns = ~CreateColumns(squareCoord.x + 1);
                return attack & columns;
                // return attack;
            case -9:
                if (squareCoord.x == 0 || squareCoord.y == 0)
                    return 0;
                attack = PositiveDiagonalMask >> 63 - (squareIndex - 9);
                columns = CreateColumns(squareCoord.x);
                return attack & columns;
                // return attack;
            case 7:
                if (squareCoord.x == 0 || squareCoord.y == 7)
                    return 0;
                attack = NegativeDiagonalMask << squareIndex;
                columns = CreateColumns(squareCoord.x);
                return attack & columns;
                // return attack;
            default:
                return 0;
        }
    }
    
    public static ulong GetLegalAttacks(ulong attacksInDirection, int direction, int blockerSquareIndex) {
        Coordinate blockerCoords = new Coordinate(blockerSquareIndex);
        ulong legalAttacks;
        switch (direction) {
            case CompassDirections.Right:
                legalAttacks = attacksInDirection;
                int xOffset = 7 - blockerCoords.x;
                legalAttacks >>= xOffset;
                legalAttacks &= attacksInDirection;
                break;
            case CompassDirections.Left:
                legalAttacks = attacksInDirection;
                xOffset = blockerCoords.x;
                legalAttacks <<= xOffset;
                legalAttacks &= attacksInDirection;
                break;
            case CompassDirections.Up:
                legalAttacks = attacksInDirection;
                int yOffset = 7 - blockerCoords.y;
                legalAttacks >>= yOffset * 8;
                legalAttacks &= attacksInDirection;
                break;
            case CompassDirections.Down:
                legalAttacks = attacksInDirection;
                yOffset = blockerCoords.y;
                legalAttacks <<= yOffset * 8;
                legalAttacks &= attacksInDirection;
                break;
            case CompassDirections.TopRight:
                legalAttacks = PositiveDiagonalMask;
                yOffset = (7 - blockerCoords.y) * 8;
                xOffset = 7 - blockerCoords.x;
                legalAttacks >>= yOffset;
                legalAttacks >>= xOffset;
                legalAttacks &= attacksInDirection;
                break;
            case CompassDirections.BottomLeft:
                legalAttacks = PositiveDiagonalMask;
                yOffset = blockerCoords.y * 8;
                xOffset = blockerCoords.x;
                legalAttacks <<= yOffset;
                legalAttacks <<= xOffset;
                legalAttacks &= attacksInDirection;
                break;
            case CompassDirections.TopLeft:
                legalAttacks = NegativeDiagonalMask;
                xOffset = blockerCoords.x;
                yOffset = (7 - blockerCoords.y) * 8;
                legalAttacks <<= xOffset;
                legalAttacks >>= yOffset;
                legalAttacks &= attacksInDirection;
                break;
            case CompassDirections.BottomRight:
                legalAttacks = NegativeDiagonalMask;
                xOffset = 7 - blockerCoords.x;
                yOffset = blockerCoords.y * 8;
                legalAttacks <<= yOffset;
                legalAttacks >>= xOffset;
                legalAttacks &= attacksInDirection;
                break;
            default:
                legalAttacks = 0;
                break;
        }
        return legalAttacks;
    }
}