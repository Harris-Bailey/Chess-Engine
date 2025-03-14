namespace Chess_Application;

public class ChessProgrammingPieceTables : IPositionTables {
    
    public int[][] AllPositionTables { get; set; }
    
    // pawn positional score
    public int[] BlackPawnPositionTable { get; set; } = {
        90,  90,  90,  90,  90,  90,  90,  90,
        30,  30,  30,  40,  40,  30,  30,  30,
        20,  20,  20,  30,  30,  30,  20,  20,
        10,  10,  10,  20,  20,  10,  10,  10,
        5,   5,  10,  20,  20,   5,   5,   5,
        0,   0,   0,   5,   5,   0,   0,   0,
        0,   0,   0, -10, -10,   0,   0,   0,
        0,   0,   0,   0,   0,   0,   0,   0
    };
    
    // knight positional score
    public int[] BlackKnightPositionTable { get; set; } = {
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,  10,  10,   0,   0,  -5,
        -5,   5,  20,  20,  20,  20,   5,  -5,
        -5,  10,  20,  30,  30,  20,  10,  -5,
        -5,  10,  20,  30,  30,  20,  10,  -5,
        -5,   5,  20,  10,  10,  20,   5,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5, -10,   0,   0,   0,   0, -10,  -5
    };
    
    // bishop positional score
    public int[] BlackBishopPositionTable { get; set; } = {
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   0,  10,  10,   0,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,  10,   0,   0,   0,   0,  10,   0,
        0,  30,   0,   0,   0,   0,  30,   0,
        0,   0, -10,   0,   0, -10,   0,   0
    
    };
    
    // rook positional score
    public int[] BlackRookPositionTable { get; set; } = {
        50,  50,  50,  50,  50,  50,  50,  50,
        50,  50,  50,  50,  50,  50,  50,  50,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,   0,  20,  20,   0,   0,   0
    
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
    
    // king positional score
    public int[] BlackKingPositionTable { get; set; } = {
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   5,   5,   5,   5,   0,   0,
        0,   5,   5,  10,  10,   5,   5,   0,
        0,   5,  10,  20,  20,  10,   5,   0,
        0,   5,  10,  20,  20,  10,   5,   0,
        0,   0,   5,  10,  10,   5,   0,   0,
        0,   5,   5,  -5,  -5,   0,   5,   0,
        0,   0,   5,   0, -15,   0,  10,   0
    };
    public int[] WhitePawnPositionTable { get; set; }
    public int[] WhiteKnightPositionTable { get; set; }
    public int[] WhiteBishopPositionTable { get; set; }
    public int[] WhiteRookPositionTable { get; set; }
    public int[] WhiteQueenPositionTable { get; set; }
    public int[] WhiteKingPositionTable { get; set; }

    public ChessProgrammingPieceTables() {
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