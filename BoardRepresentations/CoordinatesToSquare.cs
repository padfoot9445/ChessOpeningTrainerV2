namespace BoardRepresentations;
static class CoordinatesToSquare
{
    public static (File, int) ToSquare(this (int, int) coordinates) => throw new NotImplementedException(); //TODO: 0,0 = a1; 7, 7 = h8
    public static (int, int) ToCoords(this (File, int) Square) => throw new NotImplementedException(); //inverse
}