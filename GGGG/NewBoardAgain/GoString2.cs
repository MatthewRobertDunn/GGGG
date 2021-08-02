using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG.Algorithms;
using GGGG.Interface;

namespace GGGG.NewBoardAgain
{
    public class GoString2
    {
        public BoardSquares Player;
        public TerminatedList liberties;
        public TerminatedList stones;
        private int length;

        public TerminatedList Liberties
        {
            get { return liberties; }
        }

        public TerminatedList Stones
        {
            get { return stones; }
        }

        public GoString2()
        {
        }

        public GoString2(int length)
        {
            liberties = new TerminatedList(length);
            stones = new TerminatedList(length);
            Player = BoardSquares.Black;
            this.length = length;
        }

        public void AddLiberties(TerminatedList liberties)
        {
            this.liberties.AddItems(liberties);
        }

        public void AddLiberties(IEnumerable<int> liberties)
        {
            foreach (var liberty in liberties)
            {
                this.liberties.AddItem(liberty);
            }
        }

        public void AddStones(TerminatedList stones)
        {
            this.stones.AddItems(stones);
        }

        public void AddStones(IEnumerable<int> stones)
        {
            foreach (var stone in stones)
            {
                this.stones.AddItem(stone);
            }
        }

        public void AddStone(int stone)
        {
            this.stones.AddItem(stone);
        }

        public void RemoveAllLiberies(Predicate<int> x)
        {
            liberties.RemoveAll(x);
        }

        public void RemoveLiberty(int x)
        {
            liberties.RemoveItem(x);
        }

        public void AddLiberty(int x)
        {
            liberties.AddItem(x);
        }


        public static GoString2 MergeStrings(IEnumerable<GoString2> goStrings)
        {
            var first = goStrings.First();

            foreach (var target in goStrings.Skip(1))
            {
                first.AddStones(target.Stones);
                first.AddLiberties(target.Liberties);
            }

            first.RemoveAllLiberies(x => first.Stones.Contains(x));

            return first;
        }

        public void CopyTo(GoString2 newString)
        {
            newString.Player = this.Player;
            this.stones.CopyTo(ref newString.stones);
            this.liberties.CopyTo(ref newString.liberties);
        }


        public GoString2 Clone(FastStack<GoString2> pool)
        {
            var newString = pool.Pop(() => new GoString2());
            if (newString.liberties.MaxSize == 0)
            {
                newString.Player = this.Player;
                newString.stones = this.stones.Clone();
                newString.liberties = this.liberties.Clone();
                return newString;
            }
            CopyTo(newString);
            return newString;
        }


        public GoString2 Clone()
        {
            GoString2 newString = new GoString2();
            newString.Player = this.Player;
            newString.stones = this.stones.Clone();
            newString.liberties = this.liberties.Clone();
            return newString;
        }
    }
}
