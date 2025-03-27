namespace Chess;
  
public class Rook : SlidingPiece {
    public override int ClassID => (int)BitboardIndexes.RookIndex;
    protected override int[] directions { get; set; } = CompassDirections.Cardinals;
    protected override bool canMoveDiagonally => false;
    protected override bool canMoveCardinally => true;

    public Rook(Team team, int squareIndex) : base(team, squareIndex) { }

    public override string ToString() {
            return PieceTeam == Team.White ? "R" : "r";
    }
}