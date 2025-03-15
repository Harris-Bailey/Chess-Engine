using Chess;

class Program {        
    static void Main() {
        UCIChessEngine engine = new UCIChessEngine(new Evaluation_V6_Advanced(new SebLaguePieceTables()), "Version 6 - Seb Lague");
        engine.Run();
    }
}