using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using GGGG.Algorithms;
using GGGG.Interface;
using GGGG.Twister;
using GGGG.NewBoardAgain;

namespace GGGG
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            //NewPatternLibrary.LoadPatterns();
            //PrintRandomBoard(new StringBasedBoard(9));
            // RawPinDropPerformance();
            //SelfPlay(new StringBasedBoard(9));
            PrintRandomBoard(new StringBasedBoard(9));
            Console.WriteLine("Testing String Based Board Style");
            RawPinDropPerformance(new StringBasedBoard(9));

            Console.WriteLine("Testing CA Based Board Style");
            RawPinDropPerformance(new FastBoard(9, 9));

            //Console.WriteLine("New board style");
            //TestNewBoardStyle();
            //var board = new Board(9, 9);
            //Console.WriteLine(board.ToString());
            //var generator = new MoveGenerator();
            //var list = generator.GetValidMoves(board, BoardSquares.White);

            //Console.WriteLine(list[0].ToString());

            //MiniMax();
            //AlphaBeta();
            //MonteCarlo();

            //UCTMove();
            SelfPlay(new StringBasedBoard(9));
            Console.ReadLine();
        }

        public static void PrintRandomBoard(IFastBoard board)
        {
            var max = new UCTSearch();

            var test = FastMonteCarloFunctions.GetRandomBoardAtEnd(new PinDropParams(board, BoardSquares.Black));
            Console.WriteLine("Randomg board sample");
            Console.WriteLine(test.Board);


            board = board.PlayMove(7, 7, BoardSquares.Black);

            //int normal = 0, adjudicated = 0;
            //Console.WriteLine("Calculating adjudicated %");

            //for (int i = 0; i < 10000; i++)
            //{
            //    test = FastMonteCarloFunctions.GetRandomBoardAtEnd(new PinDropParams(board, BoardSquares.Black));
            //    if (test.Adjucated)
            //        adjudicated++;
            //    else
            //        normal++;
            //}
            //Console.WriteLine("Normal games {0} Adjucated games {1}", normal, adjudicated);
        }


        public static void SelfPlay(IFastBoard board)
        {
            var max = new UCTSearch();

            //var test = FastMonteCarloFunctions.GetRandomBoardAtEnd(board, BoardSquares.Black);
            //Console.WriteLine(test);
            BoardSquares player = BoardSquares.White;

            while (true)
            {
                player = player.GetEnemy();
                board = max.GetBestMove(new TimeSpan(0, 0, 0, 10), board, player).ChosenBoard;
                if (board.LastMove.Pos == 0)
                    Console.WriteLine("Engine passed");
                Console.WriteLine(board.ToString());
            }

        }

        public static void UCTMove()
        {

            Console.WriteLine("Starting UCT");
            var max = new UCTSearch();
            FastBoard board = new FastBoard(9, 9);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var result = max.GetBestMove(new TimeSpan(0, 0, 0, 20), board, BoardSquares.White);
            watch.Stop();

            Console.WriteLine(result);
            Console.WriteLine("Search took {0}", watch.ElapsedMilliseconds);
            Console.ReadLine();

        }

        //public static void MonteCarlo()
        //{

        //    Console.WriteLine("Starting alpha beta to depth 4");
        //    var max = new MonteCarloSearch();
        //    Board board = new Board(9, 9);

        //    Stopwatch watch = new Stopwatch();
        //    watch.Start();
        //    var testBoard = max.GetRandomBoardAtEnd(board, BoardSquares.White);
        //    Console.WriteLine(testBoard.ToString());
        //    var result = max.GetBestMove(200, board, BoardSquares.White);
        //    watch.Stop();

        //    Console.WriteLine("Search to depth 4 took {0}", watch.ElapsedMilliseconds);
        //    Console.ReadLine();

        //}

        public static void AlphaBeta()
        {
            Console.WriteLine("Starting alpha beta to depth 4");
            var max = new AlphaBetaSearch();

            Board board = new Board(9, 9);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var result = max.GetBestMove(5, board, double.MinValue, double.MaxValue, BoardSquares.White);
            watch.Stop();

            Console.WriteLine("Search to depth 4 took {0}", watch.ElapsedMilliseconds / 1000);
            Console.ReadLine();
        }

        public static void MiniMax()
        {
            MiniMax max = new MiniMax();

            Board board = new Board(9, 9);

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var result = max.GetBestMove(6, board, BoardSquares.White);
            watch.Stop();

            Console.WriteLine("Search to depth 4 took {0}", watch.ElapsedMilliseconds / 1000);


        }


        public static void TestNewBoardStyle()
        {
            DateTime startTime = DateTime.Now;
            int randomGamesPlayed = 0;
            FastMoveGenerator generator = new FastMoveGenerator();
            while (true)
            {
                IFastBoard board = new StringBasedBoard(9);
                //IFastBoard board = new  FastBoard(19,19);
                BoardSquares player = BoardSquares.White;
                while (true)
                {
                    player = player.GetEnemy();
                    var moves = generator.GetValidMoves(board, player).ToList();
                    if (moves.Count != 0)
                    {
                        int chosen = MyRandom.Random.Next(0, moves.Count);
                        board.Dispose();
                        board = moves[chosen];
                    }
                    else
                    {
                        if ((DateTime.Now - startTime).TotalSeconds > 20)
                        {
                            Console.WriteLine(randomGamesPlayed);
                            return;
                        }

                        randomGamesPlayed++;
                        break;
                    }
                }

            }
        }

        public static void RawPinDropPerformance(IFastBoard board)
        {
            Console.WriteLine("raw simulation performance test");

            DateTime endTime = DateTime.Now + new TimeSpan(0, 0, 0, 10);
            int i = 0;
            Parallel.For(0, 100000000000, (x, y) =>
            {
                var testBoard = FastMonteCarloFunctions.GetRandomBoardAtEnd(new PinDropParams(board, BoardSquares.Black));
                i++;
                if (i % 100 == 0)
                {
                    if (DateTime.Now > endTime)
                        y.Break();
                }
            });

            Console.WriteLine("Raw simulations is {0} per sec", i / 10);
        }

        public static void TestOldBoardStyle()
        {
            DateTime startTime = DateTime.Now;
            int randomGamesPlayed = 0;
            MoveGenerator generator = new MoveGenerator();
            while (true)
            {
                Board board = new Board(9, 9);
                BoardSquares player = BoardSquares.White;
                while (true)
                {
                    player = player.GetEnemy();
                    var moves = generator.GetValidMoves(board, player).ToList();
                    if (moves.Count != 0)
                    {
                        int chosen = MyRandom.Random.Next(0, moves.Count);
                        board = moves[chosen];
                    }
                    else
                    {
                        if ((DateTime.Now - startTime).TotalSeconds > 20)
                        {
                            Console.WriteLine(randomGamesPlayed);
                            return;
                        }

                        randomGamesPlayed++;
                        break;
                    }
                }

            }
        }

    }
}
