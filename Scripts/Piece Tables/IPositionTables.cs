namespace Chess_Application;

public interface IPositionTables {
    int[][] AllPositionTables { get; set; }
    int[] BlackPawnPositionTable { get; set; }
    int[] BlackKnightPositionTable { get; set; }
    int[] BlackBishopPositionTable { get; set; }
    int[] BlackRookPositionTable { get; set; }
    int[] BlackQueenPositionTable { get; set; }
    int[] BlackKingPositionTable { get; set; }
    
    int[] WhitePawnPositionTable { get; set; }
    int[] WhiteKnightPositionTable { get; set; }
    int[] WhiteBishopPositionTable { get; set; }
    int[] WhiteRookPositionTable { get; set; }
    int[] WhiteQueenPositionTable { get; set; }
    int[] WhiteKingPositionTable { get; set; }
}