namespace Chess_Application {
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
            // GameInfo gameInfo = new GameInfo();
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
            for (int arrIdx = 0, squareIdx = Board.dimensions * (rowNumber - 1); arrIdx < piecesLayout.Length; arrIdx++) {
                char symbol = piecesLayout[arrIdx];
                if (char.IsNumber(symbol)) {
                    squareIdx += int.Parse(symbol.ToString());
                    continue;
                }
                if (symbol == '/') {
                    squareIdx = Board.dimensions * (--rowNumber - 1);
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
            for (int y = Board.dimensions - 1; y >= 0; y--) {
                int numBlankSpaces = 0;
                for (int x = 0; x < Board.dimensions; x++) {
                    Piece piece = board.pieces[x + y * Board.dimensions];
                    if (piece == null) {
                        numBlankSpaces++;
                        continue;
                    }
                    if (numBlankSpaces > 0) {
                        FEN += numBlankSpaces;
                        numBlankSpaces = 0;
                    }
                    FEN += board.pieces[x + y * Board.dimensions].ToString();
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
        
        public static string GetFENString(Board board) {
            string FEN = string.Empty;
            for (int y = Board.dimensions - 1; y >= 0; y--) {
                int numBlankSpaces = 0;
                for (int x = 0; x < Board.dimensions; x++) {
                    Piece piece = board.pieces[x + y * Board.dimensions];
                    if (piece == null) {
                        numBlankSpaces++;
                        continue;
                    }
                    if (numBlankSpaces > 0) {
                        FEN += numBlankSpaces;
                        numBlankSpaces = 0;
                    }
                    FEN += board.pieces[x + y * Board.dimensions].ToString();
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

            int enPassantCaptureSquare = Board.ConvertChessNotationToSquare(FENRecord[3]);
            if (enPassantCaptureSquare == -1)
                return;
                
            // if the current team is white, then black was the previous team that enabled en passant and they push pawns downwards, hence -1
            int pawnDirection = board.currentTeam == Team.White ? -1 : 1;
            if (board.pieces[enPassantCaptureSquare + CompassDirections.Up * pawnDirection] is not Pawn enPassantPawn)
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
            for (int i = indexOfMoves + 1; i < FENRecord.Length; i++) {
                string moveInChessNotation = FENRecord[i];
                Move move = new Move(moveInChessNotation);
                // Console.WriteLine(move);
                Move[] moves = moveGenerator.UpdateAllPieces();
                Piece? pieceToMove = board.pieces[move.startingSquare];
                Move actualMove = GetMove(pieceToMove, moves, move);
                if (actualMove == Move.NullMove) {
                    Console.WriteLine($"Can't perform {move}");
                    return;
                }
                board.MakeMove(actualMove);
            }
        }

        private Move GetMove(Piece? pieceTryingToBeMoved, Move[] moves, Move moveTryingToBeMade) {
            if (pieceTryingToBeMoved == null)
                return Move.NullMove;
            for (int i = 0; i < pieceTryingToBeMoved.NumMoves; i++) {
                Move move = pieceTryingToBeMoved.GetMove(moves, i);
                // shouldn't ever be null with it using the numMoves variable
                // as well as using the 'GetMove' method on the piece but
                // I'm using it just in case :)
                if (move.isNullMove)
                    break;
                if (move.targetSquare == moveTryingToBeMade.targetSquare) {
                    if (moveTryingToBeMade.IsPromotion && moveTryingToBeMade.specialMoveType != move.specialMoveType) {
                        continue;
                    }
                    return move;
                }
            }
            return Move.NullMove;
        }
    }
}