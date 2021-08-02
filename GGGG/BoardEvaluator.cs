using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GGGG.Interface;

namespace GGGG
{
    public class BoardEvaluator
    {

        public double EvaluateBoard(Board board)
        {
            double eval = 0;



            var goStrings = board.GetUniqueStrings();
            var whiteStrings = goStrings.Where(x => x.Player == BoardSquares.White).ToList();
            var blackStrings = goStrings.Where(x => x.Player == BoardSquares.Black).ToList();

            //Count number of  eyes
            var eyeCount = GetEyeCount(whiteStrings, blackStrings, board);
            eval += (eyeCount.Item1 - eyeCount.Item2);

            ////Count number of liberties on strings
            //var libertyCount = PreferStringsWithMoreLiberties(whiteStrings, blackStrings);
            //eval += (libertyCount.Item1 - libertyCount.Item2) / 4;

            //var longStringCount = PreferLongerStrings(whiteStrings, blackStrings);
            //eval += (longStringCount.Item1 - longStringCount.Item2) / 8;

            double whiteStones = whiteStrings.Sum(x => x.Stones.Count());
            double blackStones = blackStrings.Sum(x => x.Stones.Count());

            //number of captured stones, more black captured is good for white
            eval += (whiteStones - blackStones); //Captures are important

            //Influence
            //double influence = CalcInfluence(board);
            //eval += influence;

            return eval;
        }

        public Tuple<double, double> PreferStringsWithMoreLiberties(IEnumerable<GoString> whiteStrings, IEnumerable<GoString> blackStrings)
        {
            double whiteLibertyAverage = whiteStrings.Sum(x => x.Liberties.Count());
            double blackLibertyAverage = blackStrings.Sum(x => x.Liberties.Count());

            return Tuple.Create(whiteLibertyAverage, blackLibertyAverage);
        }

        public Tuple<double, double> PreferLongerStrings(List<GoString> whiteStrings, List<GoString> blackStrings)
        {


            double whiteLibertyAverage = 0.0;
            if (whiteStrings.Count != 0)
                whiteLibertyAverage = whiteStrings.Average(x => x.Stones.Count());


            double blackLibertyAverage = 0.0;
            if (blackStrings.Count != 0)
                blackLibertyAverage = blackStrings.Average(x => x.Stones.Count());

            return Tuple.Create(whiteLibertyAverage, blackLibertyAverage);
        }


        public Tuple<double, double> GetEyeCount(IEnumerable<GoString> whiteStrings, IEnumerable<GoString> blackStrings, Board board)
        {
            double whiteEyes = 0;
            double blackEyes = 0;


            foreach (var whiteString in whiteStrings)
            {
                whiteEyes += EyeCounter(whiteString, board);
            }


            foreach (var blackString in blackStrings)
            {
                blackEyes += EyeCounter(blackString, board);
            }

            return new Tuple<double, double>(whiteEyes, blackEyes);
        }


        public int EyeCounter(GoString goString, Board board)
        {
            int eyes = 0;
            foreach (var liberty in goString.Liberties)
            {
                var surrounding = board.GetSquaresAround(liberty);

                if (Array.TrueForAll(surrounding, x => board.BoardVertices[x] == BoardSquares.Edge || goString.Stones.Contains(x)))
                {
                    eyes++;
                }
                //if (eyes == 2)
                    //break;
            }

            return eyes;
        }
    }



    public class FastScoreEvaluator
    {

        public double EvaluateBoard(FastBoard board)
        {
            double whiteStonecount = 0;
            double blackStoneCount = 0;


            foreach (var stone in board.BoardVertices)
            {
                if (stone == BoardSquares.White)
                    whiteStonecount++;
                if (stone == BoardSquares.Black)
                    blackStoneCount++;
            }


            return whiteStonecount - blackStoneCount;
        }
    }
}
