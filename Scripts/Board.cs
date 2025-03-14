using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Chess_Application {
    public class Board {
        public static int[][] NumSquaresToEdgeInDirection;
        public readonly static int NumPieces;
        public Piece[] pieces;
        
        // public int[] occupiedSquares;
        public const int dimensions = 8;
        
        // white king is 0th index, black king is 1st index
        public King[] kings { get; private set; }
        
        // the attacked squares by white is 0th index, squares attacked by black is 1st index
        public ulong[] attackedSquaresBitboard;
        
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
        // private ulong pinnedPieces;
        
        static Board() {
            NumSquaresToEdgeInDirection = new int[64][];
            for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
                NumSquaresToEdgeInDirection[squareIndex] = new int[8];
                for (int dirIndex = 0; dirIndex < CompassDirections.CardinalsAndDiagonals.Length; dirIndex++) {
                    int direction = CompassDirections.CardinalsAndDiagonals[dirIndex];
                    NumSquaresToEdgeInDirection[squareIndex][dirIndex] = NumSquaresToEdgeFromSquare(squareIndex, direction);
                }
            }
            NumPieces = Enum.GetNames(typeof(BitboardIndexes)).Length;
        }

        public Board() {
            attackedSquaresBitboard = new ulong[2];
            pieceBitboards = Array.Empty<ulong>();
            teamBitboards = Array.Empty<ulong>();
            kings = new King[2];
            gameStates = new Stack<GameState>();
            pieces = new Piece[dimensions * dimensions];
            currentTeam = Team.White;
            opponentTeam = Team.Black;
            repetitionTable = new Stack<string>();
        }

        public void SetBoard(Piece[] pieces, int numberOfPieces, Team currentTeam) {
            this.pieces = pieces;
            numPieceBitboardsPerTeam = numberOfPieces;
            int numTeams = Enum.GetValues(typeof(Team)).Length;
            pieceBitboards = new ulong[numPieceBitboardsPerTeam * numTeams];
            teamBitboards = new ulong[numTeams];
            kings = new King[2];
            foreach (Piece piece in pieces) {
                if (piece == null)
                    continue;
                
                BitboardHelper.AddSquare(ref pieceBitboards[piece.InstanceID + numPieceBitboardsPerTeam * (int)piece.PieceTeam], piece.SquareIndex);
                BitboardHelper.AddSquare(ref teamBitboards[(int)piece.PieceTeam], piece.SquareIndex);
                if (piece is King king) {
                    int kingIndex = (int)king.PieceTeam;
                    if (kings[kingIndex] != null) {
                        Console.WriteLine($"There are too many kings for the {king.PieceTeam} team");
                        Environment.Exit(0);
                        return;
                    }
                    kings[kingIndex] = king;
                }
            }
            this.currentTeam = currentTeam;
            opponentTeam = currentTeam == Team.White ? Team.Black : Team.White;
        }

        private int GetTeamBitboardIndex(Team team) {
            return numPieceBitboardsPerTeam - 1 + (numPieceBitboardsPerTeam * (int)team);
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
            for (int y = dimensions - 1; y >= 0; y--) {
                string row = $"{y + 1} [ ";
                for (int x = 0; x < dimensions; x++) {
                    int pieceIndex = x + y * dimensions;
                    if (pieces[pieceIndex] == null) {
                        row += ". ";
                        continue;
                    } 
                    row += $"{pieces[pieceIndex]} ";
                }
                row += "]";
                Console.WriteLine(row);
            }
            Console.WriteLine("    A B C D E F G H");
        }

        public void PrintAttacks(Team team) {
            // string attacks = Convert.ToString((long)(attackedSquaresBitboard[(int)team] | (1ul << 63)), 2).PadLeft(64, '0');
            string attacks = Convert.ToString((long)attackedSquaresBitboard[(int)team], 2).PadLeft(64, '0');

            for (int i = 0; i < dimensions; i++) {
                string attackedRow = attacks[(i * dimensions) .. ((i + 1) * dimensions)];
                string row = $"{dimensions - i} [ ";
                for (int j = attackedRow.Length - 1; j >= 0; j--) {
                    row += $"{attackedRow[j]} ";
                }
                row += "]";
                Console.WriteLine(row);
            }
            Console.WriteLine("    A B C D E F G H");
        }

        public bool IsPieceAt(int squareIndex) {
            if (SquareOnBoard(squareIndex) && pieces[squareIndex] != null) {
                return true;
            }
            return false;
        }

        public Piece? GetPieceAt(int squareIndex) {
            if (SquareOnBoard(squareIndex)) {
                return pieces[squareIndex];
            }
            return null;
        }

        public T? GetPieceAt<T>(int squareIndex) where T : Piece {
            if (SquareOnBoard(squareIndex)) {
                if (pieces[squareIndex] is T piece) {
                    return piece;
                }
            }
            return null;
        }

        public void MakeMove(Move move) {            
            int startingSquare = move.startingSquare;
            int targetSquare = move.targetSquare;
            
            Pawn? enPassantPawn = null;
            Piece? promotedPawn = null;
            Piece? capturedPiece = pieces[targetSquare];
            
            King whiteKing = GetTeamsKing(Team.White);
            King blackKing = GetTeamsKing(Team.Black);
            string castlingAvailability = whiteKing.GetCastlingString() + blackKing.GetCastlingString();
            if (castlingAvailability.Length == 0)
                castlingAvailability = "-";
                
            if (move.specialMoveType == Move.SpecialMoveType.PushPawnTwoSquares) {
                // it's the starting square because we've not moved the pawn yet
                enPassantPawn = pieces[move.startingSquare] as Pawn;
            }
            else if (move.specialMoveType == Move.SpecialMoveType.EnPassantCapture) {
                // ignore the null warning since the piece can't be null if it enters this if statement
                # nullable disable
                capturedPiece = CurrentEnPassantPawn;
                RemovePieceAt(CurrentEnPassantPawn.SquareIndex);
                # nullable enable
            }
            else if (move.specialMoveType == Move.SpecialMoveType.CastlingQueenside) {
                Piece king = pieces[move.startingSquare];
                Piece rook = pieces[king.SquareIndex - 4];
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
                Piece king = pieces[move.startingSquare];
                Piece rook = pieces[king.SquareIndex + 3];
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
                Piece pawnUpgrading = pieces[move.startingSquare];
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
                    pieces[pawnUpgrading.SquareIndex] = piece;
            }
            Piece pieceToMove = pieces[startingSquare];
            if (capturedPiece != null) {
                RemovePieceInBitboards(capturedPiece);
            }
            MovePiece(pieceToMove, startingSquare, targetSquare);
            GameState newState = new GameState(move, enPassantPawn, capturedPiece, promotedPawn, castlingAvailability, pieceToMove.HasMoved);
            gameStates.Push(newState);
            repetitionTable.Push(FENHandler.GetFENString(this));
            pieceToMove.SetToMoved();
            (currentTeam, opponentTeam) = (opponentTeam, currentTeam);
        }

        public void MovePiece(Piece piece, int startingSquare, int targetSquare) {
            // there is nothing in the start square anymore
            # nullable disable
            pieces[startingSquare] = null;
            # nullable enable
            // the target square now has the piece in it
            pieces[targetSquare] = piece;
            piece.SquareIndex = targetSquare;
            
            
            ref ulong pieceBitboard = ref pieceBitboards[GetPieceBitboardIndex(piece.InstanceID, piece.PieceTeam)];
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
            // int pieceID = PieceHandler.GetPieceID(piece.GetType());
            ref ulong pieceBitboard = ref pieceBitboards[GetPieceBitboardIndex(piece.InstanceID, piece.PieceTeam)];
            ref ulong teamPiecesBitboard = ref teamBitboards[(int)piece.PieceTeam];
            
            BitboardHelper.RemoveSquareFromBitboard(ref pieceBitboard, piece.SquareIndex);
            BitboardHelper.RemoveSquareFromBitboard(ref teamPiecesBitboard, piece.SquareIndex);
        }

        public void AddPieceInBitboards(Piece piece) {
            ref ulong pieceBitboard = ref pieceBitboards[GetPieceBitboardIndex(piece.InstanceID, piece.PieceTeam)];
            ref ulong teamBitboard = ref teamBitboards[(int)piece.PieceTeam];
            BitboardHelper.AddSquare(ref pieceBitboard, piece.SquareIndex);
            BitboardHelper.AddSquare(ref teamBitboard, piece.SquareIndex);
        }

        public void RemovePieceAt(int squareIndex) {
            RemovePieceInBitboards(pieces[squareIndex]);

            # nullable disable
            pieces[squareIndex] = null;
            # nullable enable
        }

        public void AddPieceAt(Piece piece, int squareIndex) {
            pieces[squareIndex] = piece;
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
                Piece king = pieces[previousMove.targetSquare];
                Piece rook = pieces[king.SquareIndex + 1];
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
                Piece king = pieces[previousMove.targetSquare];
                Piece rook = pieces[king.SquareIndex - 1];
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
                Piece promotedPiece = pieces[previousMove.targetSquare];
                RemovePieceInBitboards(promotedPiece);
                pieces[previousMove.targetSquare] = previousState.PromotedPawn;
                AddPieceInBitboards(previousState.PromotedPawn);
            }
            Piece pieceToMove = pieces[previousMove.targetSquare];

            MovePiece(pieceToMove, previousMove.targetSquare, previousMove.startingSquare);
            if (previousState.CapturedPiece != null) {
                AddPieceAt(previousState.CapturedPiece, previousState.CapturedPiece.SquareIndex);
            }
            pieceToMove.SetMoved(previousState.PieceMovedBefore);
            (currentTeam, opponentTeam) = (opponentTeam, currentTeam);
        }

        public static bool SquareOnBoard(int squareIdx) {
            return squareIdx < dimensions * dimensions && squareIdx >= 0;
        }

        public static int NumSquaresToEdgeFromSquare(int squareIdx, int direction) {
            (int xDir, int yDir) = ConvertDirectionToCoord(direction);
            int squareX = squareIdx % dimensions;
            int squareY = squareIdx / dimensions;

            int squareToCheckX = squareX + xDir;
            int squareToCheckY = squareY + yDir;
            int numUntilEdge = 0;
            while (squareToCheckX >= 0 && squareToCheckX < dimensions && squareToCheckY >= 0 && squareToCheckY < dimensions) {
                squareToCheckX += xDir;
                squareToCheckY += yDir;
                numUntilEdge++;
            }

            return numUntilEdge;
        }

        public static bool TryGetNewSquareFromCurrentSquare(int squareIndex, int xDirection, int yDirection, out int newSquareIndex) {
            Coordinate squareCoord = ConvertSquareIndexToCoord(squareIndex);
            int newX = squareCoord.x + xDirection;
            int newY = squareCoord.y + yDirection;
            if (newX < 0 || newX >= dimensions || newY < 0 || newY >= dimensions) {
                newSquareIndex = -1;
                return false;
            }
            newSquareIndex = ConvertCoordToSquareIndex(newX, newY);
            return true;
        }

        public static (int, int) ConvertDirectionToCoord(int direction) {
            int unsignedDir = Math.Abs(direction);
            if (unsignedDir == dimensions - 1)
                return direction < 0 ? (1, -1) : (-1, 1);
            else if (unsignedDir == dimensions + 1)
                return direction < 0 ? (-1, -1) : (1, 1);
            else if (unsignedDir == dimensions)
                return direction < 0 ? (0, -1) : (0, 1);
            else if (unsignedDir == 1)
                return direction < 0 ? (-1, 0) : (1, 0);
            return (0, 0);
        }
        
        public static Coordinate ConvertSquareIndexToCoord(int squareIndex) {
            int x = squareIndex % dimensions;
            int y = squareIndex / dimensions;

            return new Coordinate(x, y);
        }

        public static int ConvertCoordToSquareIndex(int x, int y) {
            return x + y * dimensions;
        }

        public static int ConvertChessNotationToSquare(string squareInChessNotation) {
            if (squareInChessNotation.Length != 2) 
                return -1;
            char columnInChessNotation = squareInChessNotation.ToLower()[0];
            int column;
            switch (columnInChessNotation) {
                case 'a':
                    column = 0;
                    break;
                case 'b':
                    column = 1;
                    break;
                case 'c':
                    column = 2;
                    break;
                case 'd':
                    column = 3;
                    break;
                case 'e':
                    column = 4;
                    break;
                case 'f':
                    column = 5;
                    break;
                case 'g':
                    column = 6;
                    break;
                case 'h':
                    column = 7;
                    break;
                default:
                    return -1;
            }

            if (!char.IsNumber(squareInChessNotation[1]))
                return -1;
            int row = int.Parse(squareInChessNotation[1].ToString()) - 1;
            return column + row * dimensions;
        }

        public static string ConvertSquareToChessNotation(int squareIndex) {
            if (!SquareOnBoard(squareIndex))
                return string.Empty;
            int x = squareIndex % dimensions;
            int y = squareIndex / dimensions;

            string chessNotation = string.Empty;

            switch (x) {
                case 0:
                    chessNotation += "a";
                    break;
                case 1:
                    chessNotation += "b";
                    break;
                case 2:
                    chessNotation += "c";
                    break;
                case 3:
                    chessNotation += "d";
                    break;
                case 4:
                    chessNotation += "e";
                    break;
                case 5:
                    chessNotation += "f";
                    break;
                case 6:
                    chessNotation += "g";
                    break;
                case 7:
                    chessNotation += "h";
                    break;
            }

            return chessNotation + (y + 1);
        }

        public static bool IsSameFile(int squareIndexOne, int squareIndexTwo) {
            int squareOneRank = squareIndexOne % dimensions;
            int squareTwoRank = squareIndexTwo % dimensions;

            return squareOneRank == squareTwoRank;
        }

        public static bool IsSameRank(int squareIndexOne, int squareIndexTwo) {
            int squareOneRank = squareIndexOne / dimensions;
            int squareTwoRank = squareIndexTwo / dimensions;

            return squareOneRank == squareTwoRank;
        }

        public static int GetRank(int squareIndex) {
            return squareIndex / dimensions;
        }

        public static int GetFile(int squareIndex) {
            return squareIndex % dimensions;
        }

        public King GetTeamsKing(Team team) {
            return kings[(int)team];
        }
        
        public ulong GetSquaresAttackedByNextTeam(Team team) {
            return attackedSquaresBitboard[((int)team + 1) % attackedSquaresBitboard.Length];
        }

        public King GetOpposingTeamsKing(Team myTeam) {
            return kings[(int)GetOpposingTeam(myTeam)];
        }


        // compares the attacking squares of the other team against where the king is trying to move to
        // if the result is not a 0 then the king has stepped into a square that is being attacked and would therefore
        // be illegal since the king would be checked after the turn
        // e.g. if the king is trying to move to E4 (or square inbdex 28) then this would be the king's integer:
        // 0000000000000000000000000000000000010000000000000000000000000000
        // and let's say the attacking bitboard looks like this:
        // 0001101100001111000001010000111100011011001000100100001010001010
        // we use the AND operator to combine the bits together getting only 1's when there's a 1 present in both
        // the combined bitboard is as follows:
        // 0000000000000000000000000000000000010000000000000000000000000000
        // and because there's still a 1 present, the king would be in check here
        public bool IsKingSafeOnSquare(Team kingsTeam, int squareIndex) {
            // Console.WriteLine();
            // Console.WriteLine(Convert.ToString((long)GetSquaresAttackedByNextTeam(kingsTeam), 2).PadLeft(64, '0'));
            // Console.WriteLine(Convert.ToString((long)1ul << squareIndex, 2).PadLeft(64, '0'));
            // return (GetSquaresAttackedByNextTeam(kingsTeam) & (1ul << squareIndex)) == 0;
            return !BitboardHelper.BitboardContainsSquare(GetSquaresAttackedByNextTeam(kingsTeam), squareIndex);
        }

        public void AddAttackedSquare(Team pieceTeam, int squareIndex) {
            // Console.WriteLine($"Adding square attacked: {squareIndex} for {pieceTeam}");
            attackedSquaresBitboard[(int)pieceTeam] |= 1ul << squareIndex;
        }
        
        public void AddAttacks(Team pieceTeam, ulong attacksBitboard) {
            attackedSquaresBitboard[(int)pieceTeam] |= attacksBitboard;
        }
        
        public void PinPiece(int squareIndex, ulong pinBitboard) {
            pieces[squareIndex].Pin(pinBitboard);
        }
        
        public void UnpinAllPinnedPieces() {
            ulong currentTeamsPieces = GetTeamBitboard(currentTeam);
            while (currentTeamsPieces != 0) {
                int squareIndex = BitboardHelper.PopLeastSignificantBit(ref currentTeamsPieces);
                pieces[squareIndex].Unpin();
            }
        }

        public void ClearAttackedSquares(Team pieceTeam) {
            attackedSquaresBitboard[(int)pieceTeam] = 0;
        }

        public static bool SquareIsInDirection(int startingSquare, int targetSquare, int direction) {
            Coordinate startingCoord = ConvertSquareIndexToCoord(startingSquare);
            Coordinate targetCoord = ConvertSquareIndexToCoord(targetSquare);
            
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
                    Coordinate coord = Board.ConvertSquareIndexToCoord(squareIdx);
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
}