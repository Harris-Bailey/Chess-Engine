using BenchmarkDotNet.Attributes;
using Chess;

namespace BenchmarkDiagnoser; 

[MemoryDiagnoser]
[MediumRunJob]
public class Benchmarks {

    [Params(1_000_000)]
    public int n;

    private int squareIndex = 26;
    private Board board;
    private Team team;
    private MoveGenerator moveGenerator;
    King activeTeamsKing;
    ulong activeTeamsKingBitboard;
    IEvaluation evaluation_V2;
    IEvaluation evaluation_V3;
    IEvaluation evaluation_V4;
    IEvaluation evaluation_V5 = new Evaluation_V5_PawnsAndTables(new SebLaguePieceTables());
    Evaluation_V6_Advanced evaluation_V6 = new Evaluation_V6_Advanced(new SebLaguePieceTables());
    ulong teamPawnBitboard;
    ulong opponentPawnBitboard;
    ulong bitboard1 = ulong.MaxValue;
    ulong opponentPiecesWithoutPawns;
    private const ulong fileMask = 0b0000000100000001000000010000000100000001000000010000000100000001;
    ulong blackKingBitboard;
    ulong[] fileMasks = {
        fileMask,
        fileMask << 1,
        fileMask << 2,
        fileMask << 3,
        fileMask << 4,
        fileMask << 5,
        fileMask << 6,
        fileMask << 7,
    };
    Move[] moves;
    private int[] directions = CompassDirections.Cardinals;
    
    public Benchmarks() {
        Setup();
        n = 1_000_000;
    }

    [GlobalSetup]
    public void Setup() {
        board = new Board();
        moveGenerator = new MoveGenerator(board);
        FENHandler handler = new FENHandler(board, moveGenerator);
        string encapsulatedAttacker = "8/1p1p1p1p/1brn4/1rQb3k/1qnn4/8/1P1PP1PP/RN2KB1R b - - 0 2";
        string noBlockers = "8/8/7k/3Q4/8/8/8/4K3 b - - 0 2";
        string pinnedBlocker = "8/8/8/Q5nk/8/8/8/4K3 b - - 0 2";
        string pinnedEnPassant = "8/pppppp1p/8/8/2Q3pk/8/PPPPPPPP/3K4 w - - 0 1 moves f2f4";
        string blockersOnEdge = "3p4/1pp1pp1p/p3q2r/4k3/8/P2Q3P/6P1/1P1K1P2 b - - 0 1";
        string kiwipetePosition = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R b KQkq - ";
        handler.Initialise(FENHandler.FENStartingPosition);
        activeTeamsKing = board.GetTeamsKing(board.currentTeam);
        activeTeamsKingBitboard = 1ul << activeTeamsKing.SquareIndex;
        evaluation_V2 = new Evaluation_V2_Check();
        evaluation_V3 = new Evaluation_V3_Pawns();
        evaluation_V4 = new Evaluation_V4_PositionTables(new SebLaguePieceTables());
        teamPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, Team.White);
        opponentPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, Team.Black);
        opponentPiecesWithoutPawns = board.GetTeamBitboard(Team.Black) ^ opponentPawnBitboard;
        blackKingBitboard = board.GetPieceBitboard(BitboardIndexes.KingIndex, Team.Black);
        
        moves = moveGenerator.UpdateAllPieces();
    }

    // [Benchmark]
    public void IsPieceAt() {
        for (int i = 0; i < 64; i++)
            board.IsPieceAt(i);
    }

    // [Benchmark]
    public void MakeDoublePawnMove() {
        board.MakeMove(new Move(8, 24, Move.SpecialMoveType.PushPawnTwoSquares));
        board.UndoMove();
    }

    // [Benchmark]
    public void MakeCastleMove() {
        board.MakeMove(new Move(4, 6, Move.SpecialMoveType.CastlingKingside));
        board.UndoMove();
    }

    // [Benchmark]
    public void MakeEnPassantMove() {
        board.MakeMove(new Move(31, 23, Move.SpecialMoveType.EnPassantCapture));
        board.UndoMove();
    }

    // [Benchmark]
    public void MakePromotion() {
        board.MakeMove(new Move(51, 56, Move.SpecialMoveType.PromoteToQueen));
        board.UndoMove();
    }

    // [Benchmark]
    public void UpdateAllPiecesMoves() {
        for (int i = 0; i < n; i++) {
            moveGenerator.UpdateAllPieces();
        }
    }

    // [Benchmark]
    public void TestOriginalAttackGeneration() {
        for (int i = 0; i < n; i++) {
            foreach (Piece? piece in board.pieces) {
                if (piece == null || piece.PieceTeam == board.currentTeam)
                    continue;
                // piece.GenerateSquaresAttacked(board);
            }
            // activeTeamsKing.GetRemainingAttacks(board, board.GetTeamBitboard(board.currentTeam), board.GetTeamBitboard(board.opponentTeam));
        }
    }

    // [Benchmark]
    public void Evaluation_V2() {
        for (int i = 0; i < n; i++) {
            evaluation_V2.Evaluate(board, Team.White, Team.Black);
        }
    }

    // [Benchmark]
    public void Evaluation_V3() {
        for (int i = 0; i < n; i++) {
            evaluation_V3.Evaluate(board, Team.White, Team.Black);
        }
    }

    // [Benchmark]
    public void Evaluation_V4() {
        for (int i = 0; i < n; i++) {
            evaluation_V4.Evaluate(board, Team.White, Team.Black);
        }
    }
    
    // [Benchmark]
    public void Evaluation_V5() {
        for (int i = 0; i < n; i++) {
            evaluation_V5.Evaluate(board, Team.White, Team.Black);
        }
    }
    
    [Benchmark]
    public void MoveGeneration() {
        for (int i = 0; i < n; i++) {
            // moveGenerator.GenerateMoves(board.currentTeam, false);
            moveGenerator.UpdateAllPieces();
        }
    }
    
    // [Benchmark]
    public void PopLeastSignificantBit() {
        for (int i = 0; i < n; i++) {
            ulong fullMask = ulong.MaxValue;
            while (fullMask != 0) {
                BitboardHelper.PopLeastSignificantBit(ref fullMask);
            }
        }
    }
    
    // [Benchmark]
    public void GenerateSquaresAttackedUsingBitboards() {
        for (int i = 0; i < n; i++) {
            Piece queen = board.pieces[34];
            // queen.Reset();
            // queen.GenerateSquaresAttacked(board);
        }
    }
    
    private bool CanProceedInDirection(Board board, int targetSquare, bool ignoreOpponentsKing = false) {
        if (!Board.SquareOnBoard(targetSquare))
            return false;
        Piece? blockingPiece = board.GetPieceAt(targetSquare);
        if (blockingPiece != null) {
            // ignoring the king is only used when generating the attacked squares. This is because, for example, if there's
            // a queen or a rook on a rank and we don't go past the king to mark the next squares as attacked, then the king
            // could stay on the same rank and still be in check, but ignoring the opponents king disallows this
            if (ignoreOpponentsKing && blockingPiece.PieceTeam != team && blockingPiece is King)
                return true;
            return false;
        }
        return true;
    }
    
    // [Benchmark]
    public void GetAttackLoop() {
        King opponentKing = board.GetTeamsKing(board.currentTeam);
        for (int j = 0; j < n; j++) {
            for (int i = 0; i < directions.Length; i++) {
            ulong squaresSearchedBitboard = 0;
            Piece? potentialPinnedPiece = null;
            bool checkingEnPassantIsIllegal = false;
            bool continueAddingAttackedSquares = true;
            bool continueSearchingForChecksAndPins = Board.SquareIsInDirection(squareIndex, opponentKing.SquareIndex, directions[i]);

            int numSquaresToEdge = Board.NumSquaresToEdgeFromSquare(squareIndex, directions[i]);
            for (int iterations = 1; iterations <= numSquaresToEdge; iterations++) {
                if (!continueAddingAttackedSquares && !continueSearchingForChecksAndPins)
                    break;
                int squareToSearch = squareIndex + (directions[i] * iterations);
                if (continueAddingAttackedSquares) {
                    board.AddAttackedSquare(team, squareToSearch);
                }
                if (continueAddingAttackedSquares && !CanProceedInDirection(board, squareToSearch, true)) {
                    continueAddingAttackedSquares = false;
                }

                if (!continueAddingAttackedSquares && !continueSearchingForChecksAndPins)
                    break;

                if (!continueSearchingForChecksAndPins || opponentKing.isDoubleChecked)
                    continue;
                BitboardHelper.AddSquare(ref squaresSearchedBitboard, squareToSearch);
                Piece? piece = board.GetPieceAt(squareToSearch);
                if (piece == null) {
                    continue;
                }
                Pawn? secondPawn = board.GetPieceAt<Pawn>(squareToSearch + directions[i]);
                if (piece is Pawn firstPawn && secondPawn != null && (directions[i] == CompassDirections.Left || directions[i] == CompassDirections.Right)) {
                    // the pieces are the same team so none are pinned
                    if (firstPawn.PieceTeam == secondPawn.PieceTeam || potentialPinnedPiece != null) {
                        continueSearchingForChecksAndPins = false;
                        continue;
                    }
                    checkingEnPassantIsIllegal = true;
                    // increasing the iterations since we've already checked the square after our current iteration
                    ++iterations;
                    if (firstPawn.PieceTeam != team && board.CurrentEnPassantPawn == secondPawn) {
                        potentialPinnedPiece = firstPawn;
                    }
                    else if (secondPawn.PieceTeam != team && board.CurrentEnPassantPawn == firstPawn) {
                        potentialPinnedPiece = secondPawn;
                    }
                    // if none are an en passant pawn or the en passant pawn is on our team
                    else {
                        continueSearchingForChecksAndPins = false;
                    }
                }
                else if (piece.PieceTeam != team) {
                    // can check the king
                    if (piece is King king) {
                        BitboardHelper.RemoveSquareFromBitboard(ref squaresSearchedBitboard, squareToSearch);
                        BitboardHelper.AddSquare(ref squaresSearchedBitboard, squareIndex);
                        if (checkingEnPassantIsIllegal) {
                            if (potentialPinnedPiece is not Pawn pawn) {
                                Console.WriteLine("How did we get here?!?!?!");
                                Environment.Exit(0);
                                return;
                            }
                            // pawn.enPassantIsLegal = false;
                        }
                        else if (potentialPinnedPiece != null) {
                            potentialPinnedPiece.Pin(squaresSearchedBitboard);
                        }
                        else {
                            king.Check(squaresSearchedBitboard);
                        }
                        continueSearchingForChecksAndPins = false;
                        continue;
                    }
                    // at least two opponent pieces so can't pin
                    else if (potentialPinnedPiece != null) {
                        continueSearchingForChecksAndPins = false;
                        continue;
                    }
                    potentialPinnedPiece = piece;
                }
                else if (piece.PieceTeam == team) {
                    continueSearchingForChecksAndPins = false;
                }

                if (!continueAddingAttackedSquares && !continueSearchingForChecksAndPins)
                    break;
            }
        }
        }
    }
    
    // [Benchmark]
    public void GetAttackSimplifiedLoop() {
        for (int i = 0; i < n; i++) {
            ulong attacks = 0;
            King opponentKing = board.GetTeamsKing(board.currentTeam);
            ulong teamPiecesBitboard = board.GetTeamBitboard(team);
            ulong opponentPiecesBitboard = board.GetTeamBitboard(opponentKing.PieceTeam);
            ulong teamPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, team);
            ulong opponentPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, opponentKing.PieceTeam);
            ulong allPawnsBitboard = teamPawnBitboard | opponentPawnBitboard;
            ulong allPiecesBitboard = teamPiecesBitboard | opponentPiecesBitboard;
            ulong opponentKingBitboard = 1ul << opponentKing.SquareIndex;
            foreach (int direction in directions) {
                ulong attack = 0;
                ulong attacksRegardingBlockingPieces = 0;
                int blockerSquareIndex = 0;
                bool blockerFound = false;
                int numSquaresToEdge = Board.NumSquaresToEdgeFromSquare(squareIndex, direction);
                for (int j = 1; j <= numSquaresToEdge; j++) {
                    int squareToAdd = squareIndex + (direction * j);
                    ulong squareToAddMask = 1ul << squareToAdd;
                    attack |= squareToAddMask;
                    if (!blockerFound) {
                        attacksRegardingBlockingPieces |= squareToAddMask;
                        
                        if ((allPiecesBitboard & squareToAddMask) != 0) {
                            blockerFound = true;
                            blockerSquareIndex = squareToAdd;
                        }
                    }
                    
                }
                if ((attack & opponentKingBitboard) == 0) {
                    // return attacksRegardingBlockingPieces;
                    // Console.WriteLine("No king");
                    continue;
                }
                if (blockerSquareIndex == opponentKing.SquareIndex) {
                    ulong checkLine = attacksRegardingBlockingPieces;
                    // removing the king square
                    checkLine ^= 1ul << blockerSquareIndex;
                    // add the attacker's square
                    checkLine |= 1ul << squareIndex;
                    opponentKing.Check(checkLine);
                    continue;
                }
                ulong spacesAfterLegalAttacks = attack ^ attacksRegardingBlockingPieces;
                ulong blockerSquareBitboard = 1ul << blockerSquareIndex;

                // checking whether the piece is in the opponents bitboard, if so it may need pinning
                // doing this before the en passant check because this is more common
                if ((opponentPiecesBitboard & blockerSquareBitboard) != 0) {
                    ulong squaresBetweenPieceAndKing = spacesAfterLegalAttacks;
                    // remove the king square and the potentially pinned piece square to get the squares between them
                    BitboardHelper.RemoveSquareFromBitboard(ref squaresBetweenPieceAndKing, opponentKing.SquareIndex);
                    
                    // if there's no space or there's empty space between the piece and the king, then the piece is pinned
                    if (squaresBetweenPieceAndKing == 0 || (allPiecesBitboard & squaresBetweenPieceAndKing) == 0) {
                        ulong pinnedLine = attacksRegardingBlockingPieces;
                        // remove the pinned piece square and add the attacking square
                        BitboardHelper.RemoveSquareFromBitboard(ref pinnedLine, blockerSquareIndex);
                        BitboardHelper.AddSquare(ref pinnedLine, squareIndex);
                        board.pieces[blockerSquareIndex].Pin(pinnedLine);
                    }
                }
                // checking whether to restrict en passant
                else if (NeedsToCheckRestrictionOfEnPassant(board, direction, blockerSquareIndex, allPawnsBitboard, teamPawnBitboard, opponentPawnBitboard)) {
                    ulong spacesBetweenSecondPawnAndKing = spacesAfterLegalAttacks;

                    // removing the second pawn and the king from the bitboard
                    BitboardHelper.RemoveSquareFromBitboard(ref spacesBetweenSecondPawnAndKing, blockerSquareIndex + direction);
                    BitboardHelper.RemoveSquareFromBitboard(ref spacesBetweenSecondPawnAndKing, opponentKing.SquareIndex);
                    // there's either no space or there's empty space between the two pawns and the opponent king
                    if (spacesBetweenSecondPawnAndKing == 0 || (allPiecesBitboard & spacesBetweenSecondPawnAndKing) == 0) {
                        // the first pawn is the team's en passant pawn so the opponent
                        // piece is on the second square in the direction and so needs it en passant capture restricting
                        if ((teamPiecesBitboard & blockerSquareBitboard) != 0) {
                            if (board.pieces[blockerSquareIndex + direction] is Pawn pawn) {
                                // Console.WriteLine("Restricting second pawn's en passant");
                                // pawn.enPassantIsLegal = false;
                            }
                        }
                        // the second pawn is the team's en passant pawn so the opponent
                        // piece is on the first square in the direction and so needs it en passant capture restricting
                        else if ((teamPiecesBitboard & (1ul << (blockerSquareIndex + direction))) != 0) {
                            if (board.pieces[blockerSquareIndex] is Pawn pawn) {
                                // Console.WriteLine("Restricting first pawn's en passant");
                                // pawn.enPassantIsLegal = false;
                            }
                        }
                    }
                }
            }
        }
    }
    
    private bool NeedsToCheckRestrictionOfEnPassant(Board board, int direction, int blockerSquareIndex, ulong allPawnsBitboard, ulong teamPawnBitboard, ulong opponentPawnBitboard) {
        // need to be checking attacks in the left or right direction
        if (direction != 1 && direction != -1)
            return false;
        Piece? enPassantPawn = board.CurrentEnPassantPawn;
        if (enPassantPawn == null)
            return false;
        ulong enPassantPawnBitboard = 1ul << enPassantPawn.SquareIndex;
        // en passant is allowed if the en passant pawn is of the opposite team
        if ((opponentPawnBitboard & enPassantPawnBitboard) != 0)
            return false;
        ulong squareIndexBitboard = 1ul << blockerSquareIndex;
        ulong nextSquareIndexBitboard = 1ul << (blockerSquareIndex + direction);
        
        // combining the first and next square together
        ulong combinedSquaresMask = squareIndexBitboard | nextSquareIndexBitboard;
        // getting the pieces from the squares that are pawns
        ulong matchingPawnsMask = combinedSquaresMask & allPawnsBitboard;
        ulong matchingPawnsWithEnPassantMask = matchingPawnsMask | enPassantPawnBitboard;
        // if the masks aren't equal then at least one of the squares in the combinedSquaresMask doesn't contain a pawn
        // or none of the pawns match with the en passant pawn
        if (matchingPawnsWithEnPassantMask != combinedSquaresMask)
            return false;
        bool squareIndexIsOfCurrentTeam = (teamPawnBitboard & squareIndexBitboard) != 0;
        bool nextSquareIndexIsOfCurrentTeam = (teamPawnBitboard & nextSquareIndexBitboard) != 0;
        // they both can't be true and both can't be false otherwise that'd mean they're on the same team
        return squareIndexIsOfCurrentTeam != nextSquareIndexIsOfCurrentTeam;
    }

    // [MethodImpl(MethodImplOptions.NoOptimization)]
    // [Benchmark]
    public void GetAttacksUsingImproved() {
        for (int i = 0; i < n; i++) {
            King opponentKing = board.GetOpposingTeamsKing(board.currentTeam);
            ulong allPiecesBitboard = board.GetAllPiecesBitboard();
            while (allPiecesBitboard != 0) {
                int pieceSquareIndex = BitboardHelper.PopLeastSignificantBit(ref allPiecesBitboard);
                Piece piece = board.pieces[pieceSquareIndex];
                if (piece.PieceTeam == board.currentTeam) {
                    piece.GenerateSquaresAttacked(board, opponentKing);
                }
                else {
                    piece.GenerateSquaresAttacked(board, opponentKing);
                }
            }
        }
    }
    
    // [Benchmark]
    public void GetAttacksWithQueen() {
        for (int i = 0; i < n; i++) {
            King opponentKing = board.GetOpposingTeamsKing(board.currentTeam);
            ulong allPiecesBitboard = board.GetAllPiecesBitboard();
            allPiecesBitboard &= 1ul << 21;
            while (allPiecesBitboard != 0) {
                int pieceSquareIndex = BitboardHelper.PopLeastSignificantBit(ref allPiecesBitboard);
                Piece piece = board.pieces[pieceSquareIndex];
                if (piece.PieceTeam == board.currentTeam) {
                    piece.GenerateSquaresAttacked(board, opponentKing);
                }
                else {
                    piece.GenerateSquaresAttacked(board, opponentKing);
                }
            }
        }
    }
    
    // [Benchmark]
    public void PrecomputedAttackBitboard() {
        for (int i = 0; i < n; i++) {
            foreach (int direction in directions)
                BitboardHelper.GetAttackBitboardInSingleDirection(squareIndex, direction);
        }
    }
    
    // [Benchmark]
    public void RuntimeAttackBitboard() {
        for (int i = 0; i < n; i++) {
            foreach (int direction in directions)
                BitboardHelper.GetAttackBitboard(squareIndex, direction);
        }
    }
    
    // [Benchmark]
    public void CreateColumnsOld() {
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < 8; j++) {
                BitboardHelper.OldColumnCreation(j);
            }
        }
    }
    
    // [Benchmark]
    public void CreateColumns() {
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < 8; j++) {
                BitboardHelper.CreateColumns(j);
            }
        }
    }
    
    // [Benchmark]
    public void GetPieceAt() {
        for (int i = 0; i < n; i++) {
            King? king = board.GetPieceAt<King>(39);
        }
    }
    
    // [Benchmark]
    public void GetPieceAtBitboards() {
        for (int i = 0; i < n; i++) {
            ulong bitboardCopy = blackKingBitboard;
            int squareIndex = BitboardHelper.PopLeastSignificantBit(ref bitboardCopy);
        }
    }
    
    // [Benchmark]
    public void MoveFunctionality() {
        Move move = moves[0];
        for (int i = 0; i < n; i++) {
            board.MakeMove(move);
            board.UndoMove();
        }
    }
    
    // [Benchmark]
    public void Evaluation_V6() {
        for (int i = 0; i < n; i++) {
            evaluation_V6.Evaluate(board, Team.White, Team.Black);
        }
    }
    
    // [Benchmark]
    public void Evaluation_V6_CalculatingPawnScore() {
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < 8; j++) {
                int numPawnsOnFile = BitboardHelper.GetPieceCount(teamPawnBitboard, fileMasks[j]);
                evaluation_V6.GetPawnScore(board, teamPawnBitboard, opponentPiecesWithoutPawns, opponentPawnBitboard, numPawnsOnFile, j);
            }
        }
    }

    // [Benchmark]
    public void PopLeastSignificants() {
        for (int iterations = 0; iterations < n; iterations++) {
            ulong bitboard = teamPawnBitboard;
            while (bitboard != 0) {
                BitboardHelper.PopLeastSignificantBit(ref bitboard);
            }
        }
    }
    
    // [Benchmark]
    public void PopMostSignificants() {
        for (int iterations = 0; iterations < n; iterations++) {
            ulong bitboard = teamPawnBitboard;
            while (bitboard != 0) {
                BitboardHelper.PopMostSignificantBit(ref bitboard);
            }
        }
    }
    
    // [Benchmark]
    public void GetLeastSignificants() {
        ulong bitboard = teamPawnBitboard;
        for (int iterations = 0; iterations < n; iterations++) {
            BitboardHelper.GetLeastSignificantBit(bitboard);
        }
    }
    
    // [Benchmark]
    public void GetMostSignificants() {
        ulong bitboard = teamPawnBitboard;
        for (int iterations = 0; iterations < n; iterations++) {
            BitboardHelper.GetMostSignificantBit(bitboard);
        }
    }
    
    // [Benchmark]
    public void ArraySquareIndexes() {
        for (int iterations = 0; iterations < n; iterations++) {
            for (int i = 0; i < 8; i++) {
                // int[] squareIndexes = teamPawnBitboard.GetSquareIndexes();
            }
        }
    }
    
    // [Benchmark]
    public void GetSquareIndexFromBitboardUsingSpans() {
        ulong fileMask = 0b0000000100000001000000010000000100000001000000010000000100000001;
        for (int iterations = 0; iterations < n; iterations++) {
            for (int i = 0; i < 8; i++) {
                // int[] pawnSquareIndexes = BitboardHelper.GetSquareIndexFromBitboardUsingSpans(teamPawnBitboard & (fileMask << i));
            }
        }
    }

    // [Benchmark]
    public void GetPieceCountBuiltIn() {
        // ulong fileMask = 0b0000000100000001000000010000000100000001000000010000000100000001;
        for (int iterations = 0; iterations < n; iterations++) {
            // bitboard1.GetPieceCount(bitboard1);
        }
    }
    
    // [Benchmark]
    public void GetPieceCountMine() {
        // ulong fileMask = 0b0000000100000001000000010000000100000001000000010000000100000001;
        for (int iterations = 0; iterations < n; iterations++) {
            // bitboard1.GetPieceCount(ulong.MaxValue);
        }
    }
}