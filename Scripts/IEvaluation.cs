namespace Chess_Application;

public interface IEvaluation {
    int pawnValue { get; set; }
    int knightValue { get; set; }
    int bishopValue { get; set; }
    int rookValue { get; set; }
    int queenValue { get; set; }
    int Evaluate(Board board, Team team, Team opposingTeam);
}
