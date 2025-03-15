using System.Diagnostics;
using Chess;

namespace Tests; 
public class Perft {        
    public static void RunPerft(Board board, MoveGenerator moveGenerator, int depth) {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        int numTotalNodes = Divide(board, moveGenerator, depth);
        sw.Stop();
        Console.WriteLine();
        Console.WriteLine($"Number of nodes searched: {numTotalNodes}");
        Console.WriteLine($"Completed in {(double)sw.ElapsedMilliseconds / 1000} seconds");
    }
    
    private static int MoveGenerationTest(Board board, MoveGenerator moveGenerator, int depth) {
        if (depth == 0) {
            return 1;
        }
        
        int numMovesFound = 0;
        Move[] moves = moveGenerator.UpdateAllPieces();
        for (int i = 0; i < moves.Length; i++) {
            board.MakeMove(moves[i]);
            numMovesFound += MoveGenerationTest(board, moveGenerator, depth - 1);
            board.UndoMove();
        }
        return numMovesFound;
    }
    
    private static int Divide(Board board, MoveGenerator moveGenerator, int depth) {
        int totalMovesFound = 0;
        Move[] moves = moveGenerator.UpdateAllPieces();

        for (int i = 0; i < moves.Length; i++) {
            board.MakeMove(moves[i]);
            int numNodesFound = MoveGenerationTest(board, moveGenerator, depth - 1);
            Console.WriteLine($"{moves[i]}: {numNodesFound}");
            totalMovesFound += numNodesFound;
            board.UndoMove();
        }
        return totalMovesFound;
    }
}