using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG.Algorithms;
using GGGG.Interface;

namespace GGGG.NewBoardAgain
{
    public unsafe class StringBasedBoard : BoardBase, IAdvancedBoard
    {
        public List<GoString2> ListOfStrings;
        public GoString2[] stringIndex;

        private BoardPool<GoString2> stringPool = new BoardPool<GoString2>(50000);

        public StringBasedBoard(int boardSize)
            : base(boardSize)
        {
            stringIndex = new GoString2[(boardSize + 2) * (boardSize + 2)];
            ListOfStrings = new List<GoString2>();
        }

        protected StringBasedBoard()
            : base()
        {
        }


        public IFastBoard PlayMove(int pos, BoardSquares move)
        {
            Debug.Assert(this.BoardHistory.Count < 500);
            Debug.Assert(!isDisposed);
            var board = (StringBasedBoard)this.Clone();
            board.PlayMoveInternal(pos, move);
            return board;
        }

        protected void PlayMoveInternal(int pos, BoardSquares player)
        {
            Debug.Assert(!isDisposed);
            //Pass
            if (pos == 0)
            {
                BoardHash.Delta(player, pos);
                BoardHistory.Add(BoardHash.HashValue);
                lastMove = new GoMove() { Color = player, Pos = pos };
                moveHistory.Add(lastMove);
                return;
            }

            BoardVertices[pos] = player;
            BoardHash.Delta(player, pos);


            int captures = DoStringsForMove(pos, player);
            lastMove = new GoMove() { Color = player, Pos = pos };
            lastMove.CaptureOccured = captures != 0;
            MoveHistory.Add(lastMove);
            BoardHistory.Add(BoardHash.HashValue);
        }


        public IFastBoard Clone()
        {
            Debug.Assert(!isDisposed);
            var newBoard = new StringBasedBoard();

            //Clone strings
            var newStrings = new GoString2[stringIndex.Length];
            newBoard.ListOfStrings = new List<GoString2>(ListOfStrings.Count);
            //foreach (var u in ListOfStrings)
            for (int i = 0; i < ListOfStrings.Count; i++)
            {
                var u = ListOfStrings[i];
                var newString = u.Clone(this.stringPool.Pool);
                newBoard.ListOfStrings.Add(newString);
                u.stones.ForEach(pos => newStrings[pos] = newString);
            }

            newBoard.stringIndex = newStrings;
            newBoard.capturedBlackStones = this.CapturedBlackStones;
            newBoard.capturedWhiteStones = this.CapturedWhiteStones;
            newBoard._boardVertices = (BoardSquares[])this.BoardVertices.Clone();
            newBoard.boardHash = this.boardHash.Clone();
            newBoard.boardHistory = new List<long>(this.boardHistory);
            newBoard.moveHistory = new List<GoMove>(this.moveHistory);
            newBoard._boardSize = this._boardSize;

            return newBoard;

        }

        private int DoStringsForMove(int pos, BoardSquares player)
        {
            int captures = 0;
            //Check surrounding squares for a string
            var surroundingStrings = CheckSurroundsForString(pos, player);

            //This is a new string as there is no surrounding strings
            if (surroundingStrings.Count() == 0)
            {
                GoString2 newString = new GoString2(BoardSize * BoardSize);
                newString.Player = player;
                newString.AddStone(pos);
                newString.AddLiberties(GetEmptySquaresArround(pos));

                //place this new string in the strings array lookup
                UpdateStringsArrayForString(newString);
                ListOfStrings.Add(newString);

                //Check if it caused any captures
                captures += CheckForCapturedStrings(pos, player.GetEnemy());

                //This is check for suicide
                captures += CheckForCapturedStrings(pos, player);
                return captures;
            }

            //We use the first string for the merge, this is arbitrary
            var first = surroundingStrings.First();
            //Add the newly played stone to this string
            first.AddStone(pos);
            //Add liberties around this newly played stone
            first.AddLiberties(GetEmptySquaresArround(pos));

            //Merge all found strings into one
            var result = GoString2.MergeStrings(surroundingStrings);
            //this.ListOfStrings.RemoveAll(x => surroundingStrings.Contains(x));
            foreach (var surroundingString in surroundingStrings)
            {
                this.ListOfStrings.Remove(surroundingString);
            }

            //Why is not result always in this list?
            this.ListOfStrings.Add(result);

            //Update strings lookup array
            UpdateStringsArrayForString(result);

            //Remove enemy captures first
            captures += CheckForCapturedStrings(pos, player.GetEnemy());

            //This is check for suicide
            captures += CheckForCapturedStrings(pos, player);

            return captures;
        }



        private int CheckForCapturedStrings(int pos, BoardSquares player)
        {
            int captures = 0;

            var enemyStrings = CheckSurroundsForString(pos, player);
            foreach (var enemyString in enemyStrings)
            {
                enemyString.RemoveLiberty(pos);
                if (enemyString.Liberties.Length == 0)
                {
                    captures += KillDeadString(enemyString);
                }
            }
            return captures;
        }


        private int KillDeadString(GoString2 goString)
        {
            int captures = 0;
            captures += goString.Stones.Count();

            if (goString.Player == BoardSquares.White)
                capturedWhiteStones += captures;
            if (goString.Player == BoardSquares.Black)
                capturedBlackStones += captures;
            foreach (var pos in goString.Stones)
            {
                BoardVertices[pos] = BoardSquares.Empty;
                BoardHash.Delta(goString.Player, pos);
                stringIndex[pos] = null;

                var nearbyStrings = CheckSurroundsForString(pos, goString.Player.GetEnemy());
                foreach (var nearbyString in nearbyStrings)
                {
                    nearbyString.AddLiberty(pos);
                }
            }

            ListOfStrings.Remove(goString);

            return captures;
        }


        private IEnumerable<GoString2> CheckSurroundsForString(int pos, BoardSquares player)
        {
            var current = GetStringAt(pos, player);
            var up = GetStringAt(Above(pos), player);
            var down = GetStringAt(Below(pos), player);
            var left = GetStringAt(Left(pos), player);
            var right = GetStringAt(Right(pos), player);

            if (current != null)
                yield return current;

            if (up != null)
                yield return up;

            if (down != null)
                yield return down;

            if (left != null)
                yield return left;

            if (right != null)
                yield return right;
        }

        private void UpdateStringsArrayForString(GoString2 s)
        {
            foreach (var pos in s.Stones)
            {
                stringIndex[pos] = s;
            }
        }

        private GoString2 GetStringAt(int pos, BoardSquares player)
        {
            if (stringIndex[pos] != null)
            {
                var s = stringIndex[pos];
                if (s.Player == player)
                    return s;
            }

            return null;
        }


        /// <summary>
        /// Call this when you are no longer going to use this board.
        /// </summary>
        public void Dispose()
        {
            //When this board is destroyed, return string datastructure to the pool so they can be reused
            //reduces memory allocation effort/fragmentation
            isDisposed = true;
            foreach (var s in ListOfStrings)
            {
                if (this.stringPool.Pool.Count < this.stringPool.Pool.MaxItems)
                    this.stringPool.Pool.Push(s);
            }
        }

        public GoString2[] StringsIndex
        {
            get
            {
                Debug.Assert(!isDisposed);
                return this.stringIndex;
            }
        }


        public IList<GoString2> Strings
        {
            get { return this.ListOfStrings; }
        }
    }
}
