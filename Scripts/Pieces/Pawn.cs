namespace Chess;
 
public class Pawn : Piece {

    public enum MovementDirection {
        MovingDownwards = -1,
        MovingUpwards = 1
    }

    public override int ClassID => (int)BitboardIndexes.PawnIndex;
    public MovementDirection direction { get; private set; }
    private int lastRankNum;
    private int startingRankNum;
    
    public Pawn(Team team, int squareIndex) : base(team, squareIndex) {
        direction = team == Team.White ? MovementDirection.MovingUpwards : MovementDirection.MovingDownwards;
        int startingRank = direction == MovementDirection.MovingUpwards ? 1 : 6;
        int lastRank = direction == MovementDirection.MovingDownwards ? 0 : Board.Dimensions - 1;
        startingRankNum = startingRank;
        lastRankNum = lastRank;
        
        Coordinate squareCoord = new Coordinate(squareIndex);
        if (squareCoord.y != startingRankNum)
            SetToMoved(true);
    }

    protected override void HandleMoveGeneration(Board board, Span<Move> moves, ref int movesCount, ulong capturesOnlyMask) {
        ulong opponentPiecesBitboard = board.GetTeamBitboard(Board.GetOpposingTeam(PieceTeam));
        ulong allPiecesBitboard = board.GetAllPiecesBitboard();

        Coordinate squareCoord = new Coordinate(SquareIndex);
        King friendlyKing = board.GetTeamsKing(PieceTeam);
        
        int singlePushTargetSquareIndex = SquareIndex + CompassDirections.Up * (int)direction;
        
        ulong movesBitboard = 0;
        // adding the single push move but only if there's no pieces in that square -> 0 if there is a piece - otherwise there isn't
        ulong singlePush = (1ul << singlePushTargetSquareIndex) & (~allPiecesBitboard);
        movesBitboard |= singlePush;
        // adding the attacks (the diagonal moves capturing a piece) but only if there's an opponent piece in that square
        movesBitboard |= MoveData.PawnAttacks[SquareIndex][(int)PieceTeam] & opponentPiecesBitboard;
        
        // restricting the moves to either only capturing moves or all moves depending on the mask
        // and restricting moves to only those that resolve check
        movesBitboard &= capturesOnlyMask & friendlyKing.checkBitboard;
        
        // remove any moves that don't respect the pin
        if (IsPinned)
            movesBitboard &= pinBitboard;
        
        while (movesBitboard != 0) {
            int moveTargetSquareIndex = BitboardHelper.PopLeastSignificantBit(ref movesBitboard);
            // our current y coordinate is one off from the last rank so the next move will result in a promotion
            if (squareCoord.y == lastRankNum - (int)direction) {
                GeneratePromotions(moves, moveTargetSquareIndex, ref movesCount);
                continue;
            }
            moves[movesCount++] = new Move(SquareIndex, moveTargetSquareIndex);
        }
        
        int doublePushSquareIndex = singlePushTargetSquareIndex + CompassDirections.Up * (int)direction;
        if (CanAddDoublePush(squareCoord, doublePushSquareIndex, singlePush == 0, friendlyKing, allPiecesBitboard, capturesOnlyMask != ulong.MaxValue))
            moves[movesCount++] = new Move(SquareIndex, doublePushSquareIndex, Move.SpecialMoveType.PushPawnTwoSquares);

        if (CanAddEnPassantMove(board, friendlyKing)) {
# nullable disable
            // the target square index would be above or below the en passant's square depending on its direction
            int enPassantMoveToIndex = board.CurrentEnPassantPawn.SquareIndex + CompassDirections.Up * (int)direction;
            moves[movesCount++] = new Move(SquareIndex, enPassantMoveToIndex, Move.SpecialMoveType.EnPassantCapture);
# nullable enable
        }
    }
    
    public bool CanAddDoublePush(Coordinate squareCoord, int doublePushSquareIndex, bool singlePushHasPiece, King friendlyKing, ulong allPiecesBitboard, bool capturesOnly) {
        // only generating captures and we know a double push can't capture anything
        if (capturesOnly)
            return false;
        // if the pawn isn't on the starting rank so can never add the double move
        if (squareCoord.y != startingRankNum) 
            return false;
        // there is a piece in front of the pawn in the single push position
        if (singlePushHasPiece)
            return false;
            
        // bitboard for the double push combined with the pieces -> 0 if there's a piece blocking - otherwise there isn't
        ulong doublePush = (1ul << doublePushSquareIndex) & (~allPiecesBitboard);
        
        if (IsPinned && (doublePush & pinBitboard) == 0)
            return false;
        if (friendlyKing.isChecked && (doublePush & friendlyKing.checkBitboard) == 0)
            return false;
        
        // there is a piece in the double push position
        if (doublePush == 0)
            return false;
            
        return true;
    }

    public bool CanAddEnPassantMove(Board board, King friendlyKing) {
        // there is no en passant currently available
        if (board.CurrentEnPassantPawn == null)
            return false;
        // if the pieces aren't on the same rank as each other we can return early
        if (!Board.IsSameRank(SquareIndex, board.CurrentEnPassantPawn.SquareIndex))
            return false;
        // the pieces aren't next to each other on the board
        if (Math.Abs(SquareIndex - board.CurrentEnPassantPawn.SquareIndex) != 1)
            return false;
        
        // the target square is above or below the en passant square index depending on the direction
        int enPassantTargetSquare = board.CurrentEnPassantPawn.SquareIndex + CompassDirections.Up * (int)direction;
        ulong enPassantTargetMask = 1ul << enPassantTargetSquare;
        ulong enPassantCaptureMask = 1ul << board.CurrentEnPassantPawn.SquareIndex;
        
        bool enPassantFollowsPin = true;
        
        // check if the en passant target square is restricted from a pin
        if (IsPinned)
            enPassantFollowsPin = (enPassantTargetMask & pinBitboard) != 0;
        
        // getting the overlap of the checkbitboard and the en passant capture
        bool enPassantFollowsCheck = (enPassantCaptureMask & friendlyKing.checkBitboard) != 0;

        // en passant mask would be 0 if its an invalid en passant capture, otherwise it's valid
        return enPassantFollowsPin && enPassantFollowsCheck;
    }

    private void GeneratePromotions(Span<Move> moves, int targetSquareIndex, ref int arrayCount) {
        moves[arrayCount++] = new Move(SquareIndex, targetSquareIndex, Move.SpecialMoveType.PromoteToQueen);
        moves[arrayCount++] = new Move(SquareIndex, targetSquareIndex, Move.SpecialMoveType.PromoteToKnight);
        moves[arrayCount++] = new Move(SquareIndex, targetSquareIndex, Move.SpecialMoveType.PromoteToRook);
        moves[arrayCount++] = new Move(SquareIndex, targetSquareIndex, Move.SpecialMoveType.PromoteToBishop);
    }

    public override void GenerateSquaresAttacked(Board board, King opponentKing) {
        ulong attacksFromSquare = MoveData.PawnAttacks[SquareIndex][(int)PieceTeam];
        if ((attacksFromSquare & (1ul << opponentKing.SquareIndex)) != 0)
            opponentKing.Check(SquareIndex);
            
        board.AddAttacks(PieceTeam, attacksFromSquare);
    }

    public override string ToString() {
        return PieceTeam == Team.White ? "P" : "p";
    }
}