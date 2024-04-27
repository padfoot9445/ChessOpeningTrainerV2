namespace UserInputHandlers.PgnParsers;

using System.Collections.Frozen;
using System.Collections.Immutable;
using BoardRepresentations;
using Chess;
class LichessPgnParser : BasePgnParser, IPgnParser
{
    public static readonly FrozenSet<char> LichessAnnotationCharacters = new HashSet<char>() { '?', '!' , '*'}.ToFrozenSet();
    private protected override GameGraphNode GetGameGraph(ChessBoard board, string Pgn, GameGraphNode? beginning = null)
    {
        char?[] charArray = Array.ConvertAll(Pgn.Trim().ToCharArray(), (char x) => (char?)x);
        int Current = 0;
        while(Current < charArray.Length)
        {
            if(charArray[Current] == '{')
            {
                while(charArray[Current] != '}'){ Current++; }
            }
            else if(charArray[Current] == '[')
            {
                while(charArray[Current] != ']'){ charArray[Current] = null; Current++; }
                charArray[Current] = null; //get the closing bracket as well
            }
            else if(LichessAnnotationCharacters.Contains((char)charArray[Current]!) || charArray[Current] == '\n' || charArray[Current] == '\r')
            {
                charArray[Current] = null;
            }
            Current++;
        }
        List<char> finishedProccessingApartFromAppendingSpaceList = Array.ConvertAll(
            (from char? c in charArray where c is not null select c).ToArray(), (char? x) => (char)x!
        ).ToList();
        string e = new string(finishedProccessingApartFromAppendingSpaceList.ToArray());
        return GetGameGraphAfterPreprocessing(board, e.Trim() + ' ', beginning);
    }

    private protected GameGraphNode GetGameGraphAfterPreprocessing(ChessBoard BoardInStartingPosition, string Pgn, GameGraphNode? beginning=null)
    {
        //parse until a branch, and then extract that, recurse, and then keep going
        int Start;
        int Current = 0; //Current should always point to the character currently being processed
        string CurrentAlgMove;
        BoardRepresentations.Move CurrentLibMove;
        GameGraphNode Beginning = beginning is not null? beginning: GameGraphNode.InitialState();
        List<GameGraphNode> NodeStack = [Beginning];
        while(Current < Pgn.Length)
        {
            Start = Current;
            //remove number. leading space, and any leading dots
            if(int.TryParse(Pgn[Current].ToString(), out int _)) //if the current character is a number, aka a new move
            {
                while(Pgn[Current] == '.' || IsWhiteSpace(Pgn[Current]) || int.TryParse(Pgn[Current].ToString(), out _)) //then while its a number, period, or space, increment
                {
                    Current++;
                }
                Start = Current;
            }
            //then, extract the half-move(the length check being in-front is important as it should short-circuit, preventing the access of Pgn[Current] which would be invalid when the length check is false)
            if(Current< Pgn.Length && Pgn[Current] != '(')
            {    
                while(Current < Pgn.Length &&!IsWhiteSpace(Pgn[Current]))
                {
                    Current++;
                }
                CurrentAlgMove = Pgn[Start..Current];
                CurrentLibMove = new BoardRepresentations.Move(CurrentAlgMove);
                
                bool HasComment = false;
                //don't even try to process comment if we know we've ran out of pgn
                if(Current < Pgn.Length)
                {
                    if (IsWhiteSpace(Pgn[Current])) { Current++; } //increase to consume whitespace between two half-moves or between a half-move and a new move, etc
                    //make the move on the board
                    BoardInStartingPosition.Move(CurrentLibMove.AlgebraticMove);
                    if (Current < Pgn.Length && Pgn[Current] == '{')
                    {
                        Current += 2;
                        Start = Current; //Current += 2 to not include the curly brace in start and also not include the leading brace
                        //check for move-comment
                        while(Pgn[Current] != '}'){ Current++; }
                        Current++; //to also remove the space after the curly brace
                        HasComment = true;
                    }
                
                }
                //create current node first, and then try to recurse if necessary
                NodeStack.Add(GameGraphNode.New((Fen)BoardInStartingPosition.ToFen(), CurrentLibMove, BoardInStartingPosition.Turn, HasComment ? Pgn[Start..(Current - 1)] : null));//current - 1 to skip the curly brace; and whoever's turn it is
                //then add this to the parent properly
                NodeStack[^2].AddChildNode(NodeStack[^1]);
                
                if(Current < Pgn.Length && HasComment && IsWhiteSpace(Pgn[Current])){ Current++; } //increment current again to skip the space after the comment
                // again, don't process branches if we've ran out of pgn
            }    
            if(Current < Pgn.Length && Pgn[Current] == '(')
            {
                Current++;
                Start = Current;//to skip the start bracket
                int BracketAmount = 1;
                while(BracketAmount != 0)
                {
                    if (Pgn[Current] == '(') { BracketAmount++; }
                    else if (Pgn[Current] == ')') { BracketAmount--; }
                    Current++;
                }
                //start..current would be the entire branching line
                // No need to add the return value to anything as we're passing in nodestack ^2, so anything that needs to be done will have been done within the function call
                GetGameGraphAfterPreprocessing(
                    ChessBoard.LoadFromFen(NodeStack[^2].PositionAfterMove.FenString), //Fen of the parent position used to create a new chessboard as a starting position
                    Pgn[Start..(Current - 1)], //extracted pgn, minus start and ending bracket
                    NodeStack[^2]
                );
                Current++;
                //to skip the whitespace after a branch
            }
            //shouldn't need any special logic to keep processing for both num..., new move, and just space-move formats
        }
        return Beginning;
        
    }
}
/*
1. e4 c5 2. Nf3 d6 3. d4 cxd4 4. Nxd4 Nf6 5. Nc3 g6 { also prep Bb5+ } 6. Be3 (6. Bc4) 6... a6 7. Qd2 (7. f3 Bg7 8. Qd2 Nbd7 { Same position as the mainline 8.Nbd7 }) 7... Bg7 8. f3 Nbd7 9. Bc4 b5 10. Bb3 (10. Bd5 { If Bd5 it ends up either drawing or as black being better } 10... Nxd5 11. Nxd5 { If Knight takes, its equal } (11. exd5 Bb7 12. Bh6 Bxh6 13. Qxh6 b4 14. Nc6 (14. Nd1 Bxd5 15. Ne3 Qa5 16. O-O) 14... Qb6 15. Ne4 (15. Na4 Qb5 16. b3 Nf6 17. O-O-O Nxd5 18. Nd4 (18. Qg7 Rf8 19. Nd4 Qa5 20. Kb1) 18... Qa5 19. Rd3 O-O-O) 15... Rc8 16. Qg7 Rf8 17. O-O-O { Ideas here are just a kingside attack } (17. Qxh7 Qe3+ 18. Kd1) 17... Bxc6 18. dxc6 Qxc6) 11... Bb7 12. O-O-O (12. O-O e6 13. Nb4 O-O 14. b3 Nb6) 12... e6 13. Nf4 (13. Bg5 Qb8 14. Ne3 (14. Nc3 { Ideas after this are just a quick queenside attack } 14... b4 15. Nce2 h6 16. Bh4 (16. Be3 a5 17. b3 e5 18. Nb5 d5 19. exd5 Ba6 20. c4 bxc3 21. Nbxc3 O-O 22. Na4 Rc8+ 23. Nec3 { Black is better due to an attack }) 16... O-O) 14... h6 15. Bh4 O-O { Equal, } 16. Rhe1 Rc8 17. Ng4 Kh7 18. e5 h5 19. Nf6+ Nxf6 20. exf6 Bh6) 13... Qe7 14. Nd3 Nb6 15. Nc6 Nc4 16. Nxe7 Nxd2 17. Rxd2 Kxe7 18. Rhd1 Rhd8 $10) 10... Bb7 11. O-O-O (11. Bh6 Bxh6 12. Qxh6 b4 (12... Qb6 13. O-O-O (13. Rd1 Rc8 14. O-O (14. Rd3) (14. Qe3 e5) (14. Rd2 e5 15. Nde2) 14... e5) 13... Rc8 14. g4 e5 15. Nde2 a5 16. a4 (16. g5 a4 17. Bd5 (17. Bxa4 Qe3+ 18. Kb1 bxa4 19. Rxd6 Nxe4 20. fxe4 Bxe4 21. Rc1 a3 22. b3 (22. Nxe4 Qxe4 23. b3 (23. Nc3 Qb4 24. Nd1 axb2 25. Rd5 bxc1=Q+) (23. Qh3 O-O 24. Rxd7 axb2 25. Kxb2 Qxe2)) (22. Qg7 Rf8 23. Rxd7 Bxc2+ 24. Rxc2 (24. Kxc2 Qxe2+) 24... Kxd7 25. Qf6 Rc6 26. Qg7 Rcc8) 22... Rxc3 23. Nxc3 Qxc3) 17... Nxd5) 16... bxa4 17. Bxa4 Qb4 18. Rd3 (18. g5 Nh5 19. Rxd6 (19. Ng3 Rxc3 20. bxc3 (20. Bxd7+ Kxd7 21. bxc3 Qa3+ 22. Kd2 Rc8) 20... Qxa4) 19... Qxd6 20. Rd1 Qc5 21. Bxd7+ Ke7 22. Bxc8 Rxc8) 18... Ba6 19. Rhd1 (19. Re3 Bxe2 20. Rxe2 Rxc3 21. Bxd7+ (21. bxc3 Qa3+ 22. Kd2 Qxa4) 21... Nxd7 22. bxc3 Qxc3) 19... Bxd3 20. Rxd3 Kd8 21. Qg7 Rf8 22. b3) 13. Nd5 Nxd5 14. exd5 Nb6 15. O-O Bxd5 16. Bxd5 Nxd5 17. Rad1 e5 18. Qg7 Rf8 19. Rfe1 (19. Nf5 Qb6+ 20. Rf2 Qc5 21. Nh6 f5 22. Qb7 (22. Qxh7 Nf4 23. g3) 22... Nc7) 19... Ne7 20. Nb3) (11. O-O O-O 12. a4 b4 13. Na2 a5 14. c3 Nc5 15. cxb4 axb4 16. Qxb4 Qb8) 11... Rc8 { Make an immediate Bh6 mainline; however, the game analyzed here plays Kb1 } 12. Bh6 (12. Kb1 Ne5 13. Bh6 (13. f4 Nc4) 13... Bxh6 14. Qxh6 Rxc3 15. bxc3 Qc7 16. Kb2 Nfd7 17. f4 (17. Rhe1 Nb6 18. f4 (18. Qg7 Rf8 19. Qxh7 (19. f4 Nec4+ 20. Bxc4 (20. Ka1 Nd2 21. Re3 Nbc4 22. Bxc4 Nxc4 23. Re2 Qa5 24. Nb3 Qa3 25. Rb1 Bc8) 20... Nxc4+ 21. Ka1 Na3 22. Rd3) 19... Qc5 (19... Na4+) 20. Kb1 (20. Qh6 Na4+ 21. Bxa4 Nc4+ 22. Kb1 Na3+ 23. Kc1 bxa4 24. Kd2 e5 25. Ke2 (25. Ne2 Nc4+ 26. Kd3 Nb2+ 27. Kd2 Nc4+ 28. Kd3 Bc6)) (20. f4 Na4+ 21. Kc1 Qxc3 22. fxe5 Qa1+ 23. Kd2 Qxd4+ 24. Kc1 Qb2+ 25. Kd2 Qc3+ 26. Kc1 (26. Ke2 Qxe5 27. Kf1 Nc3 28. Rd3 Nxe4) 26... Qb2+ 27. Kd2 Qxe5) 20... Nec4 21. Qh6 Na3+ 22. Kb2 a5 23. Ne2 Bc8 24. Ka1 a4 25. Qd2 axb3 26. cxb3) 18... Nec4+ 19. Ka1 (19. Bxc4 Nxc4+ 20. Ka1 Qa5 { White must defend c3 square }) 19... Nd2 20. Rxd2 (20. Re3 Nbc4 21. Qg7 (21. Re2 Na3 22. Rexd2 (22. Kb2 Nac4+ 23. Bxc4 Nxc4+ 24. Ka1 Qa5 25. Qh3 (25. Rde1 Qxc3+ 26. Kb1) 25... Qa3 26. Rb1 O-O) 22... Qxc3#) 21... Rf8) 20... Qxc3+ 21. Kb1 Qxd2) (17. h4 Nb6 18. h5 (18. Qg7 Rf8 19. h5 Nbc4+ 20. Bxc4 Nxc4+ 21. Ka1 Qc5 22. Nb3 Qf2 23. Nd4 (23. Kb1 Na3+ 24. Kb2 Nc4+ 25. Kc1 Qxg2) 23... e5 24. Rhf1) 18... Nbc4+ 19. Ka1 (19. Bxc4 Nxc4+ 20. Ka1 Qa5 21. Rd3 gxh5 22. Qg7 Rf8 23. Nb3 Qa3 24. Rb1 Bc6 25. Qxh7 Ne5 26. Qxh5) 19... Rf8 20. Ne2 Qc5 21. Rd4 Na3 22. Qxh7 gxh5 23. Rxh5 a5 24. Kb2 (24. f4 a4 25. fxe5 axb3 26. cxb3 Nc2+ 27. Kb2 Nxd4 28. exd6 Nxe2 (28... Qxd6) (28... Nf5 29. Qxf5 (29. Rxf5 Qxd6 30. Rxb5 (30. Qh5) (30. Kc1) (30. Kc2 Bxe4+ 31. Kc1 Qd3) 30... Qd2+) 29... Qxf5 30. Rxf5))) 17... Nc4+ 18. Bxc4 Qxc4 19. Qg7 Rf8) 12... O-O 13. Bxg7 Kxg7 14. Kb1 (14. Rhe1 e5 15. Nde2 b4 16. Na4 Qa5 17. a3 (17. Qxd6 Bc6) 17... Nc5 18. Nxc5 dxc5 19. Kb1 (19. a4 c4 20. Ba2 Qxa4 21. Ng3 (21. Qg5 Qxa2) 21... b3 (21... c3 22. bxc3 Qxa2)) 19... Rfd8 20. Qe3 bxa3) 14... Rh8 15. Rhe1 (15. g4 Nc5 16. g5 Nh5 17. Nde2 b4 18. Nd5 Bxd5 19. Bxd5 a5 20. Ng3 Nxg3 21. hxg3 Qb6 22. b3 Qc7 23. Rh3 Ne6 (23... e6 24. Bc4 (24. Qd4+ Kg8 25. Bc4 Na4 26. bxa4 Qxc4 27. Qxd6 Qxc2+ 28. Ka1 Qc3+ 29. Kb1 Qxf3) 24... Na4 25. bxa4 (25. Qxd6 Rcd8) 25... Qxc4) 24. Qh2 Nxg5 25. Rh4 Nxf3) (15. a3 h5 16. g4 (16. h3 h4 17. Rhg1 Qb6 18. g4 hxg3 19. Rxg3 Nc5 20. Ba2 Kf8 21. Nce2 a5) (16. Rhf1 Qb6 17. Nd5 Bxd5 18. exd5 Nc5 19. Qg5 Ng8 20. g4 b4 21. a4 Qb7 22. Nf5+ Kf8 23. Ng3 hxg4 24. fxg4 Nxb3 25. cxb3 Rxh2 26. Ne4 (26. Qf4 Nf6 27. g5 Rcc2 28. gxf6 Rxb2+ 29. Ka1 Ra2+ 30. Kb1 Rhb2+ 31. Kc1 Rc2+ 32. Kb1 Rab2+ 33. Ka1 Ra2+ 34. Kb1 Rcb2+ 35. Kc1 Qc7+ 36. Qc4 Rc2+ 37. Qxc2) 26... Nf6 27. Nxf6 (27. Qf4 Rcc2 28. Rf2 Rcxf2 29. Nxf2 Rh4 30. Qg3 Rh7) 27... Rcc2) 16... hxg4 17. fxg4 Nc5) (15. Rhg1 h5 16. g4 (16. h3 h4 17. g4 hxg3 18. Rxg3 Qb6) 16... hxg4 17. fxg4 e5 18. g5) 15... h5 16. Nd5 (16. g4 hxg4 17. fxg4 Nxg4) (16. h3 h4) 16... e5 17. Nxf6 Nxf6 18. Ne2 a5 *
*/