using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Chess_Application {
    public readonly struct Move {

        public enum SpecialMoveType {
            None,
            PushPawnTwoSquares,
            EnPassantCapture,
            CastlingKingside,
            CastlingQueenside,
            PromoteToQueen,
            PromoteToRook,
            PromoteToKnight,
            PromoteToBishop
        }

        public readonly int startingSquare;
        public readonly int targetSquare;
        public readonly SpecialMoveType specialMoveType;
        public readonly bool isNullMove = false;
        public readonly bool IsPromotion => specialMoveType >= SpecialMoveType.PromoteToQueen;

        public Move() {
            startingSquare = 0;
            targetSquare = 0;
            isNullMove = true;
        }
        
        public Move(int startingSquare, int targetSquare) {
            this.startingSquare = startingSquare;
            this.targetSquare = targetSquare;
            this.specialMoveType = SpecialMoveType.None;
        }

        public Move(int startingSquare, int targetSquare, SpecialMoveType flag) {
            this.startingSquare = startingSquare;
            this.targetSquare = targetSquare;
            this.specialMoveType = flag;
        }

        public Move(string moveInChessNotation) {
            startingSquare = Board.ConvertChessNotationToSquare(moveInChessNotation[0..2]);
            targetSquare = Board.ConvertChessNotationToSquare(moveInChessNotation[2..4]);

            if (startingSquare == -1 || targetSquare == -1) {
                this = NullMove;
                return;
            }
            
            if (moveInChessNotation.Length > 4) {
                switch (moveInChessNotation[4..]) {
                    case "q":
                    case "=q":
                        specialMoveType = SpecialMoveType.PromoteToQueen;
                        break;
                    case "r":
                    case "=r":
                        specialMoveType = SpecialMoveType.PromoteToRook;
                        break;
                    case "b":
                    case "=b":
                        specialMoveType = SpecialMoveType.PromoteToBishop;
                        break;
                    case "n":
                    case "=n":
                        specialMoveType = SpecialMoveType.PromoteToKnight;
                        break;
                }
            }
        }

        public override string ToString() {
            return $"{Board.ConvertSquareToChessNotation(startingSquare)}{Board.ConvertSquareToChessNotation(targetSquare)}{GetPromotionString()}";
        }

        private string GetPromotionString() {
            switch (specialMoveType) {
                case SpecialMoveType.PromoteToQueen:
                    return "=q";
                case SpecialMoveType.PromoteToRook:
                    return "=r";
                case SpecialMoveType.PromoteToBishop:
                    return "=b";
                case SpecialMoveType.PromoteToKnight:
                    return "=n";
                default:
                    return "";
            }
        }

        public static bool operator ==(Move move1, Move move2) {
            return move1.startingSquare == move2.startingSquare && move1.targetSquare == move2.targetSquare && !move1.isNullMove && !move2.isNullMove && move1.specialMoveType == move2.specialMoveType;
        }

        public static bool operator !=(Move move1, Move move2) {
            return !(move1 == move2);
        }

        public override readonly bool Equals([NotNullWhen(true)] object? obj) {
            if (obj is not Move move)
                return false;
            return this == move;
        }

        public override readonly int GetHashCode() {
            return base.GetHashCode();
        }

        public static Move NullMove => new Move();
    }
}