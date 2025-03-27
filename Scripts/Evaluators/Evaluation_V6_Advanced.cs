namespace Chess;

public class Evaluation_V6_Advanced : IEvaluation {
    public int halfPawnValue = 50;
    public int pawnValue { get; set; } = 100;
    public int knightValue { get; set; } = 325;
    public int bishopValue { get; set; } = 340;
    public int rookValue { get; set; } = 500;
    public int queenValue { get; set; } = 900;
    
    private const ulong fileMask = BitboardHelper.LeftColumnMask;
    private ulong[] fileMasks = {
        fileMask,
        fileMask << 1,
        fileMask << 2,
        fileMask << 3,
        fileMask << 4,
        fileMask << 5,
        fileMask << 6,
        fileMask << 7,
    };
    
    PositionTables positionTables;
    
    public Evaluation_V6_Advanced(PositionTables positionTables) {
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
        
        ulong opponentsPieces = board.GetTeamBitboard(opposingTeam);
        ulong opponentPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, opposingTeam);
        ulong opponentPiecesWithoutPawns = opponentsPieces ^ opponentPawnBitboard;
        
        int numTeamPawns = BitboardHelper.GetPieceCount(teamPawnBitboard);
        int numTeamBishops = BitboardHelper.GetPieceCount(teamBishopBitboard);
        int numTeamKnights = BitboardHelper.GetPieceCount(teamKnightBitboard);
        int numTeamRooks = BitboardHelper.GetPieceCount(teamRookBitboard);
        int numTeamQueens = BitboardHelper.GetPieceCount(teamQueenBitboard);
        
        score += numTeamPawns * pawnValue;
        score += numTeamBishops * bishopValue;
        score += numTeamKnights * knightValue;
        score += numTeamRooks * rookValue;
        score += numTeamQueens * queenValue;
        
        // need to include a transition from early game to endgame
        score += GetPositionValues(teamPawnBitboard, BitboardIndexes.PawnIndex, team);
        score += GetPositionValues(teamBishopBitboard, BitboardIndexes.BishopIndex, team);
        score += GetPositionValues(teamKnightBitboard, BitboardIndexes.KnightIndex, team);
        score += GetPositionValues(teamRookBitboard, BitboardIndexes.RookIndex, team);
        score += GetPositionValues(teamQueenBitboard, BitboardIndexes.QueenIndex, team);
        score += GetPositionValues(teamKingBitboard, BitboardIndexes.KingIndex, team);
        
        // go through every rank / file
        for (int i = 0; i < Board.Dimensions; i++) {            
            int numPawnsOnFile = BitboardHelper.GetPieceCount(teamPawnBitboard, fileMasks[i]);
            if (numPawnsOnFile > 0) {
                score += GetPawnScore(board, teamPawnBitboard, opponentPiecesWithoutPawns, opponentPawnBitboard, numPawnsOnFile, i);
            }

            // handle reward for rooks being on the same rank
            int numRooksOnFile = BitboardHelper.GetPieceCount(teamRookBitboard, fileMasks[i]);
            if (numRooksOnFile > 0) {
                score += GetRookScore(numRooksOnFile, numPawnsOnFile);
            }
        }

        score += GetBishopScore(teamBishopBitboard, teamPawnBitboard | opponentPawnBitboard, numTeamBishops);

        if (board.GetTeamsKing(opposingTeam).isChecked) {
            score += 10;
        }
        
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
    
    public int GetPawnScore(Board board, ulong teamPawnBitboard, ulong opponentBitboardWithoutPawns, ulong opponentPawnBitboard, int numPawnsOnFile, int fileIndex) {
        int score = 0;

        // the surrounding files of the current file, including the current file
        // for example the file may be file C and then the mask will include file B, C, and D
        ulong surroundingFilesMask = fileMask;
        bool hasPawnOnLeftFile = false;
        bool hasPawnOnRightFile = false;
        if (fileIndex > 0) {
            ulong fileMaskOnLeft = fileMasks[fileIndex - 1];
            hasPawnOnLeftFile = BitboardHelper.BitboardContainsAnyFromMask(teamPawnBitboard, fileMaskOnLeft);
            surroundingFilesMask |= fileMaskOnLeft;
        }
        if (fileIndex < Board.Dimensions - 1) {
            ulong fileMaskOnRight = fileMasks[fileIndex + 1];
            hasPawnOnRightFile = BitboardHelper.BitboardContainsAnyFromMask(teamPawnBitboard, fileMaskOnRight);
            surroundingFilesMask |= fileMaskOnRight;
        }

        // subtract the score if the pawn is isolated
        if (!hasPawnOnLeftFile && !hasPawnOnRightFile) {
            int pawnIsolationPenalty = halfPawnValue;
            score -= pawnIsolationPenalty;
        }
        
        // doubled pawns
        if (numPawnsOnFile > 1) {
            int stackedPawnPenalty = 7;
            // the more pawns on the same file, the more penalty
            score -= stackedPawnPenalty * numPawnsOnFile;
        }
        // I don't know if this is right
        // maybe I should just class it as one passed pawn if it's doubled?
        else if (numPawnsOnFile == 1) {
            int pawnSquareIndex = BitboardHelper.GetLeastSignificantBit(teamPawnBitboard & fileMasks[fileIndex]);
            // Console.WriteLine(pawnSquareIndex);
            if (board.Pieces[pawnSquareIndex] is not Pawn pawn) {
                // if everything runs correctly, then this should never get executed
                Console.WriteLine("Why is there a different piece in the pawn bitboard?");
                Console.WriteLine("This is being printed from Evaluation_V6_Evaluation");
                Environment.Exit(0);
                return 0;
            }
            
            Pawn.MovementDirection direction = pawn.direction;
            Coordinate pawnCoord = new Coordinate(pawnSquareIndex);
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
            
            int passedPawnRankReward = 10;
            int hostileBlockerPenalty = 5;
            
            int totalRewardForPassedPawn = 0;
            int totalPenaltyForHostileBlocker = 0;
            
            if (direction == Pawn.MovementDirection.MovingUpwards) {
                int rankToShiftTo = pawnIsEnPassantPawn ? pawnCoord.y : pawnCoord.y + 1;
                surroundingFilesMask <<= rankToShiftTo * Board.Dimensions;

                // if the pawn is moving upwards and is on a higher rank, the higher the reward is
                totalRewardForPassedPawn = passedPawnRankReward * (pawnCoord.y + 1);
                
                // if the pawn is moving upwards and is on a higher rank, the more valuable the passed pawn is
                totalPenaltyForHostileBlocker = hostileBlockerPenalty * (pawnCoord.y + 1);
            }
            else if (direction == Pawn.MovementDirection.MovingDownwards) {
                int rankToShiftTo = pawnIsEnPassantPawn ? Board.Dimensions - (pawnCoord.y + 1) : Board.Dimensions - pawnCoord.y;
                surroundingFilesMask >>= rankToShiftTo * Board.Dimensions;
                
                // if the pawn is moving downwards and is on a lower rank, the higher the reward is
                totalRewardForPassedPawn = passedPawnRankReward * (7 - (pawnCoord.y + 1));
                
                // if the pawn is moving downwards and is on a lower rank, the more valuable the passed pawn is
                totalPenaltyForHostileBlocker = hostileBlockerPenalty * (7 - (pawnCoord.y + 1));
            }
            
            // checking whether there are any opponent pawns in the mask since that would mean the pawn isn't a passed pawn
            if (BitboardHelper.BitboardContainsAnyFromMask(surroundingFilesMask, opponentPawnBitboard)) {
                // no opponent pawns in the surrounding file mask so can give the position a reward
                score += totalRewardForPassedPawn;

                // checking if there's any hostile pieces infront of a passed pawn
                ulong fileBitboardInfrontOfPawn = surroundingFilesMask & fileMasks[fileIndex];
                bool otherOpponentPiecesOnFile = (fileBitboardInfrontOfPawn & opponentBitboardWithoutPawns) != 0;

                if (otherOpponentPiecesOnFile) {
                    score -= totalPenaltyForHostileBlocker;
                }
            }
        }
        
        return score;
    }
    
    private int GetBishopScore(ulong teamBishopBitboard, ulong allPawnsBitboard, int bishopCount) {
        int score = 0;
        score += bishopCount >= 2 ? 50 : 0;
        
        int pawnOnAdjacentSquarePenalty = 20;
        ulong diagonalSquareMask = 0;
        ulong bishopBitboardCopy = teamBishopBitboard;
        while (bishopBitboardCopy != 0) {
            int bishopSquareIndex = BitboardHelper.PopLeastSignificantBit(ref bishopBitboardCopy);
            Coordinate bishopCoord = new Coordinate(bishopSquareIndex);
            if (bishopCoord.x - 1 >= 0 && bishopCoord.y - 1 >= 0) {
                diagonalSquareMask |= 1ul << (bishopSquareIndex + CompassDirections.BottomLeft);
            }
            if (bishopCoord.x + 1 < Board.Dimensions && bishopCoord.y - 1 >= 0) {
                diagonalSquareMask |= 1ul << (bishopSquareIndex + CompassDirections.BottomRight);
            }
            if (bishopCoord.x - 1 >= 0 && bishopCoord.y + 1 < Board.Dimensions) {
                diagonalSquareMask |= 1ul << (bishopSquareIndex + CompassDirections.TopLeft);
            }
            if (bishopCoord.x + 1 < Board.Dimensions && bishopCoord.y + 1 < Board.Dimensions) {
                diagonalSquareMask |= 1ul << (bishopSquareIndex + CompassDirections.TopRight);
            }
            
            if (BitboardHelper.BitboardContainsAnyFromMask(allPawnsBitboard, diagonalSquareMask)) {
                score -= pawnOnAdjacentSquarePenalty;
            }
        }
        return score;
    }
    
    private int GetRookScore(int numRooksOnFile, int numPawnsOnFile) {
        int score = 0;
                
        // if there are no pawns on the file that the rook's on then add a reward
        if (numPawnsOnFile == 0) {
            int rooksOnFileWithNoPawnsReward = 10;
            score += rooksOnFileWithNoPawnsReward;
        }
        // add a reward for two rooks on the same file
        if (numRooksOnFile >= 2) {
            int sameFileRooksReward = 15;
            score += sameFileRooksReward;
        }        
        
        return score;
    }
}