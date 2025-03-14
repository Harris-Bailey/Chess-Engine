using System.Diagnostics;

namespace Chess_Application {
    public class Perft {
        
        static int num = 0;
        
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
                num++;
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
        
        /*
            dividing with the move "e6d5" messes up the move "f6e4"
            everything's how it was so the depth is fine
            I need to get all the moves and their depths with "e6d5" and without to see what happens
            not doing it now tho... need to sleep awah awah awah :)
            
            
            so I now know it's not unpinning captured pieces
        */

        private static int Divide(Board board, MoveGenerator moveGenerator, int depth) {
            int totalMovesFound = 0;
            Move[] moves = moveGenerator.UpdateAllPieces();
            // Move[] moves = new Move[] { new("e6d5") };
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
}