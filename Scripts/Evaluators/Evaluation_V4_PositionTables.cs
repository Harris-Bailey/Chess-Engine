namespace Chess_Application;

public class Evaluation_V4_PositionTables : IEvaluation {
    public int pawnValue { get; set; } = 100;
    public int knightValue { get; set; } = 325;
    public int bishopValue { get; set; } = 340;
    public int rookValue { get; set; } = 500;
    public int queenValue { get; set; } = 900;
    
    bool printBoard = true;
    IPositionTables positionTables;
    
    public Evaluation_V4_PositionTables(IPositionTables positionTables) {
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

        int[] pawnPositions = BitboardHelper.GetSquareIndexesFromBitboard(teamPawnBitboard);
        int[] knightPositions = BitboardHelper.GetSquareIndexesFromBitboard(teamKnightBitboard);
        int[] bishopPositions = BitboardHelper.GetSquareIndexesFromBitboard(teamBishopBitboard);
        int[] rookPositions = BitboardHelper.GetSquareIndexesFromBitboard(teamRookBitboard);
        int[] queenPositions = BitboardHelper.GetSquareIndexesFromBitboard(teamQueenBitboard);
        int[] kingPositions = BitboardHelper.GetSquareIndexesFromBitboard(teamKingBitboard);
        
        if (team == Team.White) {
            score += GetPositionValues(pawnPositions, positionTables.WhitePawnPositionTable);
            score += GetPositionValues(bishopPositions, positionTables.WhiteBishopPositionTable);
            score += GetPositionValues(knightPositions, positionTables.WhiteKnightPositionTable);
            score += GetPositionValues(rookPositions, positionTables.WhiteRookPositionTable);
            score += GetPositionValues(queenPositions, positionTables.WhiteQueenPositionTable);
            score += GetPositionValues(kingPositions, positionTables.WhiteKingPositionTable);
        }
        else if (team == Team.Black) {
            score += GetPositionValues(pawnPositions, positionTables.BlackPawnPositionTable);
            score += GetPositionValues(bishopPositions, positionTables.BlackBishopPositionTable);
            score += GetPositionValues(knightPositions, positionTables.BlackKnightPositionTable);
            score += GetPositionValues(rookPositions, positionTables.BlackRookPositionTable);
            score += GetPositionValues(queenPositions, positionTables.BlackQueenPositionTable);
            score += GetPositionValues(kingPositions, positionTables.BlackKingPositionTable);
        }
        
        // if (printBoard) {
        //     board.PrintBoard();
        //     Console.WriteLine($"Current team: {team}");
        //     Console.WriteLine($"This position has a score of {score}");
        //     printBoard = false;
        // }

        return score;
    }

    private int GetPositionValues(int[] piecePositions, int[] positionValues) {
        int score = 0;
        foreach (int positionIndex in piecePositions) {
            score += positionValues[positionIndex];
        }
        return score;
    }
}