namespace Chess_Application;

// with thanks to Sebastian Lague and Chess Programming and the chess programming wiki
// https://github.com/SebLague
// https://github.com/maksimKorzh/ -> https://www.youtube.com/@chessprogramming591

// the position tables are flipped due to how the pieces are stored in the piece array
// the 0th position in the piece array is the 0th square index (the most bottom left square) 
// however in these table arrays it would be the 56th position
// maybe revisit it and see whether reording the piece array would be more sensible...
// for now though, the position tables are just flipped and the top row is the bottom row, bottom row the top, etc..
public static class PositionTables {
    public static readonly int[] BlackPawnPositionTable = {
	      0,   0,   0,   0,   0,   0,  0,   0,
	     98, 134,  61,  95,  68, 126, 34, -11,
	     -6,   7,  26,  31,  65,  56, 25, -20,
	    -14,  13,   6,  21,  23,  12, 17, -23,
	    -27,  -2,  -5,  12,  17,   6, 10, -25,
	    -26,  -4,  -4, -10,   3,   3, 33, -12,
	    -35,  -1, -20, -23, -15,  24, 38, -22,
	      0,   0,   0,   0,   0,   0,  0,   0,
	};
	
	// knight positional score
	public static readonly int[] BlackKnightPositionTable = {
	    -167, -89, -34, -49,  61, -97, -15, -107,
	     -73, -41,  72,  36,  23,  62,   7,  -17,
	     -47,  60,  37,  65,  84, 129,  73,   44,
	     -9,   17,  19,  53,  37,  69,  18,   22,
	     -13,   4,  16,  13,  28,  19,  21,   -8,
	     -23,  -9,  12,  10,  19,  17,  25,  -16,
	     -29, -53, -12,  -3,  -1,  18, -14,  -19,
	    -105, -21, -58, -33, -17, -28, -19,  -23,
	};
	
	// bishop positional score
	public static readonly int[] BlackBishopPositionTable = {
	    -29,   4, -82, -37, -25, -42,   7,  -8,
	    -26,  16, -18, -13,  30,  59,  18, -47,
	    -16,  37,  43,  40,  35,  50,  37,  -2,
	     -4,   5,  19,  50,  37,  37,   7,  -2,
	     -6,  13,  13,  26,  34,  12,  10,   4,
	      0,  15,  15,  15,  14,  27,  18,  10,
	      4,  15,  16,   0,   7,  21,  33,   1,
	    -33,  -3, -14, -21, -13, -12, -39, -21,
	};
	
	// rook positional score
	public static readonly int[] BlackRookPositionTable = {
	     32,  42,  32,  51, 63,  9,  31,  43,
	     27,  32,  58,  62, 80, 67,  26,  44,
	     -5,  19,  26,  36, 17, 45,  61,  16,
	    -24, -11,   7,  26, 24, 35,  -8, -20,
	    -36, -26, -12,  -1,  9, -7,   6, -23,
	    -45, -25, -16, -17,  3,  0,  -5, -33,
	    -44, -16, -20,  -9, -1, 11,  -6, -71,
	    -19, -13,   1,  17, 16,  7, -37, -26,
	
	};
	
	// queen positional score
	public static readonly int[] BlackQueenPositionTable = {
	    -28,   0,  29,  12,  59,  44,  43,  45,
	    -24, -39,  -5,   1, -16,  57,  28,  54,
	    -13, -17,   7,   8,  29,  56,  47,  57,
	    -27, -27, -16, -16,  -1,  17,  -2,   1,
	     -9, -26,  -9, -10,  -2,  -4,   3,  -3,
	    -14,   2, -11,  -2,  -5,   2,  14,   5,
	    -35,  -8,  11,   2,   8,  15,  -3,   1,
	     -1, -18,  -9,  10, -15, -25, -31, -50,
	};
	
	// king positional score
	public static readonly int[] BlackKingPositionTable = {
	    -65,  23,  16, -15, -56, -34,   2,  13,
	     29,  -1, -20,  -7,  -8,  -4, -38, -29,
	     -9,  24,   2, -16, -20,   6,  22, -22,
	    -17, -20, -12, -27, -30, -25, -14, -36,
	    -49,  -1, -27, -39, -46, -44, -33, -51,
	    -14, -14, -22, -46, -44, -30, -15, -27,
	      1,   7,  -8, -64, -43, -16,   9,   8,
	    -15,  36,  12, -54,   8, -28,  24,  14,
	};




    public static readonly int[] WhitePawnPositionTable;
    public static readonly int[] WhiteKnightPositionTable;
    public static readonly int[] WhiteBishopPositionTable;
    public static readonly int[] WhiteRookPositionTable;
    public static readonly int[] WhiteQueenPositionTable;
    public static readonly int[] WhiteKingPositionTable;
	private static readonly int[] flippedTable = {
		56, 57, 58, 59, 60, 61, 62, 63,
		48, 49, 50, 51, 52, 53, 54, 55,
		40, 41, 42, 43, 44, 45, 46, 47,
		32, 33, 34, 35, 36, 37, 38, 39,
		24, 25, 26, 27, 28, 29, 30, 31,
		16, 17, 18, 19, 20, 21, 22, 23,
		 8,  9, 10, 11, 12, 13, 14, 15,
		 0,  1,  2,  3,  4,  5,  6,  7,
		
	};

	static PositionTables() {
		WhitePawnPositionTable = GetFlippedPositionTable(BlackPawnPositionTable);
		WhiteKnightPositionTable = GetFlippedPositionTable(BlackKnightPositionTable);
		WhiteBishopPositionTable = GetFlippedPositionTable(BlackBishopPositionTable);
		WhiteRookPositionTable = GetFlippedPositionTable(BlackRookPositionTable);
		WhiteQueenPositionTable = GetFlippedPositionTable(BlackQueenPositionTable);
		WhiteKingPositionTable = GetFlippedPositionTable(BlackKingPositionTable);
	}

    public static int[] GetFlippedPositionTable(int[] positionTable) {
        int[] flippedPositionTable = new int[positionTable.Length];

        // for (int x = 0, flippedX = 7; x < 8; x++, flippedX--) {
        //     for (int y = 0, flippedY = 7; y < 8; y++, flippedY--) {
        //         int tableValue = positionTable[x + y * 8];
        //         flippedPositionTable[flippedX + flippedY * 8] = tableValue;
        //     }
        // }

        for (int i = 0; i < positionTable.Length; i++) {
            flippedPositionTable[i] = positionTable[flippedTable[i]];
		}
        return flippedPositionTable;
    }
}