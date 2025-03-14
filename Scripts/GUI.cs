using Chess_Application;

namespace GUI_Application;

public class GUI {
    
    Dictionary<string, int> repetitionTable;
    private Board board;
    private MoveGenerator moveGenerator;
    private FENHandler positionHandler;
    private GameResult gameResult;
    public Board Board => board;
    
    public GUI() {
        repetitionTable = new Dictionary<string, int>();
        board = new Board();
        moveGenerator = new MoveGenerator(board);
        positionHandler = new FENHandler(board, moveGenerator);
        gameResult = GameResult.None;
    }
    
    public UCIChessEngine? StartGame(UCIChessEngine startingPlayer, UCIChessEngine opponentPlayer, string positionFEN) {
        positionHandler.Initialise(positionFEN);
        UCIChessEngine activePlayer = startingPlayer;
        while (true) {
            positionFEN = HandlePlayer(activePlayer, positionFEN);
            // Console.WriteLine(positionFEN);
            if (gameResult == GameResult.Won) {
                return GetOpponent(activePlayer, startingPlayer, opponentPlayer);
            }
            else if (gameResult == GameResult.Draw) {
                return null;
            }
            activePlayer = GetOpponent(activePlayer, startingPlayer, opponentPlayer);
        }
    }
    
    private UCIChessEngine GetOpponent(UCIChessEngine activePlayer, UCIChessEngine playerOne, UCIChessEngine playerTwo) {
        if (activePlayer == playerOne)
            return playerTwo;
        else
            return playerOne;
    }
    
    private string HandlePlayer(UCIChessEngine bot, string positionFEN) {
        bot.RunAutomatically(positionFEN);
        
        if (bot.previousMove.isNullMove) {
            // Console.WriteLine("Exiting");
            gameResult = GameResult.Won;
            return "";
        }
        board.MakeMove(bot.previousMove);
        string newPositionFEN = positionHandler.GetFENString();
        
        if (repetitionTable.TryGetValue(newPositionFEN, out _)) {
            repetitionTable[newPositionFEN]++;
            // Console.WriteLine(repetitionTable[newPositionFEN]);
            if (repetitionTable[newPositionFEN] >= 3) {
                gameResult = GameResult.Draw;
                return "";
            }
        }
        else
            repetitionTable.Add(newPositionFEN, 1);
        return newPositionFEN;
    }
}