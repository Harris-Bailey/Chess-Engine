namespace Chess;

public abstract class PositionTables {
    public int[][] AllPositionTables { get; protected set; }
    public abstract int[] BlackPawnPositionTable { get; protected set; }
    public abstract int[] BlackKnightPositionTable { get; protected set; }
    public abstract int[] BlackBishopPositionTable { get; protected set; }
    public abstract int[] BlackRookPositionTable { get; protected set; }
    public abstract int[] BlackQueenPositionTable { get; protected set; }
    public abstract int[] BlackKingPositionTable { get; protected set; }
    
    public int[] WhitePawnPositionTable { get; protected set; }
    public int[] WhiteKnightPositionTable { get; protected set; }
    public int[] WhiteBishopPositionTable { get; protected set; }
    public int[] WhiteRookPositionTable { get; protected set; }
    public int[] WhiteQueenPositionTable { get; protected set; }
    public int[] WhiteKingPositionTable { get; protected set; }
    
    public PositionTables() {
        WhitePawnPositionTable = GetFlippedArray(BlackPawnPositionTable);
        WhiteKnightPositionTable = GetFlippedArray(BlackKnightPositionTable);
        WhiteBishopPositionTable = GetFlippedArray(BlackBishopPositionTable);
        WhiteRookPositionTable = GetFlippedArray(BlackRookPositionTable);
        WhiteQueenPositionTable = GetFlippedArray(BlackQueenPositionTable);
        WhiteKingPositionTable = GetFlippedArray(BlackKingPositionTable);
        
        AllPositionTables = new int[Board.NumPieces * Enum.GetValues(typeof(Team)).Length][];
		AllPositionTables[(int)BitboardIndexes.PawnIndex * (int)Team.White] = WhitePawnPositionTable;
		AllPositionTables[(int)BitboardIndexes.KnightIndex * (int)Team.White] = WhiteKnightPositionTable;
		AllPositionTables[(int)BitboardIndexes.BishopIndex * (int)Team.White] = WhiteBishopPositionTable;
		AllPositionTables[(int)BitboardIndexes.RookIndex * (int)Team.White] = WhiteRookPositionTable;
		AllPositionTables[(int)BitboardIndexes.QueenIndex * (int)Team.White] = WhiteQueenPositionTable;
		AllPositionTables[(int)BitboardIndexes.KingIndex * (int)Team.White] = WhiteKingPositionTable;
		
		AllPositionTables[(int)BitboardIndexes.PawnIndex * (int)Team.Black] = WhitePawnPositionTable;
		AllPositionTables[(int)BitboardIndexes.KnightIndex * (int)Team.Black] = WhiteKnightPositionTable;
		AllPositionTables[(int)BitboardIndexes.BishopIndex * (int)Team.Black] = WhiteBishopPositionTable;
		AllPositionTables[(int)BitboardIndexes.RookIndex * (int)Team.Black] = WhiteRookPositionTable;
		AllPositionTables[(int)BitboardIndexes.QueenIndex * (int)Team.Black] = WhiteQueenPositionTable;
		AllPositionTables[(int)BitboardIndexes.KingIndex * (int)Team.Black] = WhiteKingPositionTable;
    }
    
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
    
    public static int[] GetFlippedArray(int[] arr) {
        int[] flippedArr = new int[arr.Length];
        for (int i = 0; i < arr.Length; i++) {
            flippedArr[i] = arr[flippedTable[i]];
        }
        return flippedArr;
    }
}