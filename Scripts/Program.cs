using BenchmarkDiagnoser;
using BenchmarkDotNet.Running;
using Chess_Application;
using Tests;
using Bot_Tournament;
using GUI_Application;

namespace Console_App {
    class Program {
        static Dictionary<string, int> numTimesPositionIsReached = new Dictionary<string, int>();
        static UCIChessEngine[] players = new UCIChessEngine[] {
            new UCIChessEngine(new Evaluation_V1(), ""),
            new UCIChessEngine(new Evaluation_V1(), ""),
        };
        
        // 5 and 6 both won when being black so maybe similar eval :)
        
        static async Task Main(string[] args) {
            if (args.Length == 1 && args[0].ToLower() == "benchmark") {
               BenchmarkRunner.Run<Benchmarks>();
               return;
            }
            
            Board board = new Board();
            MoveGenerator moveGenerator = new MoveGenerator(board);
            FENHandler handler = new FENHandler(board, moveGenerator);

            string positionFEN = $"{FENHandler.FENStartingPosition}";
            handler.Initialise(positionFEN);
            
            UCIChessEngine engine = new UCIChessEngine(new Evaluation_V6_Advanced(new SebLaguePieceTables()), "Version 6 - Seb Lague");
            Console.WriteLine("Running");
            engine.Run();
            
            return;
            
            
            // int whiteEval = new Evaluation_V4_PositionTables().Evaluate(board, Team.White, Team.Black);
            // int blackEval = new Evaluation_V4_PositionTables().Evaluate(board, Team.Black, Team.White);
            // Console.WriteLine($"white evaluation: {whiteEval}");
            // Console.WriteLine($"black evaluation: {blackEval}");
            // return;
            
            UCIChessEngine[] engines = {
                new UCIChessEngine(new Evaluation_V1(), "Version 1"),
                new UCIChessEngine(new Evaluation_V2_Check(), "Version 2"),
                new UCIChessEngine(new Evaluation_V3_Pawns(), "Version 3"),
                
                new UCIChessEngine(new Evaluation_V4_PositionTables(new SebLaguePieceTables()), "Version 4 - SebLague"),
                new UCIChessEngine(new Evaluation_V4_PositionTables(new ChessProgrammingPieceTables()), "Version 4 - ChessProgramming"),
                new UCIChessEngine(new Evaluation_V4_PositionTables(new PeSTOPieceTables()), "Version 4 - PeSTO"),
                new UCIChessEngine(new Evaluation_V4_PositionTables(new TSCPPieceTables()), "Version 4 - TSCP"),
                
                new UCIChessEngine(new Evaluation_V5_PawnsAndTables(new SebLaguePieceTables()), "Version 5 - SebLague"),
                new UCIChessEngine(new Evaluation_V5_PawnsAndTables(new ChessProgrammingPieceTables()), "Version 5 - ChessProgramming"),
                new UCIChessEngine(new Evaluation_V5_PawnsAndTables(new PeSTOPieceTables()), "Version 5 - PeSTO"),
                new UCIChessEngine(new Evaluation_V5_PawnsAndTables(new TSCPPieceTables()), "Version 5 - TSCP"),
                
                new UCIChessEngine(new Evaluation_V6_Advanced(new SebLaguePieceTables()), "Version 6 - SebLague"),
                new UCIChessEngine(new Evaluation_V6_Advanced(new ChessProgrammingPieceTables()), "Version 6 - ChessProgramming"),
                new UCIChessEngine(new Evaluation_V6_Advanced(new PeSTOPieceTables()), "Version 6 - PeSTO"),
                new UCIChessEngine(new Evaluation_V6_Advanced(new TSCPPieceTables()), "Version 6 - TSCP"),
            };
            KnockoutTournament tournament = new KnockoutTournament(engines, 2);
            await tournament.StartTournament();
            return;
            
            // string positionFEN = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
            // Board board = new Board();
            // MoveGenerator moveGenerator = new MoveGenerator(board);
            // FENHandler handler = new FENHandler(board, moveGenerator);
            // handler.Initialise(positionFEN);
            
            // board.PrintBoard();
            // BitboardHelper.PrintBitboard(board.GetPieceBitboard<Pawn>(Team.Black), 8);
            
            // StartGame(board, handler, positionFEN);
            // board.PrintBoard();
        }
        
        static UCIChessEngine? StartGame(UCIChessEngine startingPlayer, UCIChessEngine opponentPlayer, Board board, FENHandler handler, string positionFEN) {
            // handler.Initialise(positionFEN);
            UCIChessEngine activePlayer = startingPlayer;
            while (true) {
                positionFEN = HandlePlayer(activePlayer, board, handler, positionFEN);
                // Console.WriteLine(positionFEN);
                if (positionFEN == "winner") {
                    return GetOpponent(activePlayer, startingPlayer, opponentPlayer);
                }
                else if (positionFEN == "draw") {
                    return null;
                }
                activePlayer = GetOpponent(activePlayer, startingPlayer, opponentPlayer);
            }
        }
        
        static UCIChessEngine GetOpponent(UCIChessEngine activePlayer, UCIChessEngine playerOne, UCIChessEngine playerTwo) {
            if (activePlayer == playerOne)
                return playerTwo;
            else
                return playerOne;
        }
        
        static void StartGame(Board board, FENHandler handler, string positionFEN) {
            int playerIndex = 0;
            while (true) {
                positionFEN = HandlePlayer(playerIndex, board, handler, positionFEN);
                Console.WriteLine(positionFEN);
                if (positionFEN == "winner") {
                    Console.WriteLine("Exiting");
                    playerIndex = (playerIndex + 1) % players.Length;
                    if (players[playerIndex].name != string.Empty) {
                        Console.WriteLine($"{players[playerIndex].name} wins!");
                    }
                    else {
                        Console.WriteLine($"{playerIndex} wins!");
                    }
                    return;
                }
                else if (positionFEN == "draw") {
                    Console.WriteLine("Draw!");
                    return;
                }
                playerIndex = (playerIndex + 1) % players.Length;
            }
        }
        
        static string HandlePlayer(int playerIndex, Board board, FENHandler handler, string positionFEN) {
            UCIChessEngine player = players[playerIndex];
            player.RunAutomatically(positionFEN);
            
            if (player.previousMove.isNullMove) {
                Console.WriteLine("Exiting");
                return "winner";
            }
            board.MakeMove(player.previousMove);
            string newPositionFEN = handler.GetFENString();
            if (numTimesPositionIsReached.TryGetValue(newPositionFEN, out _)) {
                numTimesPositionIsReached[newPositionFEN]++;
                Console.WriteLine(numTimesPositionIsReached[newPositionFEN]);
                if (numTimesPositionIsReached[newPositionFEN] >= 3) {
                    return "draw";
                }
            }
            else
                numTimesPositionIsReached.Add(newPositionFEN, 1);
            return newPositionFEN;
        }
        
        static string HandlePlayer(UCIChessEngine bot, Board board, FENHandler handler, string positionFEN) {
            bot.RunAutomatically(positionFEN);
            
            if (bot.previousMove.isNullMove) {
                Console.WriteLine("Exiting");
                return "winner";
            }
            board.MakeMove(bot.previousMove);
            string newPositionFEN = handler.GetFENString();
            if (numTimesPositionIsReached.TryGetValue(newPositionFEN, out _)) {
                numTimesPositionIsReached[newPositionFEN]++;
                Console.WriteLine(numTimesPositionIsReached[newPositionFEN]);
                if (numTimesPositionIsReached[newPositionFEN] >= 3) {
                    return "draw";
                }
            }
            else
                numTimesPositionIsReached.Add(newPositionFEN, 1);
            return newPositionFEN;
        }
    }
}