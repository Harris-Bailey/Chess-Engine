namespace Chess;

public class Evaluation_V4_PositionTables : IEvaluation {
    public int pawnValue { get; set; } = 100;
    public int knightValue { get; set; } = 325;
    public int bishopValue { get; set; } = 340;
    public int rookValue { get; set; } = 500;
    public int queenValue { get; set; } = 900;
    
    bool printBoard = true;
    PositionTables positionTables;
    
    public Evaluation_V4_PositionTables(PositionTables positionTables) {
        this.positionTables = positionTables;
    }

    public int Evaluate(Board board, Team team, Team opposingTeam) {
        int score = 0;
        ulong teamPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, team);
        ulong teamBishopBitboard = board.GetPieceBitboard(BitboardIndexes.BishopIndex, team);
        ulong teamKnightBitboard = board.GetPieceBitboard(BitboardIndexes.KnightIndex, team);
        ulong teamRookBitboard = board.GetPieceBitboard(BitboardIndexes.RookIndex, team);
        ulong teamQueenBitboard = board.GetPieceBitboard(BitboardIndexes.QueenIndex, team);
        ulong teamKingBitboard = board.GetPieceBitboard(BitboardIndexes.KingIndex, team);

        score += BitboardHelper.GetPieceCount(teamPawnBitboard) * pawnValue;
        score += BitboardHelper.GetPieceCount(teamBishopBitboard) * bishopValue;
        score += BitboardHelper.GetPieceCount(teamKnightBitboard) * knightValue;
        score += BitboardHelper.GetPieceCount(teamRookBitboard) * rookValue;
        score += BitboardHelper.GetPieceCount(teamQueenBitboard) * queenValue;
        
        score += GetPositionValues(teamPawnBitboard, BitboardIndexes.PawnIndex, team);
        score += GetPositionValues(teamBishopBitboard, BitboardIndexes.BishopIndex, team);
        score += GetPositionValues(teamKnightBitboard, BitboardIndexes.KnightIndex, team);
        score += GetPositionValues(teamRookBitboard, BitboardIndexes.RookIndex, team);
        score += GetPositionValues(teamQueenBitboard, BitboardIndexes.QueenIndex, team);
        score += GetPositionValues(teamKingBitboard, BitboardIndexes.KingIndex, team);

        return score;
    }

    private int GetPositionValues(ulong bitboard, BitboardIndexes pieceIndex, Team team) {
        int score = 0;
        while (bitboard != 0) {
            int squareIndex = BitboardHelper.PopLeastSignificantBit(ref bitboard);
            score += positionTables.GetPieceTableValue(0, pieceIndex, team, squareIndex);
        }
        return score;
    }
}