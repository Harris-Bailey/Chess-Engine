namespace Chess;

public abstract class PositionTables {
    public int[][] AllEarlyPositionTables { get; protected set; }
    protected abstract int[] BlackPawnEarlyPositionTable { get; }
    protected abstract int[] BlackKnightEarlyPositionTable { get; }
    protected abstract int[] BlackBishopEarlyPositionTable { get; }
    protected abstract int[] BlackRookEarlyPositionTable { get; }
    protected abstract int[] BlackQueenEarlyPositionTable { get; }
    protected abstract int[] BlackKingEarlyPositionTable { get; }    
    
    
    public int[][] AllLatePositionTables { get; protected set; }
    protected abstract int[] BlackPawnLatePositionTable { get; }
    protected abstract int[] BlackKnightLatePositionTable { get; }
    protected abstract int[] BlackBishopLatePositionTable { get; }
    protected abstract int[] BlackRookLatePositionTable { get; }
    protected abstract int[] BlackQueenLatePositionTable { get; }
    protected abstract int[] BlackKingLatePositionTable { get; }
    
    public PositionTables() {        
        AllEarlyPositionTables = new int[Board.NumPieces * Enum.GetValues(typeof(Team)).Length][];
		AllEarlyPositionTables[(int)BitboardIndexes.PawnIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackPawnEarlyPositionTable);
		AllEarlyPositionTables[(int)BitboardIndexes.KnightIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackKnightEarlyPositionTable);
		AllEarlyPositionTables[(int)BitboardIndexes.BishopIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackBishopEarlyPositionTable);
		AllEarlyPositionTables[(int)BitboardIndexes.RookIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackRookEarlyPositionTable);
		AllEarlyPositionTables[(int)BitboardIndexes.QueenIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackQueenEarlyPositionTable);
		AllEarlyPositionTables[(int)BitboardIndexes.KingIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackKingEarlyPositionTable);
		
		AllEarlyPositionTables[(int)BitboardIndexes.PawnIndex * ((int)Team.Black + 1)] = BlackPawnEarlyPositionTable;
		AllEarlyPositionTables[(int)BitboardIndexes.KnightIndex * ((int)Team.Black + 1)] = BlackKnightEarlyPositionTable;
		AllEarlyPositionTables[(int)BitboardIndexes.BishopIndex * ((int)Team.Black + 1)] = BlackBishopEarlyPositionTable;
		AllEarlyPositionTables[(int)BitboardIndexes.RookIndex * ((int)Team.Black + 1)] = BlackRookEarlyPositionTable;
		AllEarlyPositionTables[(int)BitboardIndexes.QueenIndex * ((int)Team.Black + 1)] = BlackQueenEarlyPositionTable;
		AllEarlyPositionTables[(int)BitboardIndexes.KingIndex * ((int)Team.Black + 1)] = BlackKingEarlyPositionTable;

        
        AllLatePositionTables = new int[Board.NumPieces * Enum.GetValues(typeof(Team)).Length][];
		AllLatePositionTables[(int)BitboardIndexes.PawnIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackPawnLatePositionTable);
		AllLatePositionTables[(int)BitboardIndexes.KnightIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackKnightLatePositionTable);
		AllLatePositionTables[(int)BitboardIndexes.BishopIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackBishopLatePositionTable);
		AllLatePositionTables[(int)BitboardIndexes.RookIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackRookLatePositionTable);
		AllLatePositionTables[(int)BitboardIndexes.QueenIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackQueenLatePositionTable);
		AllLatePositionTables[(int)BitboardIndexes.KingIndex * ((int)Team.White + 1)] = GetFlippedArray(BlackKingLatePositionTable);
		
		AllLatePositionTables[(int)BitboardIndexes.PawnIndex * ((int)Team.Black + 1)] = BlackPawnLatePositionTable;
		AllLatePositionTables[(int)BitboardIndexes.KnightIndex * ((int)Team.Black + 1)] = BlackKnightLatePositionTable;
		AllLatePositionTables[(int)BitboardIndexes.BishopIndex * ((int)Team.Black + 1)] = BlackBishopLatePositionTable;
		AllLatePositionTables[(int)BitboardIndexes.RookIndex * ((int)Team.Black + 1)] = BlackRookLatePositionTable;
		AllLatePositionTables[(int)BitboardIndexes.QueenIndex * ((int)Team.Black + 1)] = BlackQueenLatePositionTable;
		AllLatePositionTables[(int)BitboardIndexes.KingIndex * ((int)Team.Black + 1)] = BlackKingLatePositionTable;
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
    
    private static int[] GetFlippedArray(int[] arr) {
        int[] flippedArr = new int[arr.Length];
        for (int i = 0; i < arr.Length; i++) {
            flippedArr[i] = arr[flippedTable[i]];
        }
        return flippedArr;
    }
    
    public int GetPieceTableValue(int phase, BitboardIndexes pieceIndex, Team pieceTeam, int squareIndex) {
        if (phase == 0) {
            return AllEarlyPositionTables[(int)pieceIndex * ((int)pieceTeam + 1)][squareIndex];
        }
        else if (phase == 1) {
            return AllLatePositionTables[(int)pieceIndex * ((int)pieceTeam + 1)][squareIndex];
        }
        return 0;
    }
}