
using System.Data.Common;

namespace Chess_Application {
    public class King : Piece {
        public override int InstanceID => (int)BitboardIndexes.KingIndex;
        public ulong checkBitboard;
        public bool isChecked;
        public bool isDoubleChecked;
        public bool canCastleQueenside;
        public bool canCastleKingside;
        private readonly int[] directions = CompassDirections.CardinalsAndDiagonals;
        
        public King(Team team, int squareIndex) : base(team, squareIndex) {
            isChecked = false;
            isDoubleChecked = false;
            canCastleQueenside = false;
            canCastleKingside = false;
        }

        protected override void HandleMoveGeneration(Board board, Span<Move> moves, ref int movesCount, ulong capturesOnlyMask) {
            ulong movesBitboard = MoveData.KingAttacks[SquareIndex];
            
            ulong friendlyPiecesBitboard = board.GetTeamBitboard(PieceTeam);
            ulong allPiecesBitboard = board.GetAllPiecesBitboard();
            
            // only including moves that result in the king being on a safe square
            movesBitboard &= ~board.GetSquaresAttackedByNextTeam(PieceTeam);
            // remove any moves that would capture friendly pieces
            movesBitboard &= ~friendlyPiecesBitboard;
            // determines whether the moves should be only captures or whether we can include quiet moves
            movesBitboard &= capturesOnlyMask;
            
            while (movesBitboard != 0) {
                int targetSquare = BitboardHelper.PopLeastSignificantBit(ref movesBitboard);
                moves[movesCount++] = new Move(SquareIndex, targetSquare);
            }

            AddCastlingMoves(board, moves, ref movesCount, capturesOnlyMask != ulong.MaxValue, allPiecesBitboard);
        }

        private void AddCastlingMoves(Board board, Span<Move> moves, ref int movesCount, bool capturesOnly, ulong allPiecesBitboard) {
            // since castling isn't a capturing move, can return early
            // and castling is only available if the king hasn't moved and if it's not in check
            if (capturesOnly || HasMoved || isChecked)
                return;
            ulong friendlyRookBitboard = board.GetPieceBitboard(BitboardIndexes.RookIndex, PieceTeam);
            Coordinate kingCoord = Board.ConvertSquareIndexToCoord(SquareIndex);
            if (canCastleQueenside) {
                ulong spacesBetweenKingAndRook = 0b00001110ul << (kingCoord.y * 8);
                ulong affectedSquaresByAttacks = 0b00001100ul << (kingCoord.y * 8);
                if ((board.GetSquaresAttackedByNextTeam(PieceTeam) & affectedSquaresByAttacks) == 0) {
                    if (CanCastleOnSide(board, spacesBetweenKingAndRook, allPiecesBitboard, friendlyRookBitboard, 0 + kingCoord.y * 8)) {
                        moves[movesCount++] = new Move(SquareIndex, SquareIndex - 2, Move.SpecialMoveType.CastlingQueenside);
                    }
                }
            }
            if (canCastleKingside) {
                ulong spacesBetweenKingAndRook = 0b01100000ul << (kingCoord.y * 8);
                ulong affectedSquaresByAttacks = spacesBetweenKingAndRook;
                if ((board.GetSquaresAttackedByNextTeam(PieceTeam) & affectedSquaresByAttacks) == 0) {
                    if (CanCastleOnSide(board, spacesBetweenKingAndRook, allPiecesBitboard, friendlyRookBitboard, 7 + kingCoord.y * 8)) {
                        moves[movesCount++] = new Move(SquareIndex, SquareIndex + 2, Move.SpecialMoveType.CastlingKingside);
                    }
                }
            }
        }

        private bool CanCastleOnSide(Board board, ulong spacesBetweenKingAndRook, ulong allPiecesBitboard, ulong friendlyRookBitboard, int rookIntendedPosition) {
            // there are pieces between the king and the rook
            if ((spacesBetweenKingAndRook & allPiecesBitboard) != 0)
                return false;
            // the rook isn't in the position
            if (((1ul << rookIntendedPosition) & friendlyRookBitboard) == 0)
                return false;
            
            # nullable disable
            Rook castlingRook = board.pieces[rookIntendedPosition] as Rook;
            if (castlingRook.HasMoved)
                return false;
            return true;
            # nullable enable
        }
        
        public void CalculateSlidingChecksAndPins(Board board, ulong cardinalSliders, ulong diagonalSliders, ulong friendlyPiecesBitboard, ulong opponentPiecesBitboard) {
            for (int dirIndex = 0; dirIndex < CompassDirections.CardinalsAndDiagonals.Length; dirIndex++) {
                // don't have to check any other pins or checks if the king is already double checked
                // since the king can only move in this case
                if (isDoubleChecked)
                    break;
                
                int direction = CompassDirections.CardinalsAndDiagonals[dirIndex];
                int numSquaresToEdge = Board.NumSquaresToEdgeInDirection[SquareIndex][dirIndex];
                ulong opponentSliders = cardinalSliders;
                if (CompassDirections.IsDiagonalDirection(direction))
                    opponentSliders = diagonalSliders;
                    
                if (opponentSliders == 0)
                    continue;
                
                ulong directionMask = 0;
                int friendlyBlockerSquareIndex = -1;
                for (int i = 1; i <= numSquaresToEdge; i++) {
                    int newSquareIndex = SquareIndex + direction * i;
                    ulong newSquareMask = 1ul << newSquareIndex;
                    directionMask |= newSquareMask;
                    
                    // there's a friendly piece in the way
                    if ((friendlyPiecesBitboard & newSquareMask) != 0) {
                        // we've already come across a friendly piece so a pin isn't possible
                        if (friendlyBlockerSquareIndex >= 0)
                            break;
                        
                        friendlyBlockerSquareIndex = newSquareIndex;
                    }
                    // there's an opponent piece
                    else if ((opponentPiecesBitboard & newSquareMask) != 0) {
                        // ignore if the opponent piece isn't a sliding piece
                        if ((opponentSliders & newSquareMask) == 0)
                            break;

                        // there is a blocker so it needs pinning
                        if (friendlyBlockerSquareIndex >= 0) {
                            board.PinPiece(friendlyBlockerSquareIndex, directionMask);
                            break;
                        }
                        // there is no blocker so the king is exposed
                        Check(directionMask);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets knight and pawn attacks, handling checks and pins caused by knights or pawns
        /// </summary>
        /// <param name="friendlyPiecesBitboard">The bitboard that is of the same team as this king</param>
        /// <param name="opponentPiecesBitboard">The bitboard that is of the opposite team as this king</param>
        public void CalculatePawnKnightAttacks(Board board, Team opponentTeamOfKing) {
            ulong kingBitboard = 1ul << SquareIndex;
            ulong allKnightAttacks = 0;
            // knight attacks on the king
            ulong opponentKnights = board.GetPieceBitboard(BitboardIndexes.KnightIndex, board.opponentTeam);
            while (opponentKnights != 0) {
                int knightSquareIndex = BitboardHelper.PopLeastSignificantBit(ref opponentKnights);
                ulong knightAttacks = MoveData.KnightAttacks[knightSquareIndex];
                allKnightAttacks |= knightAttacks;
                
                if ((knightAttacks & kingBitboard) != 0) {
                    Check(knightSquareIndex);
                }
            }
            
            // pawn attacks on the king
            ulong opponentPawns = board.GetPieceBitboard((int)BitboardIndexes.PawnIndex, board.opponentTeam);
            ulong allPawnAttacks = 0;
            while (opponentPawns != 0) {
                int pawnSquareIndex = BitboardHelper.PopLeastSignificantBit(ref opponentPawns);
                ulong pawnAttacks = MoveData.PawnAttacks[SquareIndex][(int)board.opponentTeam];
                allPawnAttacks |= pawnAttacks;
                
                if ((pawnAttacks & kingBitboard) != 0) {
                    Check(pawnSquareIndex);
                }
            }
            
            if (!isChecked)
                checkBitboard = ulong.MaxValue;
            
            board.AddAttacks(opponentTeamOfKing, allKnightAttacks | allPawnAttacks);
        }
        
        public string GetCastlingString() {
            string castlingRights = string.Empty;
            if (canCastleKingside)
                castlingRights += ToString();
            if (canCastleQueenside)
                castlingRights += PieceTeam == Team.White ? "Q" : "q";
            return castlingRights;
        }

        public override void GenerateSquaresAttacked(Board board, King opponentKing) {
            ulong attacks = MoveData.KingAttacks[SquareIndex];
            board.AddAttacks(PieceTeam, attacks);
        }

        public override void GenerateSquaresAttackedImproved(Board board, King _) {
            for (int i = 0; i < directions.Length; i++) {
                int targetSquare = SquareIndex + directions[i];
                if (Board.NumSquaresToEdgeFromSquare(SquareIndex, directions[i]) <= 0)
                    continue;
                // puts a 1 where the king is attacking a square
                board.AddAttackedSquare(PieceTeam, targetSquare);
            }
        }

        public void Check(int squareIndexChecking) {
            isDoubleChecked = isChecked;
            isChecked = true;
            // BitboardHelper.AddSquare(ref checkBitboard, squareIndexChecking);
            checkBitboard |= 1ul << squareIndexChecking;
        }

        public void Check(ulong bitboardForSquaresChecking) {
            isDoubleChecked = isChecked;
            isChecked = true;
            // BitboardHelper.AddBitboard(ref checkBitboard, bitboardForSquaresChecking);
            checkBitboard |= bitboardForSquaresChecking;
        }

        public void Uncheck() {
            isChecked = false;
            isDoubleChecked = false;
            checkBitboard = 0;
        }

        public override string ToString() {
            return PieceTeam == Team.White ? "K" : "k";
        }
    }
}