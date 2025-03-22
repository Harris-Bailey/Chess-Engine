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
        
        if (team == Team.White) {
            score += GetPositionValues(teamPawnBitboard, positionTables.WhitePawnPositionTable);
            score += GetPositionValues(teamBishopBitboard, positionTables.WhiteBishopPositionTable);
            score += GetPositionValues(teamKnightBitboard, positionTables.WhiteKnightPositionTable);
            score += GetPositionValues(teamRookBitboard, positionTables.WhiteRookPositionTable);
            score += GetPositionValues(teamQueenBitboard, positionTables.WhiteQueenPositionTable);
            score += GetPositionValues(teamKingBitboard, positionTables.WhiteKingPositionTable);
        }
        else if (team == Team.Black) {
            score += GetPositionValues(teamPawnBitboard, positionTables.BlackPawnPositionTable);
            score += GetPositionValues(teamBishopBitboard, positionTables.BlackBishopPositionTable);
            score += GetPositionValues(teamKnightBitboard, positionTables.BlackKnightPositionTable);
            score += GetPositionValues(teamRookBitboard, positionTables.BlackRookPositionTable);
            score += GetPositionValues(teamQueenBitboard, positionTables.BlackQueenPositionTable);
            score += GetPositionValues(teamKingBitboard, positionTables.BlackKingPositionTable);
        }

        return score;
    }

    private int GetPositionValues(ulong bitboard, int[] positionValues) {
        int score = 0;
        while (bitboard != 0) {
            int squareIndex = BitboardHelper.PopLeastSignificantBit(ref bitboard);
            score += positionValues[squareIndex];
        }
        return score;
    }
}