namespace Chess_Application {
    public class Bishop : SlidingPiece {
        public override int InstanceID => (int)BitboardIndexes.BishopIndex;
        protected override int[] directions { get; set; } = CompassDirections.Diagonals;
        protected override bool canMoveDiagonally => true;
        protected override bool canMoveCardinally => false;
        
        public Bishop(Team team, int squareIndex) : base(team, squareIndex) { }


        // protected override void HandleMoveGeneration(Board board, Span<Move> moves, ref int movesCount, bool capturesOnly) {
        //     for (int i = 0; i < Directions.Length; i++) {
        //         int numSquaresToEdge = Board.NumSquaresToEdgeFromSquare(squareIndex, Directions[i]);
        //         for (int iterations = 1; iterations <= numSquaresToEdge; iterations++) {
        //             int targetSquare = squareIndex + (Directions[i] * iterations);
        //             Move move = new Move(squareIndex, targetSquare);
        //             // TryAddMove(board, move, moves, movesCount);
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
            
        }

        public override string ToString() {
            return PieceTeam == Team.White ? "B" : "b";
        }
    }
}