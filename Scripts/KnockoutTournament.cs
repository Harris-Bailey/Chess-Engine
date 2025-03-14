using Chess_Application;
using GUI_Application;

namespace Bot_Tournament;

public class KnockoutTournament {
    public UCIChessEngine[] bots;
    private object padlock = new object();
    int gameCount;
    
    public KnockoutTournament(UCIChessEngine[] bots, int gameCount) {
        this.bots = bots;
        this.gameCount = gameCount;
    }
    
    public async Task StartTournament() {
        // copies the bot array to a new array
        UCIChessEngine[] nextRoundBots = bots[..];
        int roundNumber = 1;
        while (nextRoundBots.Length != 1) {
            nextRoundBots = await StartRound(nextRoundBots, roundNumber);
            roundNumber++;
        }
        PrintToConsole($"{nextRoundBots[0].name} is the winner!");
    }
    
    private async Task<UCIChessEngine[]> StartRound(UCIChessEngine[] bots, int roundNumber) {
        int numBots = bots.Length;
        int numBattles = numBots / 2;
        UCIChessEngine[] thisRoundBots;
        Task<UCIChessEngine>[] battles = new Task<UCIChessEngine>[numBattles];
        
        // if it's odd, then have the last bot automatically go through to the next round
        if (bots.Length % 2 != 0) {
            thisRoundBots = new UCIChessEngine[numBattles + 1];
            thisRoundBots[^1] = bots[^1];
        }
        else {
            thisRoundBots = new UCIChessEngine[numBattles];
        }
        
        
        for (int i = 0; i < numBattles; i++) {
            // multiplying by 2 because that's how many players are battling in a game
            int playerOneIndex = i * 2;
            int playerTwoIndex = (i * 2) + 1;
            
            Console.WriteLine($"Starting the games between {bots[playerOneIndex].name} and {bots[playerTwoIndex].name}");
            battles[i] = RunGames(bots[playerOneIndex], bots[playerTwoIndex], roundNumber);
        }
        
        // await all the games and assign the winning players to the next round
        await Task.WhenAll(battles);
        for (int i = 0; i < battles.Length; i++) {
            thisRoundBots[i] = battles[i].Result;
        }
        return thisRoundBots;
    }
    
    private async Task<UCIChessEngine> RunGames(UCIChessEngine playerOne, UCIChessEngine playerTwo, int roundNumber) {
        float playerOneScore = 0;
        float playerTwoScore = 0;
        Task[] games = new Task[gameCount];
        for (int i = 0; i < gameCount; i++) {
            games[i] = Task.Run(() => {
                UCIChessEngine whitePlayer = playerOne.Copy();
                UCIChessEngine blackPlayer = playerTwo.Copy();
                bool playerOneIsWhite = true;
                
                // flip the white and black player after half the games
                if (i >= gameCount / 2) {
                    // change the player teams so it's fair and flip the scores
                    (whitePlayer, blackPlayer) = (blackPlayer, whitePlayer);
                    playerOneIsWhite = false;
                }
                
                GUI gui = new GUI();
                UCIChessEngine? winningPlayer = gui.StartGame(whitePlayer, blackPlayer, FENHandler.FENStartingPosition);
                if (winningPlayer == whitePlayer) {
                    if (playerOneIsWhite)
                        playerOneScore += 1;
                    else
                        playerTwoScore += 1;
                }
                else if (winningPlayer == blackPlayer) {
                    if (playerOneIsWhite)
                        playerTwoScore += 1;
                    else
                        playerOneScore += 1;
                }
                else if (winningPlayer == null) {
                    playerOneScore += 0.5f;
                    playerTwoScore += 0.5f;
                }
            });
            // PrintBoard(boardForGame);
        }
        
        await Task.WhenAll(games);
        
        // draw
        if (playerOneScore == playerTwoScore) {
            // do one last sudden death match
            UCIChessEngine? winner = await Task.Run(() => {
                UCIChessEngine whitePlayer = playerOne.Copy();
                UCIChessEngine blackPlayer = playerTwo.Copy();
                GUI gui = new GUI();
                string suddenDeathPosition = "r1bq1rk1/1p1nppbp/p1np2p1/8/3NP3/1PN1B1PP/P1P2PB1/R2Q1RK1 b - - 0 11";
                UCIChessEngine? winningEngine = gui.StartGame(whitePlayer, blackPlayer, suddenDeathPosition);
                return winningEngine == whitePlayer ? playerOne : playerTwo;
            });
            // if there's still no winner the engines are most likely similar elo and so just pick a random one
            if (winner == null) {
                int playerNumber = new Random().Next(0, 2);
                if (playerNumber == 0) {
                    PrintToConsole($"{playerTwo.name} has been randomly kicked out of the tournament at round {roundNumber} due to a drawing score");
                    return playerOne;
                }
                else {
                    PrintToConsole($"{playerOne.name} has been randomly kicked out of the tournament at round {roundNumber} due to a drawing score");
                    return playerTwo;
                }
            }
            // pick the winner
            UCIChessEngine loser = winner == playerOne ? playerTwo : playerOne;
            PrintToConsole($"{loser.name} has been kicked out of the tournament at round {roundNumber} due to losing the sudden death");
            
            return winner;
        }
        // player one wins
        else if (playerOneScore > playerTwoScore) {
            PrintToConsole($"{playerTwo.name} has been kicked out of the tournament at round {roundNumber} with a score of {playerTwoScore}");
            return playerOne;
        }
        // player two wins
        else {
            PrintToConsole($"{playerOne.name} has been kicked out of the tournament at round {roundNumber} with a score of {playerOneScore}");
            return playerTwo;
        }
    }
    
    private void PrintBoard(Board board) {
        lock (padlock) {
            board.PrintBoard();
            Console.WriteLine();
        }
    }
    
    private void PrintToConsole(string message) {
        lock (padlock) {
            Console.WriteLine(message);
            // Console.WriteLine();
        }
    }
}