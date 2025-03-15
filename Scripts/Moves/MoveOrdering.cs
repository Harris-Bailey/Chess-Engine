namespace Chess;

public class MoveOrdering {
    IEvaluation evaluation;

    public MoveOrdering(IEvaluation evaluation) {
        this.evaluation = evaluation;
    }

    const int valueMultiplier = 1_000_000;
    const int winningCaptureValue = 8 * valueMultiplier;
    const int losingCaptureValue = 2 * valueMultiplier;
    const int promoteValue = 6 * valueMultiplier;


    public void OrderMoves(Board board, Move[] moves, bool debug = false) {
        int[] scores = new int[moves.Length];
        ulong opponentPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, board.opponentTeam);

        for (int i = 0; i < moves.Length; i++) {
            Move move = moves[i];
            int score = 0;
            Piece piece = board.pieces[move.startingSquare];
            Piece? capturedPiece = board.pieces[move.targetSquare];
            ulong opponentAttacks = board.GetSquaresAttackedByNextTeam(board.currentTeam);

            if (capturedPiece != null) {
                int capturingScore = GetPieceValue(capturedPiece) - GetPieceValue(piece);
                score += capturingScore;
                bool canRecapture = BitboardHelper.BitboardContainsSquare(opponentAttacks, move.targetSquare);
                if (debug && move.ToString() == "d5e6") {
                    Console.WriteLine(canRecapture);
                    BitboardHelper.PrintBitboard(opponentAttacks, 8);
                    Console.WriteLine(capturingScore);
                }
                if (canRecapture) {
                    score += capturingScore > 0 ? winningCaptureValue : losingCaptureValue;
                }
                else {
                    score += winningCaptureValue;
                }
                if (debug) {
                    Console.WriteLine($"{score} score for {move} move");
                }
            }

            if (move.IsPromotion) {
                score += promoteValue;
            }
            else if (piece is not King) {
                // Sebastian uses the tables for the position of the pieces
                // int scoreOfCurrentPosition = PieceData.GetScoreFromSquare(piece, move.startingSquare);
                // int scoreOfNewPosition = PieceData.GetScoreFromSquare(piece, move.targetSquare);
                // score += scoreOfNewPosition - scoreOfCurrentPosition;

                // if the target square is attacked by an enemy pawn, apply a penalty
                if (BitboardHelper.BitboardContainsSquare(opponentPawnBitboard, move.targetSquare)) {
                    score -= 50;
                }
                // otherwise if the target square is attacked by any other enemy piece, apply a lower penalty
                else if (BitboardHelper.BitboardContainsSquare(opponentAttacks, move.targetSquare)) {
                    score -= 25;
                }
            }
            scores[i] = score;
        }

        Quicksort(moves, scores, 0, moves.Length - 1);
        // Array.Sort(scores, moves, Comparer<int>.Create((a, b) => a - b));

        // Console.WriteLine();
        // for (int i = 0; i < scores.Length; i++) {
        //     int score = scores[i];
        //     Move move = moves[i];
        //     Console.WriteLine($"{score} score for {move}");
        // }
    }

    private void Quicksort(Move[] moves, int[] scores, int low, int high) {
        if (low > high)
            return;
        int partitionIndex = Partition(moves, scores, low, high);
        Quicksort(moves, scores, low, partitionIndex - 1);
        Quicksort(moves, scores, partitionIndex + 1, high);

    }

    private int Partition(Move[] moves, int[] scores, int low, int high) {
        int pivot = scores[high];

        int i = low - 1;

        for (int j = low; j <= high; j++) {
            if (scores[j] > pivot) {
                i++;
                (scores[i], scores[j]) = (scores[j], scores[i]);
                (moves[i], moves[j]) = (moves[j], moves[i]);
            }
        }
        (scores[i + 1], scores[high]) = (scores[high], scores[i + 1]);
        (moves[i + 1], moves[high]) = (moves[high], moves[i + 1]);
        return i + 1;
    }

    private int GetPieceValue(Piece piece) {
        return piece switch {
            Queen => evaluation.queenValue,
            Rook => evaluation.rookValue,
            Bishop => evaluation.bishopValue,
            Knight => evaluation.knightValue,
            Pawn => evaluation.pawnValue,
            // the default includes the king since we want to see whether the other pieces can capture the
            // piece first and then the last resort is check for whether the king can capture if it's able to
            _ => 0,
        };
    }
}