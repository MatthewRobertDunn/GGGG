using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGGG.Interface;

namespace GGGG
{
    public class Board
    {
        private BoardSquares[] _board;
        private int _Width;
        private int _Height;

        public int CapturedWhiteStones;
        public int CapturedBlackStones;
        public GoString[] GoStrings;
        public Tuple<int, int> LastMove;
        public string MovesList;
        public List<string> MoveHistory = new List<string>();
        public List<string> BoardHistory = new List<string>();
        public string BoardHash = "";
        private Board(BoardSquares[] board, GoString[] strings, int width, int height)
        {
            _board = board;
            _Width = width;
            _Height = height;
            GoStrings = strings;
            CapturedWhiteStones = 0;
            CapturedBlackStones = 0;
            LastMove = new Tuple<int, int>(0, 0);
            MovesList = "";
        }

        public Board(int width, int height)
        {
            CapturedWhiteStones = 0;
            CapturedBlackStones = 0;
            width = width + 2;
            height = height + 2;
            _board = new BoardSquares[(width) * (height)];
            _Width = width;
            _Height = height;

            GoStrings = new GoString[width * height];
            LastMove = new Tuple<int, int>(0, 0);
            MovesList = "";
            SetupBoard();

            string hash = this.BoardVertices.GetHashForBoard();
            this.BoardHistory.Add(hash);
            this.BoardHash = hash;
            this.MoveHistory.Add("");
        }

        public BoardSquares GetItemAt(int x, int y)
        {
            return (BoardSquares)_board[y * Height + x];
        }

        public BoardSquares GetItemAt(int pos)
        {
            return (BoardSquares)_board[pos];
        }

        public void SetItemAt(int x, int y, BoardSquares value)
        {
            _board[y * Height + x] = value;
        }

        public void SetStringAt(int x, int y, GoString value)
        {
            GoStrings[y * Height + x] = value;
        }

        public Board PlayMove(int x, int y, BoardSquares move)
        {
            return PlayMove(XYToPos(x, y), move);
        }

        public Board PlayMove(GTextMove move)
        {
            BoardSquares player;
            if (move.Color == GoColor.White)
                player = BoardSquares.White;
            else
                player = BoardSquares.Black;

            return PlayMove((int)move.X + 1, (int)move.Y + 1, player);
        }

        public Board PlayMove(int pos, BoardSquares move)
        {
            if (move != BoardSquares.Black && move != BoardSquares.White)
                throw new Exception("Must be black or white");
            var newBoard = this.Clone();
            newBoard._board[pos] = move;
            newBoard.DoStringsForMove(pos, move);

            newBoard.LastMove = PosToXY(pos);
            GoMove lastMove = new GoMove() { Color = move, Pos = pos };

            string last = newBoard.MoveHistory.LastOrDefault();
            newBoard.MoveHistory.Add(last + lastMove.ToString());

            string hash = newBoard.BoardVertices.GetHashForBoard();
            newBoard.BoardHistory.Add(hash);
            newBoard.BoardHash = hash;
            return newBoard;
        }

        public string MoveToString(int pos, BoardSquares move)
        {
            if (move == BoardSquares.White)
                return "W" + pos.ToString("000");
            else
                return "B" + pos.ToString("000");
        }

        private void SetupBoard()
        {

            //Top edge
            for (int x = 0; x < Width; x++)
            {
                SetItemAt(x, 0, BoardSquares.Edge);
            }

            //Bottom edge
            for (int x = 0; x < Width; x++)
            {
                SetItemAt(x, Height - 1, BoardSquares.Edge);
            }

            //left edge
            for (int y = 0; y < Height; y++)
            {
                SetItemAt(0, y, BoardSquares.Edge);
            }

            //right edge
            for (int y = 0; y < Height; y++)
            {
                SetItemAt(Width - 1, y, BoardSquares.Edge);
            }
        }

        public List<GoString> CheckSurroundsForString(int pos, BoardSquares player)
        {
            var goStrings = new List<GoString>();

            var current = GetStringAt(pos, player);
            var up = GetStringAt(Above(pos), player);
            var down = GetStringAt(Below(pos), player);
            var left = GetStringAt(Left(pos), player);
            var right = GetStringAt(Right(pos), player);

            Action<GoString> addString = x =>
                                           {
                                               if (x != null)
                                                   goStrings.Add(x);
                                           };

            addString(current);
            addString(up);
            addString(down);
            addString(left);
            addString(right);

            return goStrings;
        }


        public int Above(int pos)
        {
            return pos - Height;
        }

        public int Below(int pos)
        {
            return pos + Height;
        }

        public int Left(int pos)
        {
            return pos - 1;
        }

        public int Right(int pos)
        {
            return pos + 1;
        }

        public int XYToPos(int x, int y)
        {
            return y * Height + x;
        }

        public Tuple<int, int> PosToXY(int pos)
        {
            int x = pos % Height;
            int y = (int)Math.Floor((double)pos / Height);
            return Tuple.Create(x, y);
        }


        private GoString GetStringAt(int pos, BoardSquares player)
        {
            if (GoStrings[pos] != null)
            {
                var s = GoStrings[pos];
                if (s.Player == player)
                    return s;
            }

            return null;
        }

        public List<GoString> GetUniqueStrings()
        {
            var result = new List<GoString>();

            foreach (var goString in GoStrings)
            {
                if (goString != null)
                    result.AddUnique(goString);
            }

            return result;
        }


        public int[] GetSquaresAround(int pos)
        {
            var result = new int[4];

            result[0] = Above(pos);
            result[1] = Below(pos);
            result[2] = Left(pos);
            result[3] = Right(pos);

            return result;
        }

        public BoardSquares[] GetSquareContentsAround(int pos)
        {
            var result = new BoardSquares[4];
            result[0] = GetItemAt(Above(pos));
            result[1] = GetItemAt(Below(pos));
            result[2] = GetItemAt(Left(pos));
            result[3] = GetItemAt(Right(pos));
            return result;
        }

        public bool PosIsEye(int pos, BoardSquares player)
        {
            GoString targetString;
            BoardSquares square;

            //
            var goString = GetStringAt(Above(pos), player);
            square = GetItemAt(Above(pos));

            if (goString == null && square != BoardSquares.Edge)
                return false;

            if (goString != null && goString.Player != player)
                return false;

            targetString = goString;
            //


            //
            goString = GetStringAt(Below(pos), player);
            square = GetItemAt(Below(pos));

            if (goString == null && square != BoardSquares.Edge)
                return false;

            if (goString != targetString && targetString != null && square != BoardSquares.Edge)
                return false;

            if (goString != null && goString.Player != player)
                return false;

            targetString = goString;
            //

            //
            goString = GetStringAt(Left(pos), player);
            square = GetItemAt(Left(pos));

            if (goString == null && square != BoardSquares.Edge)
                return false;

            if (goString != targetString && targetString != null && square != BoardSquares.Edge)
                return false;

            if (goString != null && goString.Player != player)
                return false;

            targetString = goString;
            //


            //
            goString = GetStringAt(Right(pos), player);
            square = GetItemAt(Right(pos));

            if (goString == null && square != BoardSquares.Edge)
                return false;

            if (goString != targetString && targetString != null && square != BoardSquares.Edge)
                return false;

            targetString = goString;

            if (goString != null && goString.Player != player)
                return false;
            //

            return true;

        }

        public List<GoString> GetStringsAround(int pos, BoardSquares player)
        {
            var result = new List<GoString>();
            var item = GetStringAt(Above(pos), player);
            if (item != null)
                result.Add(item);

            item = GetStringAt(Below(pos), player);
            if (item != null)
                result.Add(item);

            item = GetStringAt(Left(pos), player);
            if (item != null)
                result.Add(item);

            item = GetStringAt(Right(pos), player);
            if (item != null)
                result.Add(item);

            return result;
        }

        public List<int> GetEmptySquaresArround(int pos)
        {
            List<int> emptySquares = new List<int>();

            var up = GetItemAt(Above(pos));

            if (up == BoardSquares.Empty)
                emptySquares.Add(Above(pos));

            var down = GetItemAt(Below(pos));

            if (down == BoardSquares.Empty)
                emptySquares.Add(Below(pos));

            var left = GetItemAt(Left(pos));

            if (left == BoardSquares.Empty)
                emptySquares.Add(Left(pos));

            var right = GetItemAt(Right(pos));

            if (right == BoardSquares.Empty)
                emptySquares.Add(Right(pos));

            return emptySquares;
        }

        private void DoStringsForMove(int pos, BoardSquares player)
        {
            //Check surrounding squares for a string
            var surroundingStrings = CheckSurroundsForString(pos, player);

            //This is a new string as there is no surrounding strings
            if (surroundingStrings.Count == 0)
            {
                GoString newString = new GoString();
                newString.Player = player;
                newString.AddStone(pos);
                newString.AddLiberties(GetEmptySquaresArround(pos));

                //place this new string in the strings array lookup
                UpdateStringsArrayForString(newString);

                //Check if it caused any captures
                CheckForCapturedStrings(pos, player.GetEnemy());

                //This is check for suicide
                CheckForCapturedStrings(pos, player);
                return;
            }

            //We use the first string for the merge, this is arbitrary
            var first = surroundingStrings.First();
            //Add the newly played stone to this string
            first.AddStone(pos);
            //Add liberties around this newly played stone
            first.AddLiberties(GetEmptySquaresArround(pos));

            //Merge all found strings into one
            var result = GoString.MergeStrings(surroundingStrings);

            //Update strings lookup array
            UpdateStringsArrayForString(result);

            //Remove enemy captures first
            CheckForCapturedStrings(pos, player.GetEnemy());

            //This is check for suicide
            CheckForCapturedStrings(pos, player);
        }

        private void CheckForCapturedStrings(int pos, BoardSquares player)
        {

            var enemyStrings = CheckSurroundsForString(pos, player);

            foreach (var enemyString in enemyStrings)
            {
                enemyString.RemoveLiberty(pos);

                if (enemyString.Liberties.Count() == 0)
                {
                    KillDeadString(enemyString);
                }
            }
        }

        public  bool IsMoveSuicide(GoMove move)
        {
            //if you have any empty spaces around you its not suicide
            var surrounding = this.GetSquareContentsAround(move.Pos);
            if (surrounding.Any(x => x == BoardSquares.Empty))
                return false;

            //If any surrounding enemy strings has only 1 liberty its not suicide
            if (GetStringsAround(move.Pos, move.Color.GetEnemy()).Any(x => x.Liberties.Count() == 1))
                return false;
            
            //If any of your surrounding strings have more than 1 libery, it's not suicide
            if (GetStringsAround(move.Pos, move.Color).Any(x => x.Liberties.Count() > 1))
                return false;

            //If none above match, it's suicide
            return true;
        }

        public bool FastMoveIsValid(GoMove move)
        {

            if (IsMoveSuicide(move))
                return false;


            //Am I going to fill what is possibly an eye?
            //Look for moves completely surrounded by my own stones or an edge

            var surrounding = this.GetSquareContentsAround(move.Pos);
            
            if (Array.TrueForAll(surrounding, x => x == move.Color || x == BoardSquares.Edge))
            {
                //if we get in here, we MIIIGHT be playing into our own eye, only allow this if the enemy can capture by playing here

                //If any surrounding my strings has only 1 liberty then you can play into this eye because it's false
                if (GetStringsAround(move.Pos, move.Color).Any(x => x.Liberties.Count() == 1))
                    return true;
                
                return false;
            }

            return true;
        }

        public static bool MoveIsValid(Board oldBoard, Board newBoard, BoardSquares player)
        {

            //Black suicide
            if (player == BoardSquares.Black)
            {
                if (newBoard.CapturedBlackStones > oldBoard.CapturedBlackStones)
                    return false;

                //Black play into eye
            }

            //white suicide
            if (player == BoardSquares.White)
            {
                if (newBoard.CapturedWhiteStones > oldBoard.CapturedWhiteStones)
                    return false;

                //White play into eye
                //if (Array.TrueForAll(surrounding, x => x == BoardSquares.White || x == BoardSquares.Edge))
                // return false;
            }

            //Positional superko
            if (oldBoard.BoardHistory.Contains(newBoard.BoardHash))
                return false;


            //Don't play into own eye
            //if (oldBoard.PosIsEye(oldBoard.XYToPos(newBoard.LastMove.Item1, newBoard.LastMove.Item2), player))
            // return false;

            var surrounding = newBoard.GetSquareContentsAround(newBoard.XYToPos(newBoard.LastMove.Item1, newBoard.LastMove.Item2));
            //Am I going to fill what is possibly an eye?
            if (Array.TrueForAll(surrounding, x => x == player || x == BoardSquares.Edge))
            {
                //Don't do it unless it's a capture
                var oldWhite = oldBoard.CapturedWhiteStones;
                var oldBlack = oldBoard.CapturedBlackStones;

                var testBoard = oldBoard.PlayMove(newBoard.LastMove.Item1, newBoard.LastMove.Item2, player.GetEnemy());

                if (player == BoardSquares.White)
                    if (oldWhite != testBoard.CapturedWhiteStones)
                        return true;

                if (player == BoardSquares.Black)
                    if (oldBlack != testBoard.CapturedBlackStones)
                        return true;


                return false;

            }
            return true;
        }


        public static BoardSquares GetEnemy(BoardSquares player)
        {
            BoardSquares enemy = BoardSquares.White;
            if (player == BoardSquares.White)
                enemy = BoardSquares.Black;
            else
                enemy = BoardSquares.White;
            return enemy;
        }

        public int TotalWhiteMoves
        {
            get
            {
                return this.MovesList.ToCharArray().Where(x => x == 'W').Count();
            }
        }

        public int TotalBlackMoves
        {
            get
            {
                return this.MovesList.ToCharArray().Where(x => x == 'B').Count();
            }
        }

        public void KillDeadString(GoString goString)
        {
            if (goString.Player == BoardSquares.White)
                CapturedWhiteStones += goString.Stones.Count();

            if (goString.Player == BoardSquares.Black)
                CapturedBlackStones += goString.Stones.Count();

            foreach (var pos in goString.Stones)
            {
                _board[pos] = (int)BoardSquares.Empty;
                GoStrings[pos] = null;

                var nearbyStrings = CheckSurroundsForString(pos, GetEnemy(goString.Player));
                foreach (var nearbyString in nearbyStrings)
                {
                    nearbyString.AddLiberty(pos);
                }
            }
        }


        public void UpdateStringsArrayForString(GoString s)
        {
            foreach (var pos in s.Stones)
            {
                GoStrings[pos] = s;
            }
        }

        public void ForEach(Action<int, BoardSquares> func)
        {
            for (int i = 0; i < _board.Length; i++)
            {
                func(i, (BoardSquares)_board[i]);
            }
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var stone = GetItemAt(x, y);
                    switch (stone)
                    {
                        case BoardSquares.Edge:
                            s.Append("x");
                            break;
                        case BoardSquares.White:
                            s.Append("w");
                            break;
                        case BoardSquares.Black:
                            s.Append("b");
                            break;
                        case BoardSquares.Empty:
                            s.Append("+");
                            break;
                    }
                }
                s.Append(Environment.NewLine);
            }

            return s.ToString();
        }

        public IEnumerable<int> GetWhiteStones()
        {
            for (int i = 0; i < _board.Length; i++)
            {
                if (_board[i] == BoardSquares.White)
                    yield return i;
            }
        }

        public IEnumerable<int> GetBlackStones()
        {
            for (int i = 0; i < _board.Length; i++)
            {
                if (_board[i] == BoardSquares.Black)
                    yield return i;
            }
        }


        public IEnumerable<int> GetLiberties()
        {
            for (int i = 0; i < _board.Length; i++)
            {
                if (_board[i] == BoardSquares.Empty)
                    yield return i;
            }
        }

        public BoardSquares[] BoardVertices
        {
            get
            {
                return _board;
            }
        }

        public int Height
        {
            get { return _Height; }
        }

        public int Width
        {
            get { return _Width; }
        }

        public Board Clone()
        {

            //Clone strings
            var newStrings = new GoString[GoStrings.Length];
            var uniqueStrings = this.GetUniqueStrings();
            foreach (var u in uniqueStrings)
            {
                var newString = u.Clone();

                foreach (var pos in u.Stones)
                    newStrings[pos] = newString;
            }

            var b = new Board((BoardSquares[])_board.Clone(), newStrings, Width, Height);
            b.CapturedBlackStones = this.CapturedBlackStones;
            b.CapturedWhiteStones = this.CapturedWhiteStones;
            b.MovesList = this.MovesList;
            b.BoardHistory = new List<string>(this.BoardHistory);
            b.MoveHistory = new List<string>(this.MoveHistory);
            return b;

        }


    }
}
