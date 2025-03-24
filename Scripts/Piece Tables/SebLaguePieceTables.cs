namespace Chess;

public class SebLaguePieceTables : PositionTables {

    protected override sealed int[] BlackPawnEarlyPositionTable { get; } = {
    	 0,   0,   0,   0,   0,   0,   0,   0,
    	50,  50,  50,  50,  50,  50,  50,  50,
    	10,  10,  20,  30,  30,  20,  10,  10,
    	 5,   5,  10,  25,  25,  10,   5,   5,
    	 0,   0,   0,  20,  20,   0,   0,   0,
    	 5,  -5, -10,   0,   0, -10,  -5,   5,
    	 5,  10,  10, -20, -20,  10,  10,   5,
    	 0,   0,   0,   0,   0,   0,   0,   0
    };
    
    protected override sealed int[] BlackRookEarlyPositionTable { get; } = {
    	 0,  0,  0,  0,  0,  0,  0,  0,
    	 5, 10, 10, 10, 10, 10, 10,  5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	-5,  0,  0,  0,  0,  0,  0, -5,
    	 0,  0,  0,  5,  5,  0,  0,  0
    };
    
    protected override sealed int[] BlackKnightEarlyPositionTable { get; } = {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20,   0,   0,   0,   0, -20, -40,
        -30,   0,  10,  15,  15,  10,   0, -30,
        -30,   5,  15,  20,  20,  15,   5, -30,
        -30,   0,  15,  20,  20,  15,   0, -30,
        -30,   5,  10,  15,  15,  10,   5, -30,
        -40, -20,   0,   5,   5,   0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50,
    };
    
    protected override sealed int[] BlackBishopEarlyPositionTable { get; } = {
    	-20, -10, -10, -10, -10, -10, -10, -20,
    	-10,   0,   0,   0,   0,   0,   0, -10,
    	-10,   0,   5,  10,  10,   5,   0, -10,
    	-10,   5,   5,  10,  10,   5,   5, -10,
    	-10,   0,  10,  10,  10,  10,   0, -10,
    	-10,  10,  10,  10,  10,  10,  10, -10,
    	-10,   5,   0,   0,   0,   0,   5, -10,
    	-20, -10, -10, -10, -10, -10, -10, -20,
    };
    
    protected override sealed int[] BlackQueenEarlyPositionTable { get; } = {
    	-20, -10, -10, -5, -5, -10, -10, -20,
    	-10,   0,   0,  0,  0,   0,   0, -10,
    	-10,   0,   5,  5,  5,   5,   0, -10,
    	 -5,   0,   5,  5,  5,   5,   0,  -5,
    	  0,   0,   5,  5,  5,   5,   0,  -5,
    	-10,   5,   5,  5,  5,   5,   0, -10,
    	-10,   0,   5,  0,  0,   0,   0, -10,
    	-20, -10, -10, -5, -5, -10, -10, -20
    };
    
    protected override sealed int[] BlackKingEarlyPositionTable { get; } = {
    	-80, -70, -70, -70, -70, -70, -70, -80, 
    	-60, -60, -60, -60, -60, -60, -60, -60, 
    	-40, -50, -50, -60, -60, -50, -50, -40, 
    	-30, -40, -40, -50, -50, -40, -40, -30, 
    	-20, -30, -30, -40, -40, -30, -30, -20, 
    	-10, -20, -20, -20, -20, -20, -20, -10, 
    	 20,  20,  -5,  -5,  -5,  -5,  20,  20, 
    	 20,  30,  10,   0,   0,  10,  30,  20
    };
	
	protected override int[] BlackPawnLatePositionTable => BlackPawnEarlyPositionTable;
    protected override int[] BlackKnightLatePositionTable => BlackBishopEarlyPositionTable;
    protected override int[] BlackBishopLatePositionTable => BlackKnightEarlyPositionTable;
    protected override int[] BlackRookLatePositionTable => BlackRookEarlyPositionTable;
    protected override int[] BlackQueenLatePositionTable => BlackQueenEarlyPositionTable;
    protected override int[] BlackKingLatePositionTable => BlackKingEarlyPositionTable;
}