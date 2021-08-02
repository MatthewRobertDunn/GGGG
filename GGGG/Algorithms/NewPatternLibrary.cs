using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using GGGG.Interface;

namespace GGGG.Algorithms
{
    public static class NewPatternLibrary
    {
        public static HashSet<long> WhitePatterns;
        public static HashSet<long> BlackPatterns;

        public static void LoadPatterns()
        {

            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            WhitePatterns = (HashSet<long>)DeSerializeObject(location + @"\whitepatterns.bin");
            BlackPatterns = (HashSet<long>)DeSerializeObject(location + @"\blackpatterns.bin");
        }

        public static bool IsPatternMatch(int pos, FastBoard board, BoardSquares player)
        {
            var hash = board.HashSurrounding(pos,5);
            if (hash.Item1 == false)
                return false;

            if (player == BoardSquares.White)
                return WhitePatterns.Contains(hash.Item2);

            if (player == BoardSquares.Black)
                return BlackPatterns.Contains(hash.Item2);

            throw new Exception("Should be white or black");
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

    }
}
