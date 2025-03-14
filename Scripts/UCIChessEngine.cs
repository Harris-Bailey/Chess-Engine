namespace Chess_Application; 
public class UCIChessEngine {
    private Board board;
    private MoveGenerator moveGenerator;
    private Bot bot;
    FENHandler positionHandler;
    public Move previousMove { get; private set; }
    public readonly string name;
    private IEvaluation evaluator;

    
    public UCIChessEngine(IEvaluation evaluator, string name = "") {
        board = new Board();
        moveGenerator = new MoveGenerator(board);
        this.name = name;
        this.evaluator = evaluator;

        bot = new MinimaxPlayer(board, moveGenerator, OnMoveChosen, 5, evaluator);

        positionHandler = new FENHandler(board, moveGenerator);
        positionHandler.Initialise();
    }
    
    public UCIChessEngine Copy() {
        UCIChessEngine newEngine = new UCIChessEngine(evaluator, name);        
        return newEngine;
    }
    
    public void ResetEngine() {
        bot.ResetBot();
    }
    
    public void RunAutomatically(string FENString) {
        previousMove = Move.NullMove;
        positionHandler.Initialise(FENString);
        // if (name != string.Empty) {
        //     Console.WriteLine($"{name} - {board.currentTeam}");
        // }
        bot.StartProcessing();
    }

    public void Run() {
        // Move[] moves = moveGenerator.UpdateAllPieces(true);
        // foreach (Move move in moves) {
        //     Console.WriteLine(move);
        // }
        // return;
        // int evaluation = new Evaluation_V4_PositionTables().Evaluate(board, Team.White, Team.Black);
        // Console.WriteLine(evaluation);
        // int[] squareIndexes = BitboardHelper.GetSquareIndexesFromBitboard(board.GetTeamBitboard(Team.White));
        // foreach (int index in squareIndexes)
        //     Console.WriteLine(index);
        
        // int evaluation = new Evaluation_V4_PositionTables().Evaluate(board, Team.White, Team.Black);
        // Console.WriteLine(evaluation);
        
        // BitboardHelper.GetPieceCount(board.GetPieceBitboard<Pawn>(Team.White));
        // return;
        // for (int i = 1; i <= 7; i++) {
        //     bot = new MinimaxPlayer(board, moveGenerator, OnMoveChosen, i, new Evaluation_V5_PawnsAndTables());
        //     bot.StartProcessing();
        // }
        
        // black is this -> V5
        // white is terminal -> V4
        
        // return;
        while (true) {
            string? input = Console.ReadLine();
            if (input == null)
                return;
            input = input.Trim() ;
            string[] commandMessage = input.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            switch (commandMessage[0]) {
                case "uci":
                    Console.WriteLine("uciok");
                    break;
                case "ucinewgame":
                    ResetEngine();
                    break;
                case "isready":
                    Console.WriteLine("readyok");
                    break;
                case "go":
                    HandleGoCommand(input);
                    break;
                case "position":
                    HandlePositionInitialisation(input);
                    break;
                case "quit":
                    return;
            }
            
        }
    }

    private void HandleGoCommand(string message) {
        string perftCommand = "perft";
        int indexOfPerft = message.ToLower().IndexOf(perftCommand);
        if (indexOfPerft > 0) {
            if (!int.TryParse(message[(indexOfPerft + perftCommand.Length)..], out int depth)) {
                depth = 1;
            }
            Perft.RunPerft(board, moveGenerator, depth);
        }
        else {
            bot.StartProcessing();
        }
    }

    private void HandlePositionInitialisation(string message) {
        // finding the index of "fen"
        string movesCommand = "moves";
        string FENCommand = "fen";
        int indexOfFEN = message.ToLower().IndexOf(FENCommand);
        int indexOfMoves = message.ToLower().IndexOf(movesCommand);
        
        if (indexOfMoves > 0 && indexOfFEN > indexOfMoves)
            // use the start position by default since it's an invalid input
            return;


        if (indexOfFEN > 0) {
            int length = indexOfMoves > 0 ? indexOfMoves : message.Length;
            string FENString = message[(indexOfFEN + FENCommand.Length) .. length].Trim();
            // Console.WriteLine(FENString);
            positionHandler.Initialise(FENString);
        }


        if (indexOfMoves > 0) {
            string[] requestedMoves = message[(indexOfMoves + movesCommand.Length) ..].Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < requestedMoves.Length; i++) {
                Move moveToMake = new Move(requestedMoves[i]);
                Move[] actualMoves = moveGenerator.UpdateAllPieces();
                Piece pieceAtStartSquare = board.pieces[moveToMake.startingSquare];
                // Console.WriteLine(moveToMake);
                if (moveToMake.isNullMove || pieceAtStartSquare == null)
                    return;
                bool moveFound = false;
                for (int j = 0; j < pieceAtStartSquare.NumMoves; j++) {
                    Move move = pieceAtStartSquare.GetMove(actualMoves, j);
                    if (move.targetSquare == moveToMake.targetSquare) {
                        moveToMake = move;
                        moveFound = true;
                    }
                }
                if (!moveFound)
                    return;
                board.MakeMove(moveToMake);
            }
        }

        // board.PrintBoard();
    }

    private void OnMoveChosen(Move move) {
        previousMove = move;
        Console.WriteLine($"bestmove {move}");
    }
}