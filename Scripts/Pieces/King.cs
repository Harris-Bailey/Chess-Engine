namespace Chess; 

public class King : Piece {
    public override int ClassID => (int)BitboardIndexes.KingIndex;
    public ulong checkBitboard;
    public bool isChecked;
    public bool isDoubleChecked;
    public bool canCastleQueenside;
    public bool canCastleKingside;
    
    public King(Team team, int squareIndex) : base(team, squareIndex) {
        isChecked = false;
        isDoubleChecked = false;
        canCastleQueenside = false;
        canCastleKingside = false;
    }

    protected override void HandleMoveGeneration(Board board, Span<Move> moves, ref int movesCount, ulong capturesOnlyMask) {
        ulong movesBitboard = MoveData.KingAttacks[SquareIndex];
        
        ulong friendlyPiecesBitboard = board.GetTeamBitboard(PieceTeam);
        ulong allPiecesBitboard = board.GetAllPiecesBitboard();
        
        // only including moves that result in the king being on a safe square
        movesBitboard &= ~board.GetSquaresAttackedByNextTeam(PieceTeam);
        // remove any moves that would capture friendly pieces
        movesBitboard &= ~friendlyPiecesBitboard;
        // determines whether the moves should be only captures or whether we can include quiet moves
        movesBitboard &= capturesOnlyMask;
        
        while (movesBitboard != 0) {
            int targetSquare = BitboardHelper.PopLeastSignificantBit(ref movesBitboard);
            moves[movesCount++] = new Move(SquareIndex, targetSquare);
        }

        AddCastlingMoves(board, moves, ref movesCount, capturesOnlyMask != ulong.MaxValue, allPiecesBitboard);
    }

    private void AddCastlingMoves(Board board, Span<Move> moves, ref int movesCount, bool capturesOnly, ulong allPiecesBitboard) {
        // since castling isn't a capturing move, can return early
        // and castling is only available if the king hasn't moved and if it's not in check
        if (capturesOnly || HasMoved || isChecked)
            return;
        ulong friendlyRookBitboard = board.GetPieceBitboard(BitboardIndexes.RookIndex, PieceTeam);
        Coordinate kingCoord = new Coordinate(SquareIndex);
        if (canCastleQueenside) {
            ulong spacesBetweenKingAndRook = 0b00001110ul << (kingCoord.y * 8);
            ulong affectedSquaresByAttacks = 0b00001100ul << (kingCoord.y * 8);
            if ((board.GetSquaresAttackedByNextTeam(PieceTeam) & affectedSquaresByAttacks) == 0) {
                if (CanCastleOnSide(board, spacesBetweenKingAndRook, allPiecesBitboard, friendlyRookBitboard, 0 + kingCoord.y * 8)) {
                    moves[movesCount++] = new Move(SquareIndex, SquareIndex - 2, Move.SpecialMoveType.CastlingQueenside);
                }
            }
        }
        if (canCastleKingside) {
            ulong spacesBetweenKingAndRook = 0b01100000ul << (kingCoord.y * 8);
            ulong affectedSquaresByAttacks = spacesBetweenKingAndRook;
            if ((board.GetSquaresAttackedByNextTeam(PieceTeam) & affectedSquaresByAttacks) == 0) {
                if (CanCastleOnSide(board, spacesBetweenKingAndRook, allPiecesBitboard, friendlyRookBitboard, 7 + kingCoord.y * 8)) {
                    moves[movesCount++] = new Move(SquareIndex, SquareIndex + 2, Move.SpecialMoveType.CastlingKingside);
                }
            }
        }
    }

    private bool CanCastleOnSide(Board board, ulong spacesBetweenKingAndRook, ulong allPiecesBitboard, ulong friendlyRookBitboard, int rookIntendedPosition) {
        // there are pieces between the king and the rook
        if ((spacesBetweenKingAndRook & allPiecesBitboard) != 0)
            return false;
        // the rook isn't in the position
        if (((1ul << rookIntendedPosition) & friendlyRookBitboard) == 0)
            return false;
        
        # nullable disable
        Rook castlingRook = board.Pieces[rookIntendedPosition] as Rook;
        if (castlingRook.HasMoved)
            return false;
        return true;
        # nullable enable
    }
    
    public string GetCastlingString() {
        string castlingRights = string.Empty;
        if (canCastleKingside)
            castlingRights += ToString();
        if (canCastleQueenside)
            castlingRights += PieceTeam == Team.White ? "Q" : "q";
        return castlingRights;
    }

    public override void GenerateSquaresAttacked(Board board, King opponentKing) {
        ulong attacks = MoveData.KingAttacks[SquareIndex];
        board.AddAttacks(PieceTeam, attacks);
    }

    public void Check(int squareIndexChecking) {
        isDoubleChecked = isChecked;
        isChecked = true;
        checkBitboard |= 1ul << squareIndexChecking;
    }

    public void Check(ulong bitboardForSquaresChecking) {
        isDoubleChecked = isChecked;
        isChecked = true;
        checkBitboard |= bitboardForSquaresChecking;
    }

    public void Uncheck() {
        isChecked = false;
        isDoubleChecked = false;
        checkBitboard = 0;
    }

    public override string ToString() {
        return PieceTeam == Team.White ? "K" : "k";
    }
}