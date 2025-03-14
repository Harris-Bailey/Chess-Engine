namespace Chess_Application {
    public struct GameState {
        public readonly Move MovePlayed;
        public readonly Pawn? PawnCapturableByEnPassant;
        public readonly Piece? CapturedPiece;
        public readonly Piece? PromotedPawn;
        public readonly string CastlingAvailability;
        public readonly bool PieceMovedBefore;

        public GameState(
            Move movePlayed,
            Pawn? pawnCapturableByEnPassant,
            string castlingAvailability,
            bool pieceMovedBefore
        ) {
            MovePlayed = movePlayed;
            PawnCapturableByEnPassant = pawnCapturableByEnPassant;
            CapturedPiece = null;
            CastlingAvailability = castlingAvailability;
            PieceMovedBefore = pieceMovedBefore;
        }

        public GameState(
            Move movePlayed,
            Pawn? pawnCapturableByEnPassant,
            Piece? capturedPiece,
            Piece? promotedPawn,
            string castlingAvailability,
            bool pieceMovedBefore
        ) {
            MovePlayed = movePlayed;
            PawnCapturableByEnPassant = pawnCapturableByEnPassant;
            CapturedPiece = capturedPiece;
            PromotedPawn = promotedPawn;
            CastlingAvailability = castlingAvailability;
            PieceMovedBefore = pieceMovedBefore;
        }
    }
}