using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using GGGG;
using GoTraxx;

namespace PatternCreator
{
    class Program
    {

        private const int minRank = 1800;
        static void Main(string[] args)
        {

            HashSet<Int64> whitePatterns = new HashSet<long>();
            HashSet<Int64> blackPatterns = new HashSet<long>();

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(@"F:\GoDbase\");

            int i = 0;
            foreach (var file in dir.GetFiles("*.sgf"))
            {
                i++;

                GameRecord gameRecord = new GameRecord();
                SGFCollection collection = new SGFCollection();
                collection.LoadSGFFile(file.FullName);
                collection.RetrieveGame(gameRecord);

                if (gameRecord.WhiteRank.Contains("?") || gameRecord.BlackRank.Contains("?"))
                {
                    continue;
                }

                if (int.Parse(gameRecord.WhiteRank) > minRank || int.Parse(gameRecord.BlackRank) > minRank)
                {
                    LoadPatternsFromCollection(blackPatterns, whitePatterns, gameRecord);
                }


                if (i % 1000 == 0)
                {
                    Console.WriteLine("Loaded {0} games", i);
                    Console.WriteLine("total patterns loaded {0}", whitePatterns.Count + blackPatterns.Count);
                }

            }

            Console.WriteLine("total patterns loaded {0}", whitePatterns.Count + blackPatterns.Count);


            Console.WriteLine("Saving white patterns");
            SerializeObject(@"f:\whitepatterns.bin", whitePatterns);

            Console.WriteLine("Saving black patterns");
            SerializeObject(@"f:\blackpatterns.bin", blackPatterns);

            Console.WriteLine("All done");

            Console.ReadLine();
        }



        public static void SerializeObject(string filename, object objectToSerialize)
        {
            Stream stream = File.Open(filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, objectToSerialize);
            stream.Close();
        }

        public static object DeSerializeObject(string filename)
        {
            object objectToSerialize;
            Stream stream = File.Open(filename, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            objectToSerialize = bFormatter.Deserialize(stream);
            stream.Close();
            return objectToSerialize;
        }

        private static void LoadPatternsFromCollection(HashSet<long> blackPatterns, HashSet<long> whitePatterns, GameRecord gameRecord)
        {
            CoordinateSystem cs = new CoordinateSystem(9);
            IFastBoard board = new FastBoard(9, 9);
            foreach (var move in gameRecord.Moves)
            {
                int x = cs.GetColumn(move.Move) + 1;
                int y = cs.GetRow(move.Move) + 1;

                if (move.Player.IsWhite)
                {
                    board = board.PlayMove(x, y, BoardSquares.White);

                    if (int.Parse(gameRecord.WhiteRank) > minRank)
                    {
                        var pattern = board.HashSurrounding(board.XYToPos(x, y), 5);
                        if (pattern.Item1)
                            whitePatterns.Add(pattern.Item2);
                    }

                }

                if (move.Player.IsBlack)
                {
                    board = board.PlayMove(x, y, BoardSquares.Black);

                    if (int.Parse(gameRecord.BlackRank) > minRank)
                    {
                        var pattern = board.HashSurrounding(board.XYToPos(x, y), 5);
                        if (pattern.Item1)
                            blackPatterns.Add(pattern.Item2);

                    }

                }

                //Get pattern around move just played


            }
        }
    }
}
