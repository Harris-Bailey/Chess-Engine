namespace Chess;
 
public class Knight : Piece {
    public override int ClassID => (int)BitboardIndexes.KnightIndex;
    
    public Knight(Team team, int squareIndex) : base(team, squareIndex) { }

    private readonly Coordinate[] moveDirections = {
        new Coordinate(2, 1),
        new Coordinate(2, -1),
        new Coordinate(-2, 1),
        new Coordinate(-2, -1),
        new Coordinate(1, 2),
        new Coordinate(1, -2),
        new Coordinate(-1, 2),
        new Coordinate(-1, -2),
    };


    protected override void HandleMoveGeneration(Board board, Span<Move> moves, ref int movesCount, ulong capturesOnlyMask) {
        ulong movesBitboard = MoveData.KnightAttacks[SquareIndex];
        King friendlyKing = board.GetTeamsKing(PieceTeam);
        
        ulong friendlyPiecesBitboard = board.GetTeamBitboard(PieceTeam);
        
        // remove moves that capture friendly pieces
        movesBitboard &= ~friendlyPiecesBitboard;
        // determines whether the moves should be only captures or whether we can include quiet moves
        movesBitboard &= capturesOnlyMask;
        // remove any moves that don't resolve the check
        movesBitboard &= friendlyKing.checkBitboard;
        // limit moves to ones that follow pins
        if (IsPinned)
            movesBitboard &= pinBitboard;
        
        while (movesBitboard != 0) {
            int targetSquare = BitboardHelper.PopLeastSignificantBit(ref movesBitboard);
            moves[movesCount++] = new Move(SquareIndex, targetSquare);
        }
    }

    public override void GenerateSquaresAttacked(Board board, King opponentKing) {
        ulong attacksFromSquare = MoveData.KnightAttacks[SquareIndex];
        if ((attacksFromSquare & (1ul << opponentKing.SquareIndex)) != 0)
            opponentKing.Check(SquareIndex);
        
        board.AddAttacks(PieceTeam, attacksFromSquare);
    }

    public override string ToString() {
        return PieceTeam == Team.White ? "N" : "n";
    }
}