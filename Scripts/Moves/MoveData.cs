namespace Chess;

public static class MoveData {
    
    public static ulong[] KnightAttacks;
    public static ulong[] KingAttacks;
    public static ulong[][] PawnAttacks;
    
    static MoveData() {
        KnightAttacks = new ulong[64];
        KingAttacks = new ulong[64];
        PawnAttacks = new ulong[64][];
        
        AssignKnightAttacks();
        AssignKingAttacks();
        AssignPawnAttacks();
    }
    
    private static void AssignKingAttacks() {
        int[] directions = CompassDirections.CardinalsAndDiagonals;
        for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
            ulong attacks = 0;
            foreach (int direction in directions) {
                attacks |= 1ul << (squareIndex + direction);
            }
            Coordinate kingCoord = Board.ConvertSquareIndexToCoord(squareIndex);
            
            // these switch statements remove the wrap-around which happens due to
            // adding each direction without considering the number of squares to the edge
            switch (kingCoord.x) {
                case 0:
                    attacks &= BitboardHelper.NotHFile;
                    break;
                case 7:
                    attacks &= BitboardHelper.NotAFile;
                    break;
            }
            
            switch (kingCoord.y) {
                case 0:
                    attacks &= BitboardHelper.NotRank8;
                    break;
                case 7:
                    attacks &= BitboardHelper.NotRank1;
                    break;
            }
            KingAttacks[squareIndex] = attacks;
        }
    }
    
    private static void AssignKnightAttacks() {
        Coordinate[] knightDirections = {
            new Coordinate(2, 1),
            new Coordinate(2, -1),
            new Coordinate(-2, 1),
            new Coordinate(-2, -1),
            new Coordinate(1, 2),
            new Coordinate(1, -2),
            new Coordinate(-1, 2),
            new Coordinate(-1, -2),
        };
        
        for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
            Coordinate squareCoord = Board.ConvertSquareIndexToCoord(squareIndex);
            ulong attacks = 0;
            foreach (Coordinate moveCoord in knightDirections) {
                Coordinate targetCoord = squareCoord + moveCoord;
                if (targetCoord.x < 0 || targetCoord.x >= Board.dimensions | targetCoord.y < 0 || targetCoord.y >= Board.dimensions)
                    continue;
                attacks |= 1ul << targetCoord.ConvertToSquareIndex();
            }
            KnightAttacks[squareIndex] = attacks;
        }
    }
    
    private static void AssignPawnAttacks() {
        for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
            PawnAttacks[squareIndex] = new ulong[2];
            Coordinate squareCoord = Board.ConvertSquareIndexToCoord(squareIndex);

            ulong validRowForAttacks = BitboardHelper.BottomRowMask << ((squareCoord.y + 1) * 8);
            int positiveDiagonalAttackSquareIndex = squareIndex + 9;
            int negativeDiagonalAttackSquareIndex = squareIndex + 7;
            ulong rightDiagonalAttack = 1ul << positiveDiagonalAttackSquareIndex;
            ulong leftDiagonalAttack = 1ul << negativeDiagonalAttackSquareIndex;
            PawnAttacks[squareIndex][(int)Team.White] = (rightDiagonalAttack | leftDiagonalAttack) & validRowForAttacks;

            validRowForAttacks = BitboardHelper.BottomRowMask << ((squareCoord.y - 1) * 8);
            positiveDiagonalAttackSquareIndex = squareIndex - 9;
            negativeDiagonalAttackSquareIndex = squareIndex - 7;
            leftDiagonalAttack = 1ul << positiveDiagonalAttackSquareIndex;
            rightDiagonalAttack = 1ul << negativeDiagonalAttackSquareIndex;
            PawnAttacks[squareIndex][(int)Team.Black] = (rightDiagonalAttack | leftDiagonalAttack) & validRowForAttacks;
        }
    }
}