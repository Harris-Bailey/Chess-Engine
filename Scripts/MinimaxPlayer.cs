using System.Runtime.InteropServices;

namespace Chess_Application; 
public class MinimaxPlayer : Bot {
    private int depth;
    IEvaluation evaluation;
    private Move bestMove = Move.NullMove;

    private const int positiveInfinity = 99_999_999;
    private const int negativeInfinity = -positiveInfinity;
    private const int checkmateValue = 1_000_000;
    private Dictionary<string, int> repetitionTable;
    // private Dictionary<string, int> positionEvaluations;
    int nodes = 0;
    MoveOrdering orderer;
    Random rand;
    
    public MinimaxPlayer(Board board, MoveGenerator moveGenerator, Action<Move> onMoveChosen, int depth, IEvaluation evaluation) : base(board, moveGenerator, onMoveChosen) {
        this.depth = depth;
        this.evaluation = evaluation;
        orderer = new MoveOrdering(evaluation);
        repetitionTable = new Dictionary<string, int>();
        rand = new Random();
    }

    public override void StartProcessing() {
        nodes = 0;
        StartAlgorithm();
        // Console.WriteLine(nodes);
    }
    
    public override void ResetBot() {
        repetitionTable.Clear();
    }
    
    private void InitialiseRepetitionTable() {
        string[] boardRepetitionTable = board.repetitionTable.ToArray();
        foreach (string state in boardRepetitionTable) {
            ref int repetitionsOfState = ref CollectionsMarshal.GetValueRefOrAddDefault(repetitionTable, state, out _);
            repetitionsOfState++;
        }
    }

    private void StartAlgorithm() {
        InitialiseRepetitionTable();
        
        int eval = Search(1, negativeInfinity, positiveInfinity);
        Console.WriteLine(eval);
        // the move hasn't been set so just choose the first move in the array for the team and play it
        if (bestMove.isNullMove) {
            Move[] moves = moveGenerator.UpdateAllPieces();
            if (moves.Any()) {
                // assign a random move
                bestMove = moves[rand.Next(moves.Length)];
            }
        }
        onMoveChosen.Invoke(bestMove);
        bestMove = Move.NullMove;
    }

    private int Search(int depth, int alpha, int beta) {
        // this repetition table will only be useful if we have a repetition table in the board class too
        ref int repetitionsOfState = ref CollectionsMarshal.GetValueRefOrAddDefault(repetitionTable, FENHandler.GetFENString(board), out _);
        repetitionsOfState++;
        if (repetitionsOfState >= 3) {
            return 0;
        }
        if (depth == 0) {
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
                return -(checkmateValue + depth); 
            }
            return 0;
        }

        orderer.OrderMoves(board, moves);
        
        foreach (Move move in moves) {
            board.MakeMove(move);
            string currentFENPosition = FENHandler.GetFENString(board);

            nodes++;
            int evaluation = -Search(depth - 1, -beta, -alpha);
            
            Console.WriteLine($"{move} has evaluation of {evaluation}");

            repetitionsOfState = ref CollectionsMarshal.GetValueRefOrAddDefault(repetitionTable, FENHandler.GetFENString(board), out _);
            board.UndoMove();
            repetitionsOfState--;
            // if (depth == this.depth) {
            //     Console.WriteLine($"{move} has evaluation of {evaluation}");
            // }
            if (evaluation >= beta) {
                return beta;
            }
            if (evaluation > alpha) {
                alpha = evaluation;

                if (depth == this.depth) {
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
            // Console.WriteLine();
            // Console.WriteLine("Making move: ");
            // BitboardHelper.PrintBitboard(board.GetPieceBitboard<Pawn>(Team.Black), 8);
            nodes++;
            int evaluation = -QuiescenceSearch(-beta, -alpha);
            board.UndoMove();
            // Console.WriteLine();
            // Console.WriteLine("Undoing: ");
            // BitboardHelper.PrintBitboard(board.GetPieceBitboard<Pawn>(Team.Black), 8);
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
        // getting the pieces of the teams
        // PiecesRemaining whitePiecesRemaining = board.GetTeamsPieces(Team.Black);
        // PiecesRemaining blackPiecesRemaining = board.GetTeamsPieces(Team.Black);
        int whiteEvaluation = evaluation.Evaluate(board, Team.White, Team.Black);
        int blackEvaluation = evaluation.Evaluate(board, Team.Black, Team.White);

        int overallEvaluation = whiteEvaluation - blackEvaluation;
        int perspective = board.currentTeam == Team.White ? 1 : -1;
        
        return overallEvaluation * perspective;
    }
}