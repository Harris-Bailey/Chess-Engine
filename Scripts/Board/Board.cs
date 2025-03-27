namespace Chess;

public class Board {
    
    public const int Dimensions = 8;
    private static readonly int[][] numSquaresToEdgeInDirection;
    public static readonly int NumPieces;
    
    public Piece[] Pieces;
    public King[] Kings { get; private set; }
    
    // the attacked squares by white is 0th index, squares attacked by black is 1st index
    private ulong[] attackedSquaresByTeamBitboards;
    
    // the index for all of white's pieces is equal to numPieceBitboardsPerTeam 
    // and the bitboard for all of black's pieces is equal to (numPieceBitboardsPerTeam * 2) - 1
    public ulong[] pieceBitboards;
    public ulong[] teamBitboards;
    private int numPieceBitboardsPerTeam;
    public Stack<GameState> gameStates;
    public Pawn? CurrentEnPassantPawn { 
        get {
            if (gameStates.Count == 0)
                return null;
            return gameStates.Peek().PawnCapturableByEnPassant;
        }
    }
    public Team currentTeam { get; private set; }
    public Team opponentTeam { get; private set; }
    public Stack<string> repetitionTable;
    
    static Board() {
        numSquaresToEdgeInDirection = new int[64][];
        for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
            numSquaresToEdgeInDirection[squareIndex] = new int[8];
            for (int dirIndex = 0; dirIndex < CompassDirections.CardinalsAndDiagonals.Length; dirIndex++) {
                int direction = CompassDirections.CardinalsAndDiagonals[dirIndex];
                numSquaresToEdgeInDirection[squareIndex][dirIndex] = NumSquaresToEdgeFromSquare(squareIndex, direction);
            }
        }
        NumPieces = Enum.GetNames(typeof(BitboardIndexes)).Length;
    }

    public Board() {
        attackedSquaresByTeamBitboards = new ulong[2];
        pieceBitboards = Array.Empty<ulong>();
        teamBitboards = Array.Empty<ulong>();
        Kings = new King[2];
        gameStates = new Stack<GameState>();
        Pieces = new Piece[Dimensions * Dimensions];
        currentTeam = Team.White;
        opponentTeam = Team.Black;
        repetitionTable = new Stack<string>();
    }

    public void SetBoard(Piece[] pieces, int numberOfPieces, Team currentTeam) {
        Pieces = pieces;
        numPieceBitboardsPerTeam = numberOfPieces;
        int numTeams = Enum.GetValues(typeof(Team)).Length;
        pieceBitboards = new ulong[numPieceBitboardsPerTeam * numTeams];
        teamBitboards = new ulong[numTeams];
        Kings = new King[2];
        foreach (Piece piece in pieces) {
            if (piece == null)
                continue;
            
            BitboardHelper.AddSquare(ref pieceBitboards[piece.ClassID + numPieceBitboardsPerTeam * (int)piece.PieceTeam], piece.SquareIndex);
            BitboardHelper.AddSquare(ref teamBitboards[(int)piece.PieceTeam], piece.SquareIndex);
            if (piece is King king) {
                int kingIndex = (int)king.PieceTeam;
                if (Kings[kingIndex] != null) {
                    Console.WriteLine($"There are too many kings for the {king.PieceTeam} team");
                    Environment.Exit(0);
                    return;
                }
                Kings[kingIndex] = king;
            }
        }
        this.currentTeam = currentTeam;
        opponentTeam = currentTeam == Team.White ? Team.Black : Team.White;
    }

    public ulong GetTeamBitboard(Team team) {
        return teamBitboards[(int)team];
    }
    
    public ulong GetAllPiecesBitboard() {
        return teamBitboards[(int)Team.White] | teamBitboards[(int)Team.Black];
    }

    public ulong GetPieceBitboard(BitboardIndexes index, Team team) {
        int bitboardIndex = (int)index + numPieceBitboardsPerTeam * (int)team;
        return pieceBitboards[bitboardIndex];
    }
    
    private int GetPieceBitboardIndex(int pieceID, Team team) {
        return pieceID + numPieceBitboardsPerTeam * (int)team;
    }

    public void PrintBoard() {
        // print the board from the top to bottom so it prints in the terminal correctly
        for (int y = Dimensions - 1; y >= 0; y--) {
            string row = $"{y + 1} [ ";
            for (int x = 0; x < Dimensions; x++) {
                int pieceIndex = x + y * Dimensions;
                if (Pieces[pieceIndex] == null) {
                    row += ". ";
                    continue;
                } 
                row += $"{Pieces[pieceIndex]} ";
            }
            row += "]";
            Console.WriteLine(row);
        }
        Console.WriteLine("    A B C D E F G H");
    }

    public void MakeMove(Move move) {            
        int startingSquare = move.startingSquare;
        int targetSquare = move.targetSquare;
        
        Pawn? enPassantPawn = null;
        Piece? promotedPawn = null;
        Piece? capturedPiece = Pieces[targetSquare];
        
        King whiteKing = GetTeamsKing(Team.White);
        King blackKing = GetTeamsKing(Team.Black);
        string castlingAvailability = whiteKing.GetCastlingString() + blackKing.GetCastlingString();
        if (castlingAvailability.Length == 0)
            castlingAvailability = "-";
            
        if (move.specialMoveType == Move.SpecialMoveType.PushPawnTwoSquares) {
            // it's the starting square because we've not moved the pawn yet
            enPassantPawn = Pieces[move.startingSquare] as Pawn;
        }
        else if (move.specialMoveType == Move.SpecialMoveType.EnPassantCapture) {
            // ignore the null warning since the piece can't be null if it enters this if statement
            # nullable disable
            capturedPiece = CurrentEnPassantPawn;
            RemovePieceAt(CurrentEnPassantPawn.SquareIndex);
            # nullable enable
        }
        else if (move.specialMoveType == Move.SpecialMoveType.CastlingQueenside) {
            Piece king = Pieces[move.startingSquare];
            Piece rook = Pieces[king.SquareIndex - 4];
            if (rook == null) {
                Console.WriteLine("Error!");
                Console.WriteLine("Can't get rook for queenside castling!");
                Environment.Exit(0);
                return;
            }
            MovePiece(rook, rook.SquareIndex, move.targetSquare + 1);
            rook.SetToMoved();
            if (king is King castedKing) {
                castedKing.canCastleQueenside = false;
                castedKing.canCastleKingside = false;
            }
            
        }
        else if (move.specialMoveType == Move.SpecialMoveType.CastlingKingside) {
            Piece king = Pieces[move.startingSquare];
            Piece rook = Pieces[king.SquareIndex + 3];
            if (rook == null) {
                Console.WriteLine("Error!");
                Console.WriteLine("Can't get rook for kingside castling!");
                PrintBoard();
                Console.WriteLine(move);
                Environment.Exit(0);
                return;
            }
            MovePiece(rook, rook.SquareIndex, move.targetSquare - 1);
            rook.SetToMoved();
            if (king is King castedKing) {
                castedKing.canCastleQueenside = false;
                castedKing.canCastleKingside = false;
            }
        }
        else if (move.IsPromotion) {
            Piece pawnUpgrading = Pieces[move.startingSquare];
            promotedPawn = pawnUpgrading;
            RemovePieceInBitboards(pawnUpgrading);
            char pieceSymbol = ' ';
            switch (move.specialMoveType) {
                case Move.SpecialMoveType.PromoteToQueen:
                    pieceSymbol = 'q';
                    break;
                case Move.SpecialMoveType.PromoteToRook:
                    pieceSymbol = 'r';
                    break;
                case Move.SpecialMoveType.PromoteToBishop:
                    pieceSymbol = 'b';
                    break;
                case Move.SpecialMoveType.PromoteToKnight:
                    pieceSymbol = 'n';
                    break;
            }
            Piece? piece = CreatePiece(pieceSymbol, pawnUpgrading.SquareIndex, pawnUpgrading.PieceTeam);
            if (piece != null)
                Pieces[pawnUpgrading.SquareIndex] = piece;
        }
        Piece pieceToMove = Pieces[startingSquare];
        if (capturedPiece != null) {
            RemovePieceInBitboards(capturedPiece);
        }
        MovePiece(pieceToMove, startingSquare, targetSquare);
        GameState newState = new GameState(move, enPassantPawn, capturedPiece, promotedPawn, castlingAvailability, pieceToMove.HasMoved);
        gameStates.Push(newState);
        repetitionTable.Push(FENHandler.GetFENPieces(this));
        pieceToMove.SetToMoved();
        (currentTeam, opponentTeam) = (opponentTeam, currentTeam);
    }

    public void MovePiece(Piece piece, int startingSquare, int targetSquare) {
        // there is nothing in the start square anymore
        # nullable disable
        Pieces[startingSquare] = null;
        # nullable enable
        // the target square now has the piece in it
        Pieces[targetSquare] = piece;
        piece.SquareIndex = targetSquare;
        
        
        ref ulong pieceBitboard = ref pieceBitboards[GetPieceBitboardIndex(piece.ClassID, piece.PieceTeam)];
        // removing the starting square from the bitboard
        pieceBitboard &= ~(1ul << startingSquare);
        // adding the new target square to the bitboard
        pieceBitboard |= 1ul << targetSquare;
        
        ref ulong teamBitboard = ref teamBitboards[(int)piece.PieceTeam];
        // removing the starting square from the bitboard
        teamBitboard &= ~(1ul << startingSquare);
        // adding the new target square to the bitboard
        teamBitboard |= 1ul << targetSquare;
    }

    public void RemovePieceInBitboards(Piece piece) {       
        ulong squareIndexRemoverMask = ~(1ul << piece.SquareIndex);
        pieceBitboards[GetPieceBitboardIndex(piece.ClassID, piece.PieceTeam)] &= squareIndexRemoverMask;
        pieceBitboards[(int)piece.PieceTeam] &= squareIndexRemoverMask;
    }

    public void AddPieceInBitboards(Piece piece) {
        ulong squareBitboard = 1ul << piece.SquareIndex;
        pieceBitboards[GetPieceBitboardIndex(piece.ClassID, piece.PieceTeam)] |= squareBitboard;
        teamBitboards[(int)piece.PieceTeam] |= squareBitboard;
    }

    public void RemovePieceAt(int squareIndex) {
        RemovePieceInBitboards(Pieces[squareIndex]);
        
        # nullable disable
        Pieces[squareIndex] = null;
        # nullable enable
    }

    public void AddPieceAt(Piece piece, int squareIndex) {
        Pieces[squareIndex] = piece;
        
        AddPieceInBitboards(piece);
    }

    // this is majorly for the AI player but still can be used for things like
    // seeing the move history
    public void UndoMove() {
        if (gameStates.Count == 0)
            return;
            
        GameState previousState = gameStates.Pop();
        repetitionTable.Pop();
        Move previousMove = previousState.MovePlayed;
        string castlingRights = previousState.CastlingAvailability;

        if (previousMove.specialMoveType == Move.SpecialMoveType.CastlingQueenside) {
            Piece king = Pieces[previousMove.targetSquare];
            Piece rook = Pieces[king.SquareIndex + 1];
            MovePiece(rook, rook.SquareIndex, previousMove.startingSquare - 4);
            rook.SetToNotMoved();
            if (king is not King castedKing) {
                Console.WriteLine("King is not an actual king...");
                return;
            }
            char kingsideSymbol = castedKing.PieceTeam == Team.White ? 'K' : 'k';
            char queensideSymbol = castedKing.PieceTeam == Team.White ? 'Q' : 'q';
            if (castlingRights.Length > 0 && castlingRights.Contains(kingsideSymbol)) {
                castedKing.canCastleKingside = true;
            }
            if (castlingRights.Length > 1 && castlingRights.Contains(queensideSymbol)) {
                castedKing.canCastleQueenside = true;
            }
        }
        else if (previousMove.specialMoveType == Move.SpecialMoveType.CastlingKingside) {
            Piece king = Pieces[previousMove.targetSquare];
            Piece rook = Pieces[king.SquareIndex - 1];
            MovePiece(rook, rook.SquareIndex, previousMove.startingSquare + 3);
            rook.SetToNotMoved();
            if (king is not King castedKing) {
                Console.WriteLine("King is not an actual king... Error on line 309");
                return;
            }
            char kingsideSymbol = castedKing.PieceTeam == Team.White ? 'K' : 'k';
            char queensideSymbol = castedKing.PieceTeam == Team.White ? 'Q' : 'q';
            if (castlingRights.Length > 0 && castlingRights.Contains(kingsideSymbol)) {
                castedKing.canCastleKingside = true;
            }
            if (castlingRights.Length > 1 && castlingRights.Contains(queensideSymbol)) {
                castedKing.canCastleQueenside = true;
            }
        }
        else if (previousMove.IsPromotion) {
            if (previousState.PromotedPawn == null) {
                Console.WriteLine("Error when trying to revert the promotion");
                Console.WriteLine("Pawn doesn't exist apparently?!??!");
                Environment.Exit(0);
                return;
            }
            Piece promotedPiece = Pieces[previousMove.targetSquare];
            RemovePieceInBitboards(promotedPiece);
            Pieces[previousMove.targetSquare] = previousState.PromotedPawn;
            AddPieceInBitboards(previousState.PromotedPawn);
        }
        Piece pieceToMove = Pieces[previousMove.targetSquare];

        MovePiece(pieceToMove, previousMove.targetSquare, previousMove.startingSquare);
        if (previousState.CapturedPiece != null) {
            AddPieceAt(previousState.CapturedPiece, previousState.CapturedPiece.SquareIndex);
        }
        pieceToMove.SetMoved(previousState.PieceMovedBefore);
        (currentTeam, opponentTeam) = (opponentTeam, currentTeam);
    }

    public King GetTeamsKing(Team team) {
        return Kings[(int)team];
    }
    
    public ulong GetSquaresAttackedByNextTeam(Team team) {
        return attackedSquaresByTeamBitboards[((int)team + 1) % attackedSquaresByTeamBitboards.Length];
    }
    
    public ulong GetSquaresAttacked(Team team) {
        return attackedSquaresByTeamBitboards[(int)team];
    }
    
    public void AddAttacks(Team pieceTeam, ulong attacksBitboard) {
        attackedSquaresByTeamBitboards[(int)pieceTeam] |= attacksBitboard;
    }
    
    public void ClearAttackedSquares(Team pieceTeam) {
        attackedSquaresByTeamBitboards[(int)pieceTeam] = 0;
    }
    
    public void PinPiece(int squareIndex, ulong pinBitboard) {
        Pieces[squareIndex].Pin(pinBitboard);
    }
    
    public void UnpinAllPinnedPieces() {
        ulong currentTeamsPieces = GetTeamBitboard(currentTeam);
        while (currentTeamsPieces != 0) {
            int squareIndex = BitboardHelper.PopLeastSignificantBit(ref currentTeamsPieces);
            Pieces[squareIndex].Unpin();
        }
    }
    
    
    
    public static bool SquareOnBoard(int squareIdx) {
        return squareIdx < Dimensions * Dimensions && squareIdx >= 0;
    }

    public static int NumSquaresToEdgeFromSquare(int squareIdx, int direction) {
        Coordinate directionCoord = ConvertDirectionToCoord(direction);
        Coordinate squareCoord = new Coordinate(squareIdx);

        Coordinate coordToCheck = squareCoord + directionCoord;
        int numUntilEdge = 0;
        while (SquareOnBoard(coordToCheck.ConvertToSquareIndex())) {
            coordToCheck += directionCoord;
            numUntilEdge++;
        }

        return numUntilEdge;
    }

    public static Coordinate ConvertDirectionToCoord(int direction) {
        return direction switch {
            1  => new Coordinate( 1,  0),
            7  => new Coordinate(-1,  1),
            8  => new Coordinate( 0,  1),
            9  => new Coordinate( 1,  1),
            
            -1 => new Coordinate(-1,  0),
            -8 => new Coordinate( 0, -1),
            -7 => new Coordinate( 1, -1),
            -9 => new Coordinate(-1, -1),
            
            _  => new Coordinate( 0,  0),
        };
    }

    public static bool IsSameFile(int squareIndexOne, int squareIndexTwo) {
        int squareOneRank = squareIndexOne % Dimensions;
        int squareTwoRank = squareIndexTwo % Dimensions;

        return squareOneRank == squareTwoRank;
    }

    public static bool IsSameRank(int squareIndexOne, int squareIndexTwo) {
        int squareOneRank = squareIndexOne / Dimensions;
        int squareTwoRank = squareIndexTwo / Dimensions;

        return squareOneRank == squareTwoRank;
    }

    public static int GetRank(int squareIndex) {
        return squareIndex / Dimensions;
    }

    public static int GetFile(int squareIndex) {
        return squareIndex % Dimensions;
    }

    public static bool SquareIsInDirection(int startingSquare, int targetSquare, int direction) {
        Coordinate startingCoord = new Coordinate(startingSquare);
        Coordinate targetCoord = new Coordinate(targetSquare);
        
        if (CompassDirections.IsDiagonalDirection(direction)) {
            return Math.Abs(targetCoord.x - startingCoord.x) == Math.Abs(targetCoord.y - startingCoord.y);
        }
        else if (CompassDirections.IsHoriztonal(direction)) {
            return startingCoord.y == targetCoord.y;
        }
        else if (CompassDirections.IsVertical(direction)) {
            return startingCoord.x == targetCoord.x;
        }
        return false;
    }
    
    public static Team GetOpposingTeam(Team myTeam) {
        return myTeam switch {
            Team.White => Team.Black,
            _ => Team.White,
        };
    }
    
    public static Piece? CreatePiece(char pieceSymbol, int squareIdx, Team piecesTeam) {
        pieceSymbol = char.ToLower(pieceSymbol);
        switch (pieceSymbol) {
            case 'p':
                Coordinate coord = new Coordinate(squareIdx);
                return new Pawn(piecesTeam, squareIdx, coord.y);
            case 'b':
                return new Bishop(piecesTeam, squareIdx);
            case 'n':
                return new Knight(piecesTeam, squareIdx);
            case 'r':
                return new Rook(piecesTeam, squareIdx);
            case 'q':
                return new Queen(piecesTeam, squareIdx);
            case 'k':
                return new King(piecesTeam, squareIdx);
            default:
                return null;
        }
    }
}