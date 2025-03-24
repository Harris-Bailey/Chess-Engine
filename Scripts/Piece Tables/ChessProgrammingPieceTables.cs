namespace Chess;

public class ChessProgrammingPieceTables : PositionTables {
    
    // pawn positional score
    protected override sealed int[] BlackPawnEarlyPositionTable { get; } = {
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
    protected override sealed int[] BlackKnightEarlyPositionTable { get; } = {
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
    protected override sealed int[] BlackBishopEarlyPositionTable { get; } = {
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
    protected override sealed int[] BlackRookEarlyPositionTable { get; } = {
        50,  50,  50,  50,  50,  50,  50,  50,
        50,  50,  50,  50,  50,  50,  50,  50,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,   0,  20,  20,   0,   0,   0
    
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
    
    // king positional score
    protected override sealed int[] BlackKingEarlyPositionTable { get; } = {
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   5,   5,   5,   5,   0,   0,
        0,   5,   5,  10,  10,   5,   5,   0,
        0,   5,  10,  20,  20,  10,   5,   0,
        0,   5,  10,  20,  20,  10,   5,   0,
        0,   0,   5,  10,  10,   5,   0,   0,
        0,   5,   5,  -5,  -5,   0,   5,   0,
        0,   0,   5,   0, -15,   0,  10,   0
    };
    protected override int[] BlackPawnLatePositionTable => BlackPawnEarlyPositionTable;
    protected override int[] BlackKnightLatePositionTable => BlackBishopEarlyPositionTable;
    protected override int[] BlackBishopLatePositionTable => BlackKnightEarlyPositionTable;
    protected override int[] BlackRookLatePositionTable => BlackRookEarlyPositionTable;
    protected override int[] BlackQueenLatePositionTable => BlackQueenEarlyPositionTable;
    protected override int[] BlackKingLatePositionTable => BlackKingEarlyPositionTable;
}