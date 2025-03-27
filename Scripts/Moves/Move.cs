using System.Diagnostics.CodeAnalysis;

namespace Chess;

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
    
    public static Move NullMove => new Move();
    
    public static int MoveNotationToSquare(string moveNotation) {
        if (moveNotation.Length != 2) 
            return -1;
        if (!char.IsNumber(moveNotation[1]))
            return -1;

        char columnInChessNotation = moveNotation.ToLower()[0];
        int ASCIIStartRowIndex = Convert.ToInt32('a');
        int columnIndex = Convert.ToInt32(columnInChessNotation) - ASCIIStartRowIndex;
        
        int rowIndex = int.Parse(moveNotation[1].ToString()) - 1;
        return columnIndex + rowIndex * Board.Dimensions;
    }
    
    public static string SquareToMoveNotation(int squareIndex) {
        if (!Board.SquareOnBoard(squareIndex))
            return string.Empty;
        Coordinate squareCoord = new Coordinate(squareIndex);
        
        int ASCIIStartRowIndex = Convert.ToInt32('a');
        char columnLetter = Convert.ToChar(squareCoord.x + ASCIIStartRowIndex);
        
        return $"{columnLetter}{squareCoord.y}";
    }

    private readonly string GetPromotionString() {
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
                return string.Empty;
        }
    }

    public static bool operator ==(Move move1, Move move2) {
        if (move1.isNullMove || move2.isNullMove)
            return false;
        return move1.startingSquare == move2.startingSquare && move1.targetSquare == move2.targetSquare && move1.specialMoveType == move2.specialMoveType;
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
    
    public override readonly string ToString() {
        return $"{SquareToMoveNotation(startingSquare)}{SquareToMoveNotation(targetSquare)}{GetPromotionString()}";
    }
}