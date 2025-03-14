namespace Chess_Application;

public class GameInfo {
    public Team currentTeam { get; private set; }
    public Team opposingTeam { get; private set; }

    public GameInfo(Team currentTeam, Team opposingTeam) {
        this.currentTeam = currentTeam;
        this.opposingTeam = opposingTeam;
    }

    public GameInfo() {

    }

    public void SetTeams(Team currentTeam, Team opposingTeam) {
        this.currentTeam = currentTeam;
        this.opposingTeam = opposingTeam;
    }
}