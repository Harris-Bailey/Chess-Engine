namespace Tests; 
public class ComparisonWithStockfish {

    public static void GetPerftDifferences() {
        Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string perftResultsFolder = Path.Combine(baseDirectory, @"../../../Perft Results/");
        
        string[] myResults = File.ReadAllLines(perftResultsFolder + "My Results.txt");
        string[] stockfishResults = File.ReadAllLines(perftResultsFolder + "Stockfish Results.txt");

        Console.WriteLine("My results that aren't in stockfish results:");
        foreach (string s in myResults) {
            if (!stockfishResults.Contains(s))
                Console.WriteLine(s);
        }

        Console.WriteLine();
        Console.WriteLine("Stockfish results that aren't in my results:");
        foreach (string s in stockfishResults) {
            if (!myResults.Contains(s))
                Console.WriteLine(s);
        }
    }
}