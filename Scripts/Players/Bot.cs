namespace Chess;

public abstract class Bot {
    protected Board board { get; private set; }
    protected MoveGenerator moveGenerator { get; private set; }
    protected Action<Move> onMoveChosen { get; private set; }
    private CancellationToken? cancellationToken;

    public Bot(Board board, MoveGenerator moveGenerator, Action<Move> onMoveChosen) {
        this.board = board;
        this.moveGenerator = moveGenerator;
        this.onMoveChosen += onMoveChosen;
    }

    public abstract void StartProcessing();
    public abstract void ResetBot();

    public void CancelSearch() {
        
    }
}