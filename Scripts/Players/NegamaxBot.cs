using System.Runtime.InteropServices;

namespace Chess;
 
public class NegamaxBot : Bot {
    private int maxDepth;
    private IEvaluation evaluation;
    private Move bestMove;
    private MoveOrdering orderer;
    private Dictionary<string, int> repetitionTable;

    private const int InitialAlpha = 99_999_999;
    private const int InitialBeta = -InitialAlpha;
    private const int checkmateValue = 1_000_000;

    
    public NegamaxBot(Board board, MoveGenerator moveGenerator, Action<Move> onMoveChosen, int maxDepth, IEvaluation evaluation) : base(board, moveGenerator, onMoveChosen) {        
        this.maxDepth = maxDepth;
        this.evaluation = evaluation;
        orderer = new MoveOrdering(evaluation);
        repetitionTable = new Dictionary<string, int>();
    }

    public override void StartProcessing() {
        StartAlgorithm();
    }
    
    public override void ResetBot() {
        repetitionTable.Clear();
    }
    
    private void InitialiseRepetitionTable() {
        repetitionTable.Clear();
        string[] boardRepetitionTable = board.repetitionTable.ToArray();
        foreach (string state in boardRepetitionTable) {
            ref int repetitionsOfState = ref CollectionsMarshal.GetValueRefOrAddDefault(repetitionTable, state, out _);
            repetitionsOfState++;
        }
    }

    private void StartAlgorithm() {
        InitialiseRepetitionTable();
        
        Search(0, maxDepth, InitialBeta, InitialAlpha);
        
        if (bestMove.isNullMove) {
            Move[] moves = moveGenerator.UpdateAllPieces();
            
            if (moves.Length > 0)
                bestMove = moves[0];
        }
        
        onMoveChosen.Invoke(bestMove);
        bestMove = Move.NullMove;
    }

    private int Search(int depth, int maxDepth, int alpha, int beta) {
        if (depth == maxDepth) {
            int evaluation = QuiescenceSearch(alpha, beta);
            return evaluation;
        }
        
        King teamsKing = board.GetTeamsKing(board.currentTeam);
        Move[] moves = moveGenerator.UpdateAllPieces();
        
        if (moves.Length == 0) {
            if (teamsKing.isChecked) {
                // returning it as minus since it needs to reflect the current team and its a terrible position
                // adding the depth so it checkmates in the fewest moves
                // the higher the depth the less it takes to checkmate and the higher the checkmate value will be
                return -(checkmateValue - depth); 
            }
            return 0;
        }

        orderer.OrderMoves(board, moves);
        
        foreach (Move move in moves) {
            board.MakeMove(move);
            string currentFENPosition = FENHandler.GetFENString(board);
            ref int repetitionsOfState = ref CollectionsMarshal.GetValueRefOrAddDefault(repetitionTable, FENHandler.GetFENPieces(board), out _);
            ++repetitionsOfState;
            
            if (repetitionsOfState > 3) {
                board.UndoMove();                
                return 0;
            }

            int evaluation = -Search(depth + 1, maxDepth, -beta, -alpha);

            board.UndoMove();
            repetitionsOfState--;

            if (evaluation >= beta) {
                return beta;
            }
            if (evaluation > alpha) {
                alpha = evaluation;
                
                if (depth == 0) {
                    bestMove = move;
                }
            }
        }
        return alpha;
    }

    // makes sure the bot doesn't think a position is better than it is due to its depth cut off
    // so captures everything to give it a more accurate evaluation score
    private int QuiescenceSearch(int alpha, int beta) {
        int positionEvaluation = StaticEvaluation();
        if  (positionEvaluation >= beta) {
            return beta;
        }
        if (positionEvaluation > alpha) {
            alpha = positionEvaluation;
        }

        Move[] captures = moveGenerator.UpdateAllPieces(true);
        orderer.OrderMoves(board, captures);
        foreach (Move move in captures) {
            board.MakeMove(move);

            int evaluation = -QuiescenceSearch(-beta, -alpha);
            board.UndoMove();

            if (evaluation >= beta) {
                return beta;
            }
            if (evaluation > alpha) {
                alpha = evaluation;
            }
        }
        return alpha;
    }

    private int StaticEvaluation() {
        int whiteEvaluation = evaluation.Evaluate(board, Team.White, Team.Black);
        int blackEvaluation = evaluation.Evaluate(board, Team.Black, Team.White);

        int overallEvaluation = whiteEvaluation - blackEvaluation;
        int perspective = board.currentTeam == Team.White ? 1 : -1;
        
        return overallEvaluation * perspective;
    }
}