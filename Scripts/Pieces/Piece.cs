namespace Chess;
 
public abstract class Piece {
    public int NumMoves { get; protected set; }
    public bool IsPinned { get; private set; }
    public readonly Team PieceTeam;
    public int SquareIndex;
    public bool HasMoved { get; private set; }
    public abstract int ClassID { get; }
    private int movesArrayStartIndex;
    private bool hasMovedBeforeStartup;
    protected ulong pinBitboard;

    public Piece(Team team, int squareIndex) {
        this.PieceTeam = team;
        this.SquareIndex = squareIndex;
        HasMoved = false;
        hasMovedBeforeStartup = false;
    }

    public void GenerateMoves(Board board, Span<Move> moves, ref int movesCount, ulong capturesOnlyMask) {
        int initialNumMoves = movesCount;
        movesArrayStartIndex = initialNumMoves;
        HandleMoveGeneration(board, moves, ref movesCount, capturesOnlyMask);
        NumMoves = movesCount - initialNumMoves;
    }
    protected abstract void HandleMoveGeneration(Board board, Span<Move> moves, ref int movesCount, ulong capturesOnlyMask);
    public abstract void GenerateSquaresAttacked(Board board, King opponentKing);

    protected bool CanAddMove(Board board, Move move, bool capturesOnly, bool canCaptureOnTargetSquare = true) {
        int targetSquare = move.targetSquare;
        if (!Board.SquareOnBoard(targetSquare))
            return false;
        Piece? capturingPiece = board.Pieces[targetSquare];
        if (capturingPiece != null) {
            if (capturingPiece != null && capturingPiece.PieceTeam == PieceTeam) {
                return false;
            }
            if (!canCaptureOnTargetSquare && capturingPiece != null) {
                return false;
            }
        }
        else if (capturesOnly)
            return false;
            
        // if the piece is pinned then it can only move to squares that don't result in the king being checked
        // so it can only move to the target square if it's part of the allowed pinned squares
        if (IsPinned && !BitboardHelper.BitboardContainsSquare(pinBitboard, targetSquare)) {
            return false;
        }
        // this checks whether moving a piece resolves a check if moving to the target square
        // and can only move to the target square if it does
        King teamsKing = board.GetTeamsKing(PieceTeam);
        if (this is not King && teamsKing.isChecked) {
            if (!BitboardHelper.BitboardContainsSquare(teamsKing.checkBitboard, targetSquare)) {
                return false;
            }
        }
        return true;
    }

    public void Pin(ulong pinBitboardToAppend) {
        BitboardHelper.AddBitboard(ref pinBitboard, pinBitboardToAppend);
        IsPinned = true;
    }

    public void Unpin() {
        pinBitboard = 0;
        IsPinned = false;
    }

    public void SetToMoved(bool callingOnStartup = false) {
        HasMoved = true;
        if (callingOnStartup)
            hasMovedBeforeStartup = true;
    }

    public void SetToNotMoved() {
        if (hasMovedBeforeStartup)
            return;
        HasMoved = false;
    }
    
    public void SetMoved(bool hasMoved) {
        HasMoved = hasMoved;
    }

    public Move GetMove(Move[] moves, int moveIndex) {
        if (moveIndex >= NumMoves || moveIndex < 0)
            return Move.NullMove;
        return moves[movesArrayStartIndex + moveIndex];
    }
    
    public abstract override string ToString();
}