namespace Chess_Application;

public class Evaluation_V6_Advanced : IEvaluation {
    public int pawnValue { get; set; } = 100;
    public int knightValue { get; set; } = 325;
    public int bishopValue { get; set; } = 340;
    public int rookValue { get; set; } = 500;
    public int queenValue { get; set; } = 900;
    
    private const ulong fileMask = 0b0000000100000001000000010000000100000001000000010000000100000001;
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
    
    IPositionTables positionTables;
    
    public Evaluation_V6_Advanced(IPositionTables positionTables) {
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
        
        // go through every rank / file
        for (int i = 0; i < Board.dimensions; i++) {            
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
    
    private int GetPositionValues(ulong bitboard, int[] positionValues) {
        int score = 0;
        while (bitboard != 0) {
            int squareIndex = BitboardHelper.PopLeastSignificantBit(ref bitboard);
            score += positionValues[squareIndex];
        }
        return score;
    }
    
    public int GetPawnScore(Board board, ulong teamPawnBitboard, ulong opponentBitboardWithoutPawns, ulong opponentPawnBitboard, int numPawnsOnFile, int fileIndex) {
        int score = 0;
        // doubled pawns
        if (numPawnsOnFile > 1) {
            int stackedPawnPenalty = 7;
            // the more pawns on the same file, the more penalty
            score -= stackedPawnPenalty * numPawnsOnFile;
        }

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
        if (fileIndex < Board.dimensions - 1) {
            ulong fileMaskOnRight = fileMasks[fileIndex + 1];
            hasPawnOnRightFile = BitboardHelper.BitboardContainsAnyFromMask(teamPawnBitboard, fileMaskOnRight);
            surroundingFilesMask |= fileMaskOnRight;
        }

        // subtract the score if the pawn is isolated
        if (!hasPawnOnLeftFile && !hasPawnOnRightFile) {
            int pawnIsolationPenalty = 2;
            score -= pawnIsolationPenalty;
        }
        
        ulong teamPawnsInFileMask = teamPawnBitboard & fileMask;
        while (teamPawnsInFileMask != 0) {
            int pawnSquareIndex = BitboardHelper.PopLeastSignificantBit(ref teamPawnsInFileMask);
            if (board.pieces[pawnSquareIndex] is not Pawn pawn) {
                // if everything runs correctly, then this should never get executed
                Console.WriteLine("Why is there a different piece in the pawn bitboard?");
                Console.WriteLine("This is being printed from Evaluation_V6_Evaluation on line 148!");
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
            int passedPawnRewardPerRank = 10;
            int totalRewardForPassedPawn = 0;
            int hostilePieceBlockingPassedPawnPenalty = 5;
            int totalPenaltyForHostileBlocker = 0;
            if (direction == Pawn.MovementDirection.MovingUpwards) {
                int rankToShiftTo = pawnIsEnPassantPawn ? pawnCoord.y : pawnCoord.y + 1;
                surroundingFilesMask <<= rankToShiftTo * Board.dimensions;

                // if the pawn is moving upwards and is on a higher rank, the higher the reward is
                totalRewardForPassedPawn = passedPawnRewardPerRank * (pawnCoord.y + 1);
                
                // if the pawn is moving upwards and is on a higher rank, the more valuable the passed pawn is
                totalPenaltyForHostileBlocker = hostilePieceBlockingPassedPawnPenalty * (pawnCoord.y + 1);
            }
            else if (direction == Pawn.MovementDirection.MovingDownwards) {
                int rankToShiftTo = pawnIsEnPassantPawn ? Board.dimensions - (pawnCoord.y + 1) : Board.dimensions - pawnCoord.y;
                surroundingFilesMask <<= rankToShiftTo * Board.dimensions;
                
                // if the pawn is moving downwards and is on a lower rank, the higher the reward is
                totalRewardForPassedPawn = passedPawnRewardPerRank * (7 - pawnCoord.y + 1);
                
                // if the pawn is moving downwards and is on a lower rank, the more valuable the passed pawn is
                totalPenaltyForHostileBlocker = hostilePieceBlockingPassedPawnPenalty * (7 - pawnCoord.y + 1);
            }
            // checking whether there are any opponent pawns in the mask since that would mean the pawn isn't a passed pawn
            if (BitboardHelper.BitboardContainsAnyFromMask(surroundingFilesMask, opponentPawnBitboard)) {
                continue;
            }
            // no opponent pawns in the surrounding file mask so can give the position a reward
            score += totalRewardForPassedPawn;

            // checking if there's any hostile pieces infront of a passed pawn
            ulong fileBitboardInfrontOfPawn = surroundingFilesMask & fileMask;
            bool otherOpponentPiecesOnFile = BitboardHelper.BitboardContainsAnyFromMask(fileBitboardInfrontOfPawn, opponentBitboardWithoutPawns);
            // this section might make the engine think that simply putting a teams piece infront of the pawn is more beneficial
            // so maybe change this to check whether the pawn is protected? I'm not sure about this yet though
            if (otherOpponentPiecesOnFile) {
                score -= totalPenaltyForHostileBlocker;
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
            Coordinate bishopCoord = Board.ConvertSquareIndexToCoord(bishopSquareIndex);
            if (bishopCoord.x - 1 >= 0 && bishopCoord.y - 1 >= 0) {
                diagonalSquareMask |= 1ul << (bishopSquareIndex + CompassDirections.BottomLeft);
            }
            if (bishopCoord.x + 1 < Board.dimensions && bishopCoord.y - 1 >= 0) {
                diagonalSquareMask |= 1ul << (bishopSquareIndex + CompassDirections.BottomRight);
            }
            if (bishopCoord.x - 1 >= 0 && bishopCoord.y + 1 < Board.dimensions) {
                diagonalSquareMask |= 1ul << (bishopSquareIndex + CompassDirections.TopLeft);
            }
            if (bishopCoord.x + 1 < Board.dimensions && bishopCoord.y + 1 < Board.dimensions) {
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