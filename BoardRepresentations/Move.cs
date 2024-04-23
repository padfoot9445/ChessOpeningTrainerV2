namespace BoardRepresentations;
struct Move
{
    public string AlgebraticMove{ get; }
    public Move(string algmove)
    {
        AlgebraticMove = algmove;
    }
}