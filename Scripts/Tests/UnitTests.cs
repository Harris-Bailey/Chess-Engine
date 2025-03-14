namespace Chess_Application;

public static class UnitTests {
    
    
    public static bool TestMoveAndUndo(Board board, Move move) {
        // store the board and piece variables
        
        
        // perform move and then undo
        board.MakeMove(move);
        board.UndoMove();
        
        // get the new board and piece variables
        
        
        // compare the previous variables with the new ones
        bool passedTest = true;
        
        
        
        return passedTest;
    }
}