namespace Chess_Application;

public static class ArrayHelpers {
    
    private static readonly int[] flippedTable = {
		56, 57, 58, 59, 60, 61, 62, 63,
		48, 49, 50, 51, 52, 53, 54, 55,
		40, 41, 42, 43, 44, 45, 46, 47,
		32, 33, 34, 35, 36, 37, 38, 39,
		24, 25, 26, 27, 28, 29, 30, 31,
		16, 17, 18, 19, 20, 21, 22, 23,
		 8,  9, 10, 11, 12, 13, 14, 15,
		 0,  1,  2,  3,  4,  5,  6,  7,
	};
    
    public static int[] GetFlippedArray(this int[] arr) {
        int[] flippedArr = new int[arr.Length];
        for (int i = 0; i < arr.Length; i++) {
            flippedArr[i] = arr[flippedTable[i]];
        }
        return flippedArr;
    }
	
	public static void PrintArray(this int[] arr, int dimensions) {
		
	}
}