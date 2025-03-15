namespace Chess;

public class Evaluation_V3_Pawns : IEvaluation {
    public int pawnValue { get; set; } = 100;
    public int knightValue { get; set; } = 325;
    public int bishopValue { get; set; } = 340;
    public int rookValue { get; set; } = 500;
    public int queenValue { get; set; } = 900;

    const ulong fileMask = 0b0000000100000001000000010000000100000001000000010000000100000001;
    readonly ulong[] fileMasks = {
        fileMask,
        fileMask << 1,
        fileMask << 2,
        fileMask << 3,
        fileMask << 4,
        fileMask << 5,
        fileMask << 6,
        fileMask << 7,
    };

    public int Evaluate(Board board, Team team, Team opposingTeam) {
        
        int score = 0;

        ulong teamPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, team);
        ulong teamBishopBitboard = board.GetPieceBitboard(BitboardIndexes.BishopIndex, team);
        ulong teamKnightBitboard = board.GetPieceBitboard(BitboardIndexes.KnightIndex, team);
        ulong teamRookBitboard = board.GetPieceBitboard(BitboardIndexes.RookIndex, team);
        ulong teamQueenBitboard = board.GetPieceBitboard(BitboardIndexes.QueenIndex, team);
        // ulong teamsKingBitboard = board.GetPieceBitboard(typeof(King), team);

        ulong opponentPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, opposingTeam);
        ulong opponentBishopBitboard = board.GetPieceBitboard(BitboardIndexes.BishopIndex, opposingTeam);
        ulong opponentKnightBitboard = board.GetPieceBitboard(BitboardIndexes.KnightIndex, opposingTeam);
        ulong opponentRookBitboard = board.GetPieceBitboard(BitboardIndexes.RookIndex, opposingTeam);
        ulong opponentQueenBitboard = board.GetPieceBitboard(BitboardIndexes.QueenIndex, opposingTeam);

        score += BitboardHelper.GetPieceCount(teamPawnBitboard) * pawnValue;
        score += BitboardHelper.GetPieceCount(teamBishopBitboard) * bishopValue;
        score += BitboardHelper.GetPieceCount(teamKnightBitboard) * knightValue;
        score += BitboardHelper.GetPieceCount(teamRookBitboard) * rookValue;
        score += BitboardHelper.GetPieceCount(teamQueenBitboard) * queenValue;

        // ulong teamPawnBitboard = board.GetPieceBitboard<Pawn>(team);
        // ulong opposingPawnBitboard = board.GetPieceBitboard<Pawn>(opposingTeam);

        // ulong teamRookBitboard = board.GetPieceBitboard<Rook>(team);

        
        ulong rankMask = 0b11111111;
        for (int i = 0; i < Board.dimensions; i++) {
            ulong shiftedRankMask = rankMask << (i * Board.dimensions);
            // ulong shiftedFileMask = fileMask << i;
            
            int numPawnsOnFile = BitboardHelper.GetPieceCount(teamPawnBitboard, fileMasks[i]);
            if (numPawnsOnFile > 0) {
                score += GetPawnScore(board, teamPawnBitboard, opponentPawnBitboard, numPawnsOnFile, i);
            }

            // handle reward for rooks being on the same rank
            int numRooksOnSameFile = BitboardHelper.GetPieceCount(teamRookBitboard, fileMasks[i]);
            if (numRooksOnSameFile >= 2) {
                int sameRankRooksReward = 15;
                score += sameRankRooksReward;

                // add more reward if there are no pawns on the file
                if (numPawnsOnFile == 0) {
                    int rooksOnFileWithNoPawnsReward = 10;
                    score += rooksOnFileWithNoPawnsReward;
                }
            }
        }

        int bishopCount = BitboardHelper.GetPieceCount(teamBishopBitboard);
        if (bishopCount >= 2)
            score += 50;

        if (board.GetTeamsKing(opposingTeam).isChecked) {
            score += 10;
        }

        return score;
    }

    private int GetPawnScore(Board board, ulong teamPawnBitboard, ulong opponentPawnBitboard, int numPawnsOnFile, int fileIndex) {
        int score = 0;
        // doubled pawns
        if (numPawnsOnFile > 1) {
            int stackedPawnPenalty = 7;
            // the more pawns on the same file, the more penalty
            score -= stackedPawnPenalty * numPawnsOnFile;
        }

        bool hasProtectivePawns = false;
        if (fileIndex > 0) {
            bool pawnIsOnLeftFile = BitboardHelper.BitboardContainsAnyFromMask(teamPawnBitboard, fileMasks[fileIndex - 1]);
            if (pawnIsOnLeftFile)
                hasProtectivePawns = true;
        }
        if (fileIndex < Board.dimensions - 1) {
            bool pawnIsOnRightFile = BitboardHelper.BitboardContainsAnyFromMask(teamPawnBitboard, fileMasks[fileIndex + 1]);
            if (pawnIsOnRightFile)
                hasProtectivePawns = true;
        }

        // subtract the score if the pawn is isolated
        if (!hasProtectivePawns) {
            int pawnIsolationPenalty = 2;
            score -= pawnIsolationPenalty;
        }

        // the surrounding files of the current file, including the current file
        // for example the file may be file C and then the mask will include file B, C, and D
        ulong surroundingFilesMask = fileMask;
        // adding the left file to the mask if it's inside the bounds
        if (fileIndex > 0) {
            surroundingFilesMask |= fileMask << -1;
        }
        // adding the right file to the mask if it's inside the bounds
        if (fileIndex < Board.dimensions) {
            surroundingFilesMask |= fileMask << 1;
        }
        int[] pawnSquareIndexes = BitboardHelper.GetSquareIndexesFromBitboard(teamPawnBitboard & fileMask);
        foreach (int pawnSquareIndex in pawnSquareIndexes) {
            if (board.pieces[pawnSquareIndex] is not Pawn pawn) {
                // if everything runs correctly, then this should never get executed
                Console.WriteLine("Why is there a different piece in the pawn bitboard?");
                Environment.Exit(0);
                return 0;
            }
            Pawn.MovementDirection direction = pawn.direction;
            Coordinate pawnCoord = Board.ConvertSquareIndexToCoord(pawnSquareIndex);
            // this section offsets the file mask so that instead of it being the whole file, it just keeps the squares infront of the pawn
            // if it's the en passant pawn though, then we have to include the rank of the pawn because it can be captured on that rank
            // e.g. assuming this is an upwards moving pawn:
            /*  surrounding file mask       pawn position       resulting file mask     resulting file mask for en passant pawn
                        1 1 1                   0 0 0               1 1 1                           1 1 1
                        1 1 1                   0 0 0               1 1 1                           1 1 1
                        1 1 1                   0 1 0               0 0 0                           1 1 1
                        1 1 1                   0 0 0               0 0 0                           0 0 0
                        1 1 1                   0 0 0               0 0 0                           0 0 0
                        1 1 1                   0 0 0               0 0 0                           0 0 0
                        1 1 1                   0 0 0               0 0 0                           0 0 0
                        1 1 1                   0 0 0               0 0 0                           0 0 0
            */
            // and for black it'd just select the squares below the pawn
            bool pawnIsEnPassantPawn = pawn == board.CurrentEnPassantPawn;
            if (direction == Pawn.MovementDirection.MovingUpwards) {
                int rankToShiftTo = pawnIsEnPassantPawn ? pawnCoord.y : pawnCoord.y + 1;
                surroundingFilesMask <<= rankToShiftTo * Board.dimensions;
            }
            else if (direction == Pawn.MovementDirection.MovingDownwards) {
                int rankToShiftTo = pawnIsEnPassantPawn ? Board.dimensions - (pawnCoord.y + 1) : Board.dimensions - pawnCoord.y;
                surroundingFilesMask >>= rankToShiftTo * Board.dimensions;
            }
            bool opponentPawnsInMask = (opponentPawnBitboard & surroundingFilesMask) != 0;
            if (!opponentPawnsInMask) {
                // no opponent pawns on these squares so can give the evaluation a boost
                int passedPawnRewardPerRank = 10;
                if (direction == Pawn.MovementDirection.MovingUpwards) {
                    // if the pawn is moving upwards and is on a higher rank, the higher the reward is
                    score += passedPawnRewardPerRank * (pawnCoord.y + 1);
                }
                else if (direction == Pawn.MovementDirection.MovingDownwards) {
                    // if the pawn is moving downwards and is on a lower rank, the higher the reward is
                    score += (passedPawnRewardPerRank * Board.dimensions) - passedPawnRewardPerRank * (pawnCoord.y + 1);
                }
            }
        }
        return score;
    }
}
