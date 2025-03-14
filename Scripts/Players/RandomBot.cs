namespace Chess;
 
public class RandomBot : Bot {
    private Random rand;

    public RandomBot(Board board, MoveGenerator moveGenerator, Action<Move> onMoveChosen) : base(board, moveGenerator, onMoveChosen) {
        rand = new Random();
    }

    public override void StartProcessing() {
        Move[] moves = moveGenerator.UpdateAllPieces();
        Move randomMove = moves[rand.Next(0, moves.Length)];
        onMoveChosen.Invoke(randomMove);
    }

    public override void ResetBot() {
        // don't have to do anything for this specific bot
    }
}