using System.Collections.Generic;
using System.Linq;
using System;
using GGGG.Interface;

namespace GGGG
{
    public class GoStringEdge : GoString
    {
    }

    public class GoString
    {
        public BoardSquares Player;
        private List<int> liberties = new List<int>();

        public IEnumerable<int> Liberties
        {
            get { return liberties; }
        }
        private List<int> stones = new List<int>();

        public IEnumerable<int> Stones
        {
            get { return stones; }
        }

        ~GoString()
        {
           // if (Q.Count < 10000000)
           // {
                Q.Push(this);
            //}
        }

        public static GoString GetNew()
        {
            GoString result = null;

            if (Q.TryPop(out result))
            {
                result.liberties.Clear();
                result.stones.Clear();
                return result;
            }
            else
                return new GoString();
        }

        public void AddLiberties(IEnumerable<int> liberties)
        {
            this.liberties.AddRangeUnique(liberties);
        }

        public void AddStones(IEnumerable<int> stones)
        {
            this.stones.AddRangeUnique(stones);
        }

        public void AddStone(int stone)
        {
            this.stones.AddUnique(stone);
        }

        public void RemoveAllLiberies(Predicate<int> x)
        {
            liberties.RemoveAll(x);
        }

        public void RemoveLiberty(int x)
        {
            liberties.Remove(x);
        }

        public void AddLiberty(int x)
        {
            liberties.AddUnique(x);
        }


        public static GoString MergeStrings(List<GoString> goStrings)
        {
            var first = goStrings.First();

            for (int i = 1; i < goStrings.Count; i++)
            {
                var target = goStrings[i];
                first.AddStones(target.Stones);
                first.AddLiberties(target.Liberties);
            }

            first.RemoveAllLiberies(x => first.Stones.Contains(x));

            return first;
        }

        public GoString Clone()
        {
            GoString newString = GetNew();

            newString.Player = this.Player;

            //newString.Stones = new List<int>(this.Stones);
            //newString.Liberties = new List<int>(this.Liberties);

            newString.stones.AddRange(this.Stones);
            newString.liberties.AddRange(this.Liberties);

            return newString;
        }

        public static System.Collections.Concurrent.ConcurrentStack<GoString> Q = new System.Collections.Concurrent.ConcurrentStack<GoString>();

    }
}