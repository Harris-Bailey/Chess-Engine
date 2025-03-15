namespace Chess;

public class ChessProgrammingPieceTables : PositionTables {
    
    // pawn positional score
    public override sealed int[] BlackPawnPositionTable { get; protected set; } = {
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
    public override sealed int[] BlackKnightPositionTable { get; protected set; } = {
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
    public override sealed int[] BlackBishopPositionTable { get; protected set; } = {
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
    public override sealed int[] BlackRookPositionTable { get; protected set; } = {
        50,  50,  50,  50,  50,  50,  50,  50,
        50,  50,  50,  50,  50,  50,  50,  50,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,  10,  20,  20,  10,   0,   0,
        0,   0,   0,  20,  20,   0,   0,   0
    
    };
    
    public override sealed int[] BlackQueenPositionTable { get; protected set; } = {
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
    public override sealed int[] BlackKingPositionTable { get; protected set; } = {
        0,   0,   0,   0,   0,   0,   0,   0,
        0,   0,   5,   5,   5,   5,   0,   0,
        0,   5,   5,  10,  10,   5,   5,   0,
        0,   5,  10,  20,  20,  10,   5,   0,
        0,   5,  10,  20,  20,  10,   5,   0,
        0,   0,   5,  10,  10,   5,   0,   0,
        0,   5,   5,  -5,  -5,   0,   5,   0,
        0,   0,   5,   0, -15,   0,  10,   0
    };
}