namespace Chess_Application;

public class TSCPPieceTables : IPositionTables {
    
    public int[][] AllPositionTables { get; set; }
    
    public int[] BlackPawnPositionTable { get; set; } = {
    	  0,   0,   0,   0,   0,   0,   0,   0,
    	  5,  10,  15,  20,  20,  15,  10,   5,
    	  4,   8,  12,  16,  16,  12,   8,   4,
    	  3,   6,   9,  12,  12,   9,   6,   3,
    	  2,   4,   6,   8,   8,   6,   4,   2,
    	  1,   2,   3, -10, -10,   3,   2,   1,
    	  0,   0,   0, -40, -40,   0,   0,   0,
    	  0,   0,   0,   0,   0,   0,   0,   0
    };
    
    public int[] BlackKnightPositionTable { get; set; } = {
    	-10, -10, -10, -10, -10, -10, -10, -10,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10,   0,   5,   5,   5,   5,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   0,   5,   5,   5,   5,   0, -10,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10, -30, -10, -10, -10, -10, -30, -10
    };
    
    public int[] BlackBishopPositionTable { get; set; } = {
    	-10, -10, -10, -10, -10, -10, -10, -10,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10,   0,   5,   5,   5,   5,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   0,   5,   5,   5,   5,   0, -10,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10, -10, -20, -10, -10, -20, -10, -10
    };
    
    public int[] BlackRookPositionTable { get; set; } = {
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
    };
    
    public int[] BlackQueenPositionTable { get; set; } = {
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
    };
    
    public int[] BlackKingPositionTable { get; set; } = {
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-20, -20, -20, -20, -20, -20, -20, -20,
    	  0,  20,  40, -20,   0, -20,  40,  20
    };
    
    public int[] WhitePawnPositionTable { get; set; }
    public int[] WhiteKnightPositionTable { get; set; }
    public int[] WhiteBishopPositionTable { get; set; }
    public int[] WhiteRookPositionTable { get; set; }
    public int[] WhiteQueenPositionTable { get; set; }
    public int[] WhiteKingPositionTable { get; set; }
    
    public TSCPPieceTables() {
        WhitePawnPositionTable = BlackPawnPositionTable.GetFlippedArray();
        WhiteKnightPositionTable = BlackKnightPositionTable.GetFlippedArray();
        WhiteBishopPositionTable = BlackBishopPositionTable.GetFlippedArray();
        WhiteRookPositionTable = BlackRookPositionTable.GetFlippedArray();
        WhiteQueenPositionTable = BlackQueenPositionTable.GetFlippedArray();
        WhiteKingPositionTable = BlackKingPositionTable.GetFlippedArray();
        
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
}