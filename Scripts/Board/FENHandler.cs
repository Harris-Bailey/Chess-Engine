namespace Chess;
 
public class FENHandler {

    private Board board;
    private MoveGenerator moveGenerator;
    string[] FENRecord;
    public const string FENStartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public FENHandler(Board board, MoveGenerator moveGenerator) {
        this.board = board;
        this.moveGenerator = moveGenerator;
        FENRecord = Array.Empty<string>();
    }

    public void Initialise(string FENString = FENStartingPosition) {
        FENRecord = FENString.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Piece[] pieces = SetBoardsPieces();
        Team startingTeam = SetStartingTeam();
        
        int numPieces = Enum.GetNames(typeof(BitboardIndexes)).Length;
        board.SetBoard(pieces, numPieces, startingTeam);
        
        moveGenerator.Initialise(board);
        SetCastlingRights();
        SetEnPassantPawn();
        SetHalfMove();
        SetFullMove();
        AddMoves();
    }

    public Piece[] SetBoardsPieces() {
        if (FENRecord.Length < 1) {
            return Array.Empty<Piece>();
        }
        Piece[] pieces = new Piece[64];
        string piecesLayout = FENRecord[0];
        int rowNumber = 8;
        for (int arrIdx = 0, squareIdx = Board.Dimensions * (rowNumber - 1); arrIdx < piecesLayout.Length; arrIdx++) {
            char symbol = piecesLayout[arrIdx];
            if (char.IsNumber(symbol)) {
                squareIdx += int.Parse(symbol.ToString());
                continue;
            }
            if (symbol == '/') {
                squareIdx = Board.Dimensions * (--rowNumber - 1);
                continue;
            }

            Team piecesTeam = char.IsUpper(symbol) ? Team.White : Team.Black;
            Piece? piece = Board.CreatePiece(symbol, squareIdx, piecesTeam);
            if (piece != null)
                pieces[squareIdx] = piece;
            ++squareIdx;
        }
        return pieces;
    }
    
    public string GetFENString() {
        string FEN = string.Empty;
        for (int y = Board.Dimensions - 1; y >= 0; y--) {
            int numBlankSpaces = 0;
            for (int x = 0; x < Board.Dimensions; x++) {
                Piece piece = board.Pieces[x + y * Board.Dimensions];
                if (piece == null) {
                    numBlankSpaces++;
                    continue;
                }
                if (numBlankSpaces > 0) {
                    FEN += numBlankSpaces;
                    numBlankSpaces = 0;
                }
                FEN += board.Pieces[x + y * Board.Dimensions].ToString();
            }
            if (numBlankSpaces > 0)
                FEN += numBlankSpaces;
            FEN += "/";
        }
        if (board.currentTeam == Team.White) {
            FEN += " w ";
        }
        else if (board.currentTeam == Team.Black) {
            FEN += " b ";
        }
        King whiteKing = board.GetTeamsKing(Team.White);
        King blackKing = board.GetTeamsKing(Team.Black);
        string castlingRights = string.Empty;
        if (!whiteKing.HasMoved) {
            if (whiteKing.canCastleKingside)
                castlingRights += "K";
            if (whiteKing.canCastleQueenside)
                castlingRights += "Q";
        }
        if (!blackKing.HasMoved) {
            if (blackKing.canCastleKingside)
                castlingRights += "k";
            if (blackKing.canCastleQueenside)
                castlingRights += "q";
        }
        if (castlingRights == string.Empty)
            castlingRights = "-";
        FEN += castlingRights;
        
        return FEN.Trim('/');
    }

    public Team SetStartingTeam() {
        if (FENRecord.Length < 2) {
            return Team.White;
        }

        return FENRecord[1].ToLower() switch {
            "b" => Team.Black,
            // defaulted to white plays first
            _ => Team.White,
        };
    }

    public void SetCastlingRights() {
        if (FENRecord.Length < 3 || FENRecord[2] == "-")
            return;
        King whiteKing = board.GetTeamsKing(Team.White);
        King blackKing = board.GetTeamsKing(Team.Black);

        foreach (char c in FENRecord[2]) {
            switch (c) {
                case 'K':
                    whiteKing.canCastleKingside = true;
                    break;
                case 'Q':
                    whiteKing.canCastleQueenside = true;
                    break;
                case 'k':
                    blackKing.canCastleKingside = true;
                    break;
                case 'q':
                    blackKing.canCastleQueenside = true;
                    break;
            }
        }
    }

    public void SetEnPassantPawn() {
        if (FENRecord.Length < 4 || FENRecord[3] == "-") {
            return;
        }

        int enPassantCaptureSquare = Move.MoveNotationToSquare(FENRecord[3]);
        if (enPassantCaptureSquare == -1)
            return;
            
        // if the current team is white, then black was the previous team that enabled en passant and they push pawns downwards, hence -1
        int pawnDirection = board.currentTeam == Team.White ? -1 : 1;
        if (board.Pieces[enPassantCaptureSquare + CompassDirections.Up * pawnDirection] is not Pawn enPassantPawn)
            return;
        

        // saving the inital castling rights to use in the gamestate
        string castlingRights = board.GetTeamsKing(Team.White).GetCastlingString() + board.GetTeamsKing(Team.Black).GetCastlingString();
        if (castlingRights.Length == 0)
            castlingRights = "-";
        
        // adding the en passant move so the board knows it's capturable
        Move enPassantMove = new Move(enPassantPawn.SquareIndex + CompassDirections.Up * 2 * pawnDirection, enPassantPawn.SquareIndex, Move.SpecialMoveType.PushPawnTwoSquares);
        board.gameStates.Push(new GameState(enPassantMove, enPassantPawn, castlingRights, true));
    }

    public void SetHalfMove() {

    }

    public void SetFullMove() {

    }

    public void AddMoves() {
        int indexOfMoves = Array.IndexOf(FENRecord, "moves");
        if (indexOfMoves == -1) {
            return;
        }
        string[] requestedMoves = FENRecord[(indexOfMoves + 1)..];
        foreach (string requestedMove in requestedMoves) {
            int startSquare = Move.MoveNotationToSquare(requestedMove[0..2]);
            int targetSquare = Move.MoveNotationToSquare(requestedMove[2..4]);
            
            Move[] legalMoves = moveGenerator.UpdateAllPieces();
            foreach (Move move in legalMoves) {
                if (move.startingSquare == startSquare && move.targetSquare == targetSquare)
                    board.MakeMove(move);
            }
        }
    }
    
    
    public static string GetFENString(Board board) {
        string FEN = GetFENPieces(board).Trim('/');
        if (board.currentTeam == Team.White) {
            FEN += " w ";
        }
        else if (board.currentTeam == Team.Black) {
            FEN += " b ";
        }
        King whiteKing = board.GetTeamsKing(Team.White);
        King blackKing = board.GetTeamsKing(Team.Black);
        if (whiteKing.canCastleKingside) {
            FEN += "K";
        }
        if (whiteKing.canCastleQueenside)
            FEN += "Q";
        if (blackKing.canCastleKingside)
            FEN += "k";
        if (blackKing.canCastleQueenside)
            FEN += "q";
        if (!whiteKing.canCastleKingside && !whiteKing.canCastleQueenside && !blackKing.canCastleKingside && !blackKing.canCastleQueenside)
            FEN += "-";
        return FEN;
    }
    
    public static string GetFENPieces(Board board) {
        string FEN = string.Empty;
        for (int y = Board.Dimensions - 1; y >= 0; y--) {
            int numBlankSpaces = 0;
            for (int x = 0; x < Board.Dimensions; x++) {
                Piece piece = board.Pieces[x + y * Board.Dimensions];
                if (piece == null) {
                    numBlankSpaces++;
                    continue;
                }
                if (numBlankSpaces > 0) {
                    FEN += numBlankSpaces;
                    numBlankSpaces = 0;
                }
                FEN += board.Pieces[x + y * Board.Dimensions].ToString();
            }
            if (numBlankSpaces > 0)
                FEN += numBlankSpaces;
            FEN += "/";
        }
        return FEN;
    }
}