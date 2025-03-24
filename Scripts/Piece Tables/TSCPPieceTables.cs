namespace Chess;

public class TSCPPieceTables : PositionTables {
    
    protected override sealed int[] BlackPawnEarlyPositionTable { get; } = {
    	  0,   0,   0,   0,   0,   0,   0,   0,
    	  5,  10,  15,  20,  20,  15,  10,   5,
    	  4,   8,  12,  16,  16,  12,   8,   4,
    	  3,   6,   9,  12,  12,   9,   6,   3,
    	  2,   4,   6,   8,   8,   6,   4,   2,
    	  1,   2,   3, -10, -10,   3,   2,   1,
    	  0,   0,   0, -40, -40,   0,   0,   0,
    	  0,   0,   0,   0,   0,   0,   0,   0
    };
    
    protected override sealed int[] BlackKnightEarlyPositionTable { get; } = {
    	-10, -10, -10, -10, -10, -10, -10, -10,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10,   0,   5,   5,   5,   5,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   0,   5,   5,   5,   5,   0, -10,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10, -30, -10, -10, -10, -10, -30, -10
    };
    
    protected override sealed int[] BlackBishopEarlyPositionTable { get; } = {
    	-10, -10, -10, -10, -10, -10, -10, -10,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10,   0,   5,   5,   5,   5,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   0,   5,   5,   5,   5,   0, -10,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10, -10, -20, -10, -10, -20, -10, -10
    };
    
    protected override sealed int[] BlackRookEarlyPositionTable { get; } = {
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
    };
    
    protected override sealed int[] BlackQueenEarlyPositionTable { get; } = {
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
    };
    
    protected override sealed int[] BlackKingEarlyPositionTable { get; } = {
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-40, -40, -40, -40, -40, -40, -40, -40,
    	-20, -20, -20, -20, -20, -20, -20, -20,
    	  0,  20,  40, -20,   0, -20,  40,  20
    };
	
	protected override int[] BlackPawnLatePositionTable => BlackPawnEarlyPositionTable;
    protected override int[] BlackKnightLatePositionTable => BlackBishopEarlyPositionTable;
    protected override int[] BlackBishopLatePositionTable => BlackKnightEarlyPositionTable;
    protected override int[] BlackRookLatePositionTable => BlackRookEarlyPositionTable;
    protected override int[] BlackQueenLatePositionTable => BlackQueenEarlyPositionTable;
    protected override int[] BlackKingLatePositionTable => BlackKingEarlyPositionTable;
}