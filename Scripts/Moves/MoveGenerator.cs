namespace Chess;

public class MoveGenerator {
    Board board;

    public MoveGenerator(Board board) {
        this.board = board;
    }
    
    public void Initialise(Board setupBoard) {
        // need to initialise the starting teams attack data
        // King opponentTeamsKing = board.GetTeamsKing(board.opponentTeam);
        // ulong opponentTeamsKingBitboard = 1ul << opponentTeamsKing.squareIndex;
        // ulong friendlyPiecesBitboard = board.GetTeamBitboard(board.currentTeam);
        // while (friendlyPiecesBitboard != 0) {
        //     int pieceSquareIndex = BitboardHelper.PopLeastSignificantBit(ref friendlyPiecesBitboard);
        //     Piece friendlyPiece = board.pieces[pieceSquareIndex];
        //     friendlyPiece.GenerateSquaresAttacked(board, opponentTeamsKingBitboard, board.currentTeam);
        // }
    }

    /// <summary>
    /// Resets all pieces, generates the attacked squares by the opponent, and then generates the moves of the current team
    /// </summary>
    /// <returns>An array on moves for the current team</returns>
    public Move[] UpdateAllPieces(bool capturesOnly = false) {
        King currentTeamsKing = board.GetTeamsKing(board.currentTeam);
        currentTeamsKing.Uncheck();
        
        board.UnpinAllPinnedPieces();
        board.ClearAttackedSquares(board.opponentTeam);
        
        ulong friendlyPiecesBitboard = board.GetTeamBitboard(board.currentTeam);
        ulong opponentPiecesBitboard = board.GetTeamBitboard(board.opponentTeam);
            
        GenerateOpponentsAttackedSquares(currentTeamsKing, opponentPiecesBitboard);
        
        if (!currentTeamsKing.isChecked)
            currentTeamsKing.checkBitboard = ulong.MaxValue;
            
        Move[] teamsMoves = GenerateMoves(currentTeamsKing, capturesOnly, friendlyPiecesBitboard, opponentPiecesBitboard);
        return teamsMoves;

    }

    public void GenerateOpponentsAttackedSquares(King currentTeamsKing, ulong opponentTeamsBitboard) {            
        while (opponentTeamsBitboard != 0) {
            int squareIndex = BitboardHelper.PopLeastSignificantBit(ref opponentTeamsBitboard);
            board.Pieces[squareIndex].GenerateSquaresAttacked(board, currentTeamsKing);
        }
    }

    public Move[] GenerateMoves(King friendlyKing, bool capturesOnly, ulong friendlyPiecesBitboard, ulong opponentPiecesBitboard) {
        int movesCount = 0;
        ulong capturesOnlyMask = capturesOnly ? board.GetTeamBitboard(board.opponentTeam) : ulong.MaxValue;
        
        // making sure enough moves can be stored at any one time
        // the most moves currently found in a given position is the composition by Nenad Petrovic with 218 so I've added a little extra padding
        // https://lichess.org/analysis/fromPosition/R6R/3Q4/1Q4Q1/4Q3/2Q4Q/Q4Q2/pp1Q4/kBNN1KB1_w_-_-
        Span<Move> moves = stackalloc Move[256];
        // calculate king moves separate so we can skip unnecessary function calls if the king is in double check
        friendlyKing.GenerateMoves(board, moves, ref movesCount, capturesOnlyMask);
        
        if (friendlyKing.isDoubleChecked)
            return moves[..movesCount].ToArray();
        
        // removing the king since we've calculated its moves separately
        ulong friendlyPiecesCopy = friendlyPiecesBitboard ^ (1ul << friendlyKing.SquareIndex);
        while (friendlyPiecesCopy != 0) {
            int pieceSquareIndex = BitboardHelper.PopLeastSignificantBit(ref friendlyPiecesCopy);
            Piece piece = board.Pieces[pieceSquareIndex];
            piece.GenerateMoves(board, moves, ref movesCount, capturesOnlyMask);
        }
        return moves[..movesCount].ToArray();
    }
}