namespace Chess_Application;

public class SebLaguePieceTables : IPositionTables {
	
	public int[][] AllPositionTables { get; set; }
    public int[] BlackPawnPositionTable { get; set; } = {
    	 0,   0,   0,   0,   0,   0,   0,   0,
    	50,  50,  50,  50,  50,  50,  50,  50,
    	10,  10,  20,  30,  30,  20,  10,  10,
    	 5,   5,  10,  25,  25,  10,   5,   5,
    	 0,   0,   0,  20,  20,   0,   0,   0,
    	 5,  -5, -10,   0,   0, -10,  -5,   5,
    	 5,  10,  10, -20, -20,  10,  10,   5,
    	 0,   0,   0,   0,   0,   0,   0,   0
    };
    
    public int[] BlackRookPositionTable { get; set; } = {
    	 0,  0,  0,  0,  0,  0,  0,  0,
    	 5, 10, 10, 10, 10, 10, 10,  5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	 0,  0,  0,  5,  5,  0,  0,  0
    };
    
    public int[] BlackKnightPositionTable { get; set; } = {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20,   0,   0,   0,   0, -20, -40,
        -30,   0,  10,  15,  15,  10,   0, -30,
        -30,   5,  15,  20,  20,  15,   5, -30,
        -30,   0,  15,  20,  20,  15,   0, -30,
        -30,   5,  10,  15,  15,  10,   5, -30,
        -40, -20,   0,   5,   5,   0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50,
    };
    
    public int[] BlackBishopPositionTable { get; set; } = {
    	-20, -10, -10, -10, -10, -10, -10, -20,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   5,   5,  10,  10,   5,   5, -10,
    	-10,   0,  10,  10,  10,  10,   0, -10,
    	-10,  10,  10,  10,  10,  10,  10, -10,
    	-10,   5,   0,   0,   0,   0,   5, -10,
    	-20, -10, -10, -10, -10, -10, -10, -20,
    };
    
    public int[] BlackQueenPositionTable { get; set; } = {
    	-20, -10, -10, -5, -5, -10, -10, -20,
    	-10,   0,   0,  0,  0,   0,   0, -10,
    	-10,   0,   5,  5,  5,   5,   0, -10,
    	 -5,   0,   5,  5,  5,   5,   0,  -5,
    	  0,   0,   5,  5,  5,   5,   0,  -5,
    	-10,   5,   5,  5,  5,   5,   0, -10,
    	-10,   0,   5,  0,  0,   0,   0, -10,
    	-20, -10, -10, -5, -5, -10, -10, -20
    };
    
    public int[] BlackKingPositionTable { get; set; } = {
    	-80, -70, -70, -70, -70, -70, -70, -80, 
    	-60, -60, -60, -60, -60, -60, -60, -60, 
    	-40, -50, -50, -60, -60, -50, -50, -40, 
    	-30, -40, -40, -50, -50, -40, -40, -30, 
    	-20, -30, -30, -40, -40, -30, -30, -20, 
    	-10, -20, -20, -20, -20, -20, -20, -10, 
    	 20,  20,  -5,  -5,  -5,  -5,  20,  20, 
    	 20,  30,  10,   0,   0,  10,  30,  20
    };
    public int[] WhitePawnPositionTable { get; set; }
    public int[] WhiteKnightPositionTable { get; set; }
    public int[] WhiteBishopPositionTable { get; set; }
    public int[] WhiteRookPositionTable { get; set; }
    public int[] WhiteQueenPositionTable { get; set; }
    public int[] WhiteKingPositionTable { get; set; }
    
    public SebLaguePieceTables() {
        WhitePawnPositionTable = BlackPawnPositionTable.GetFlippedArray();
        WhiteKnightPositionTable = BlackKnightPositionTable.GetFlippedArray();
        WhiteBishopPositionTable = BlackBishopPositionTable.GetFlippedArray();
        WhiteRookPositionTable = BlackRookPositionTable.GetFlippedArray();
        WhiteQueenPositionTable = BlackQueenPositionTable.GetFlippedArray();
        WhiteKingPositionTable = BlackKingPositionTable.GetFlippedArray();
		
		AllPositionTables = new int[Board.NumPieces * Enum.GetValues(typeof(Team)).Length][];
		// AllPositionTables[PieceHandler.GetPieceID<Pawn>() * (int)Team.White] = WhitePawnPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<Knight>() * (int)Team.White] = WhiteKnightPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<Bishop>() * (int)Team.White] = WhiteBishopPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<Rook>() * (int)Team.White] = WhiteRookPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<Queen>() * (int)Team.White] = WhiteQueenPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<King>() * (int)Team.White] = WhiteKingPositionTable;
		
		// AllPositionTables[PieceHandler.GetPieceID<Pawn>() * (int)Team.Black] = WhitePawnPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<Knight>() * (int)Team.Black] = WhiteKnightPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<Bishop>() * (int)Team.Black] = WhiteBishopPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<Rook>() * (int)Team.Black] = WhiteRookPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<Queen>() * (int)Team.Black] = WhiteQueenPositionTable;
		// AllPositionTables[PieceHandler.GetPieceID<King>() * (int)Team.Black] = WhiteKingPositionTable;
    }
}