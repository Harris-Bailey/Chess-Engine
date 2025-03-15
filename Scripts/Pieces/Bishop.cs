namespace Chess;
 
public class Bishop : SlidingPiece {
    public override int InstanceID => (int)BitboardIndexes.BishopIndex;
    protected override int[] directions { get; set; } = CompassDirections.Diagonals;
    protected override bool canMoveDiagonally => true;
    protected override bool canMoveCardinally => false;
    
    public Bishop(Team team, int squareIndex) : base(team, squareIndex) { }

    public override string ToString() {
        return PieceTeam == Team.White ? "B" : "b";
    }
}