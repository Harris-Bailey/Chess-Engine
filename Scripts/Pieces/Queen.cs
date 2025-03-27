namespace Chess; 

public class Queen : SlidingPiece {
    public override int ClassID => (int)BitboardIndexes.QueenIndex;
    protected override sealed int[] directions { get; set; } = CompassDirections.CardinalsAndDiagonals;  
    protected override bool canMoveDiagonally => true;
    protected override bool canMoveCardinally => true;

    public Queen(Team team, int squareIndex) : base(team, squareIndex) { }

    public override string ToString() {
        return PieceTeam == Team.White ? "Q" : "q";
    }
}