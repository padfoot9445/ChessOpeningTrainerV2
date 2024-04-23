using Chess;

namespace BoardRepresentations;
class GameGraphNode
{
    public static Dictionary<Fen, GameGraphNode> Cache = new();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Parents"></param>
    /// <param name="Children"></param>
    /// <param name="PositionAfterMove"></param>
    /// <param name="Move"></param>
    /// <param name="Color"></param>
    /// <param name="Comment"></param>
    /// <returns></returns> <summary>
    /// 
    /// </summary>
    /// <param name="Parents"></param>
    /// <param name="Children"></param>
    /// <param name="PositionAfterMove"></param>
    /// <param name="Move"></param>
    /// <param name="Color"></param>
    /// <param name="Comment"></param>
    /// <returns></returns>
    public static GameGraphNode New(HashSet<GameGraphNode> Parents, HashSet<GameGraphNode> Children, Fen PositionAfterMove, Move Move, PieceColor Color, string? Comment = null)
    {
        if(Cache.TryGetValue(PositionAfterMove, out GameGraphNode? value)) { return value; }
        return new GameGraphNode(Parents, Children, PositionAfterMove, Move, Color, Comment);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="PositionAfterMove"></param>
    /// <param name="Move"></param>
    /// <param name="Color"></param>
    /// <param name="Comment"></param>
    /// <returns></returns> <summary>
    /// 
    /// </summary>
    /// <param name="PositionAfterMove"></param>
    /// <param name="Move"></param>
    /// <param name="Color"></param>
    /// <param name="Comment"></param>
    /// <returns></returns>
    public static GameGraphNode New(Fen PositionAfterMove, Move Move, PieceColor Color, string? Comment = null)
    {
        return New(new HashSet<GameGraphNode>(), new HashSet<GameGraphNode>(), PositionAfterMove, Move, Color, Comment);
    }
    public HashSet<GameGraphNode> Parents { get; }
    public HashSet<GameGraphNode> Children { get; }
    public Fen PositionAfterMove{ get; }
    public BoardRepresentations.Move? Move{ get; }
    public string Comment{ get; }
    public Chess.PieceColor ColorToMove{ get; }
    private GameGraphNode(HashSet<GameGraphNode> Parents, HashSet<GameGraphNode> Children, Fen PositionAfterMove, Move? Move, PieceColor Color, string? Comment = null)
    {
        (this.Parents, this.Children, this.PositionAfterMove, this.Move, this.ColorToMove, this.Comment) = (Parents, Children, PositionAfterMove, Move, Color, Comment is not null ? Comment : string.Empty);
    }
    private GameGraphNode(): this(new HashSet<GameGraphNode>(), new HashSet<GameGraphNode>(), (Fen)new ChessBoard().ToFen(), null, PieceColor.White, null){}
    public static GameGraphNode InitialState()
    {
        return new();
    }
    public bool IsEndNode => Children.Count == 0;
    public bool IsTreeBeginning => Move is null;
    public override int GetHashCode()
    {
        return PositionAfterMove.GetHashCode();
    }
    public override bool Equals(object? obj)
    {
        return obj is GameGraphNode Node && Node.PositionAfterMove == PositionAfterMove;
    }

    public void AddChildNode(GameGraphNode child)
    {
        child.AddParentNodeWithoutAlsoAddingThisNodeAsAChildToTheParentNode(this); //add this node to the child's parents
        Children.Add(child);
    }
    private protected void AddParentNodeWithoutAlsoAddingThisNodeAsAChildToTheParentNode(GameGraphNode parent)
    {
        Parents.Add(parent);
    }
}