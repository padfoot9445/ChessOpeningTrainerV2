namespace BoardRepresentations;
class GameGraphNode
{
    public List<Fen> Parents { get; }
    public List<Fen> Children { get; }
    public Fen PositionAfterMove{ get; }
    public Move Move{ get; }
    public GameGraphNode(List<Fen> Parents, List<Fen> Children, Fen PositionAfterMove, Move Move) => (this.Parents, this.Children, this.PositionAfterMove, this.Move) = (Parents, Children, PositionAfterMove, Move);
    public bool IsEndNode => Children.Count == 0;
    public bool IsTreeBeginning => Parents.Count == 0;
}