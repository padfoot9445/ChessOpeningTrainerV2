namespace UserInputHandlers.PgnParsers;
using BoardRepresentations;
interface IPgnParser
{
    public GameGraphNode GetGameGraph(string Pgn); //the gamegraphnode should be of the starting position
    public GameGraphNode GetGameGraph(Fen StartingPosition, string Pgn);
}