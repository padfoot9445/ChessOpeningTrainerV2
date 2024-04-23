namespace UserInputHandlers.PgnParsers;
using BoardRepresentations;
using Chess;

abstract class BasePgnParser: IPgnParser
{
    public virtual GameGraphNode GetGameGraph(string Pgn) => GetGameGraph(new ChessBoard(), Pgn);
    public virtual GameGraphNode GetGameGraph(Fen StartingPosition, string Pgn) => GetGameGraph(ChessBoard.LoadFromFen(StartingPosition.FenString), Pgn);
    private protected abstract GameGraphNode GetGameGraph(ChessBoard board, string Pgn, GameGraphNode? beginning=null);
    private protected bool IsWhiteSpace(char c)
    {
        switch (c)
        {
            case ' ':
            case '\t':
            case '\r':
            case '\n':
                return true;
            default: return false;
        }
    }
}