

namespace Chess_Application {
	public class Rook : SlidingPiece {
        public override int InstanceID => (int)BitboardIndexes.RookIndex;
        protected override int[] directions { get; set; } = CompassDirections.Cardinals;
        protected override bool canMoveDiagonally => false;
        protected override bool canMoveCardinally => true;
        
		public Rook(Team team, int squareIndex) : base(team, squareIndex) { }

        // protected override void HandleMoveGeneration(Board board, Span<Move> moves, ref int movesCount, bool capturesOnly) {
        // 	for (int i = 0; i < Directions.Length; i++) {
        //         int numSquaresToEdge = Board.NumSquaresToEdgeFromSquare(squareIndex, Directions[i]);
        //         for (int iterations = 1; iterations <= numSquaresToEdge; iterations++) {
        //             int targetSquare = squareIndex + (Directions[i] * iterations);
        //             // TryAddMove(board, new Move(squareIndex, targetSquare));
        //             Move move = new Move(squareIndex, targetSquare);
        //             if (CanAddMove(board, move, capturesOnly)) {
        //                 moves[movesCount++] = move;
        //             }
        //             if (!CanProceedInDirection(board, targetSquare, false)) {
        //                 break;
        //             }
        //         }
        //     }
        // }

        public override void GenerateSquaresAttackedImproved(Board board, King opponentKing) {
            for (int i = 0; i < directions.Length; i++) {
                ulong squaresSearchedBitboard = 0;
                Piece? potentialPinnedPiece = null;
                bool checkingEnPassantIsIllegal = false;
                bool continueAddingAttackedSquares = true;
                bool continueSearchingForChecksAndPins = Board.SquareIsInDirection(SquareIndex, opponentKing.SquareIndex, directions[i]);

                int numSquaresToEdge = Board.NumSquaresToEdgeFromSquare(SquareIndex, directions[i]);
                for (int iterations = 1; iterations <= numSquaresToEdge; iterations++) {
                    if (!continueAddingAttackedSquares && !continueSearchingForChecksAndPins)
                        break;
                    int squareToSearch = SquareIndex + (directions[i] * iterations);
                    if (continueAddingAttackedSquares) {
                        board.AddAttackedSquare(PieceTeam, squareToSearch);
                    }
                    if (continueAddingAttackedSquares && !CanProceedInDirection(board, squareToSearch, true)) {
                        continueAddingAttackedSquares = false;
                        // break;
                    }
                    // continue;
                    // need to pass a bool into this method to keep a track of whether the king is double checked

                    if (!continueAddingAttackedSquares && !continueSearchingForChecksAndPins)
                        break;

                    if (!continueSearchingForChecksAndPins || opponentKing.isDoubleChecked)
                        continue;
                    // Console.WriteLine("searching in rook...");
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
                        if (firstPawn.PieceTeam != PieceTeam && board.CurrentEnPassantPawn == secondPawn) {
                            potentialPinnedPiece = firstPawn;
                        }
                        else if (secondPawn.PieceTeam != PieceTeam && board.CurrentEnPassantPawn == firstPawn) {
                            potentialPinnedPiece = secondPawn;
                        }
                        // if none are an en passant pawn or the en passant pawn is on our team
                        else {
                            continueSearchingForChecksAndPins = false;
                        }
                    }
                    else if (piece.PieceTeam != PieceTeam) {
                        // can check the king
                        if (piece is King king) {
                            BitboardHelper.RemoveSquareFromBitboard(ref squaresSearchedBitboard, squareToSearch);
                            BitboardHelper.AddSquare(ref squaresSearchedBitboard, SquareIndex);
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
                    else if (piece.PieceTeam == PieceTeam) {
                        continueSearchingForChecksAndPins = false;
                    }

                    if (!continueAddingAttackedSquares && !continueSearchingForChecksAndPins)
                        break;
                }
            }
        }

        public override string ToString() {
			return PieceTeam == Team.White ? "R" : "r";
		}
	}
}