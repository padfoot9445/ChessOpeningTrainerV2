using Chess;

namespace BoardRepresentations;
readonly struct Move
{
    public Pieces Piece{ get; }
    public (int, int) OriginCoords{ get; }
    public (int, int) DestinationCoords{ get; }
    public bool Check{ get; }
    public bool Capture{ get; }
    public Pieces? CapturedPiece{ get; }
    public bool Promotes{ get; }
    public Pieces? PromotedPiece{ get; }
    public Chess.PieceColor Color{ get; }
    public Move(Pieces Piece, (int, int) OriginCoords, (int, int) DestinationCoords, bool Check, PieceColor Color, Pieces? CapturedPiece=null, Pieces? PromotedPiece=null)
    {
        this.Piece = Piece;
        this.OriginCoords = OriginCoords;
        this.DestinationCoords = DestinationCoords;
        this.Check = Check;
        this.Color = Color;
        this.Capture = CapturedPiece is not null;
        this.CapturedPiece = CapturedPiece;
        this.Promotes = PromotedPiece is not null;
        this.PromotedPiece = PromotedPiece;
    }
}