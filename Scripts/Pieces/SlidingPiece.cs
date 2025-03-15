namespace Chess;
 
public abstract class SlidingPiece : Piece {
    protected SlidingPiece(Team team, int squareIndex) : base(team, squareIndex) { }
    protected abstract int[] directions { get; set; }
    protected abstract bool canMoveDiagonally { get; }
    protected abstract bool canMoveCardinally { get; }

    protected override sealed void HandleMoveGeneration(Board board, Span<Move> moves, ref int movesCount, ulong capturesOnlyMask) {
        King friendlyKing = board.GetTeamsKing(PieceTeam);
        King opponentKing = board.GetTeamsKing(board.opponentTeam);
        
        ulong friendlyPiecesBitboard = board.GetTeamBitboard(PieceTeam);
        ulong opponentPiecesBitboard = board.GetTeamBitboard(Board.GetOpposingTeam(PieceTeam));
        
        // remove any moves that would capture friendly pieces
        // determines whether the moves should be only captures or whether we can include quiet moves
        // remove any moves that don't resolve the check
        ulong attackBitboard = GetAttacks(board, friendlyPiecesBitboard, friendlyPiecesBitboard | opponentPiecesBitboard, opponentKing, false);
        attackBitboard &= (~friendlyPiecesBitboard) & capturesOnlyMask & friendlyKing.checkBitboard;
        
        // remove any moves that don't respect the pin
        if (IsPinned)
            attackBitboard &= pinBitboard;
            
        while (attackBitboard != 0) {
            int targetSquareIndex = BitboardHelper.PopLeastSignificantBit(ref attackBitboard);
            moves[movesCount++] = new Move(SquareIndex, targetSquareIndex);
        }
    }
    
    public override sealed void GenerateSquaresAttacked(Board board, King opponentKing) {
        ulong friendlyPiecesBitboard = board.GetTeamBitboard(PieceTeam);
        ulong allPiecesBitboard = board.GetAllPiecesBitboard();
        ulong attacks = GetAttacks(board, friendlyPiecesBitboard, allPiecesBitboard, opponentKing, true);
        board.AddAttacks(PieceTeam, attacks);
    }

    public ulong GetAttacks(Board board, ulong friendlyPiecesBitboard, ulong allPiecesBitboard, King opponentKing, bool calculateChecksAndPins) {
        ulong attacks = 0;
        foreach (int direction in directions) {
            ulong attacksIncludingIllegal = BitboardHelper.GetAttackBitboardInSingleDirection(SquareIndex, direction);

            if ((allPiecesBitboard & attacksIncludingIllegal) == 0) {
                attacks |= attacksIncludingIllegal;
                continue;
            }
            
            ulong blockersInAttackLine = 0;
            int firstBlockerSquareIndex = 0;
            int secondBlockerSquareIndex = 0;
            int thirdBlockerSquareIndex = 0;
            switch (direction) {
                case < 0:
                    blockersInAttackLine = attacksIncludingIllegal & allPiecesBitboard;
                    // pop most significant bit to get the piece closest to the attacker
                    firstBlockerSquareIndex = BitboardHelper.PopMostSignificantBit(ref blockersInAttackLine);
                    secondBlockerSquareIndex = BitboardHelper.PopMostSignificantBit(ref blockersInAttackLine);
                    thirdBlockerSquareIndex = BitboardHelper.PopMostSignificantBit(ref blockersInAttackLine);
                    break;
                case > 0:
                    blockersInAttackLine = attacksIncludingIllegal & allPiecesBitboard;
                    // pop least significant bit to get the piece closest to the attacker
                    firstBlockerSquareIndex = BitboardHelper.PopLeastSignificantBit(ref blockersInAttackLine);
                    secondBlockerSquareIndex = BitboardHelper.PopLeastSignificantBit(ref blockersInAttackLine);
                    thirdBlockerSquareIndex = BitboardHelper.PopLeastSignificantBit(ref blockersInAttackLine);
                    break;
            }
            
            // adding the attacks up to the blocker
            ulong legalAttacks = BitboardHelper.GetLegalAttacks(attacksIncludingIllegal, direction, firstBlockerSquareIndex);
            attacks |= legalAttacks;
            
            ulong blockerBitboard = 1ul << firstBlockerSquareIndex;
            
            if (!calculateChecksAndPins)
                continue;
            
            // if we've restricted en passant then we know there's 2 pawns in the way of the king 
            // and don't need to check for further pins or checks
            if (TryRestrictEnPassantCapture(board, opponentKing, direction, firstBlockerSquareIndex, secondBlockerSquareIndex, thirdBlockerSquareIndex))
                continue;
            
            // the blocker is a friendly piece so we continue since we can't pin or check our own pieces
            if ((blockerBitboard & friendlyPiecesBitboard) != 0)
                continue;
                
            
            // if the opponent's king is the first blocker we need to give a check
            if (firstBlockerSquareIndex == opponentKing.SquareIndex) {
                ulong checkBitboard = legalAttacks;
                // removing the king from the check bitboard
                checkBitboard ^= blockerBitboard;
                // adding this piece to the check bitboard so other piece's know they can capture it as part of the check if possible
                checkBitboard ^= 1ul << SquareIndex;
                opponentKing.Check(checkBitboard);
                
                // need to add squares being attacked behind the king so the king knows it can't move there
                // a result of -1 signifies that there is no piece behind the king
                if (secondBlockerSquareIndex != -1) {
                    ulong attacksExcludingKing = BitboardHelper.GetLegalAttacks(attacksIncludingIllegal, direction, secondBlockerSquareIndex);
                    attacks |= attacksExcludingKing;
                }
                // this would mean there is no other piece so we can add the attacks that go to the edge of the board
                else
                    attacks |= attacksIncludingIllegal;
            }
            // otherwise see if there's the king behind the blocking piece
            else if (secondBlockerSquareIndex == opponentKing.SquareIndex) {
                ulong pinBitboard = BitboardHelper.GetLegalAttacks(attacksIncludingIllegal, direction, secondBlockerSquareIndex);
                // remove the king's square from the pin
                pinBitboard ^= 1ul << secondBlockerSquareIndex;
                // add this piece's square index so other piece's know they can capture it as part of the pin if possible
                pinBitboard ^= 1ul << SquareIndex;
                board.PinPiece(firstBlockerSquareIndex, pinBitboard);
            }
        }
        return attacks;
    }
    
    private bool TryRestrictEnPassantCapture(Board board, King opponentKing, int direction, int firstBlockerSquareIndex, int secondBlockerSquareIndex, int thirdBlockerSquareIndex) {
        // there is no en passant pawn to be captured so we don't need this check
        if (board.CurrentEnPassantPawn == null)
            return false;

        // if it's not horizontal direction then there's no en passant restriction
        // and the same goes for if the third blocker isn't the opponent king
        if (!CompassDirections.IsHoriztonal(direction) || thirdBlockerSquareIndex != opponentKing.SquareIndex)
            return false;

        // the pieces aren't next to each other
        if (Math.Abs(firstBlockerSquareIndex - secondBlockerSquareIndex) != 1)
            return false;

        ulong friendlyPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, PieceTeam);
        ulong opponentPawnBitboard = board.GetPieceBitboard(BitboardIndexes.PawnIndex, Board.GetOpposingTeam(PieceTeam));

        // masking both squares with the pawn bitboards
        ulong friendlyPawnMask = friendlyPawnBitboard & ((1ul << firstBlockerSquareIndex) | (1ul << secondBlockerSquareIndex));
        ulong opponentPawnMask = opponentPawnBitboard & ((1ul << firstBlockerSquareIndex) | (1ul << secondBlockerSquareIndex));

        // there's no pawns in one of them, if this isn't true then there's exactly one pawn in both bitboards
        if (friendlyPawnMask == 0 || opponentPawnMask == 0)
            return false;

        // if the en passant pawn isn't on our team we don't need to restrict anything
        int friendlyPawnSquareIndex = BitboardHelper.GetLeastSignificantBit(friendlyPawnMask);
        if (friendlyPawnSquareIndex != board.CurrentEnPassantPawn.SquareIndex)
            return false;
        
#nullable disable
        Pawn friendlyPawn = board.pieces[friendlyPawnSquareIndex] as Pawn;
        // the capture square is above or below the pawn's square index depending on its direction
        int enPassantCaptureSquare = friendlyPawnSquareIndex - CompassDirections.Up * (int)friendlyPawn.direction;
        // this will turn all bits on except for the capture square of the en passant pawn
        ulong enPassantRestrictionMask = ~(1ul << enPassantCaptureSquare);
#nullable enable
        
        // get the opponent pawn
        int opponentPawnSquareIndex = BitboardHelper.GetLeastSignificantBit(opponentPawnMask);
        // pin the pawn so that it can no longer capture the en passant pawn
        board.pieces[opponentPawnSquareIndex].Pin(enPassantRestrictionMask);
        
        return true;
    }
}