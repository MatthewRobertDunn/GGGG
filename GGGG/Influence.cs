//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using GGGG.Interface;

//namespace GGGG
//{

//    public struct InfluenceMap
//    {
//        public float[] WhiteMap;
//        public float[] BlackMap;
//    }

//    public static class Influence
//    {
//        public static FastMoveGenerator generator = new FastMoveGenerator();

//        private static readonly float[][] SizeNineInfluenceMaps;
//        private static readonly float[][] SizeNineTeenInfluenceMaps;

//        static Influence()
//        {
//            var whiteMaps = GetInfluenceMap(9, BoardSquares.White);
//            SizeNineInfluenceMaps = new float[11 * 11][];
//            foreach (var whiteMap in whiteMaps)
//            {
//                SizeNineInfluenceMaps[whiteMap.Key] = whiteMap.Value;
//            }

//            SizeNineTeenInfluenceMaps = new float[21 * 21][];

//            whiteMaps = GetInfluenceMap(19, BoardSquares.White);
//            foreach (var whiteMap in whiteMaps)
//            {
//                SizeNineTeenInfluenceMaps[whiteMap.Key] = whiteMap.Value;
//            }

//        }


//        public static Dictionary<int, float[]> GetInfluenceMap(int boardSize, BoardSquares color)
//        {
//            var board = BoardFactory.GetBoard(boardSize);
//            var results = new Dictionary<int, float[]>();

//            foreach (var move in generator.GetMoveList(board, color))
//            {
//                if (move.Pos == 0)
//                    continue;

//                var map = CalcInfluence(move.Pos, color, board);
//                results.Add(move.Pos, map);
//            }

//            board.Dispose();

//            return results;
//        }

//        public static unsafe float CalcInfluence(this IFastBoard board)
//        {
//            float[][] influenceMaps = null;

//            //If board size is 11 get the 9x9 influence maps handy
//            if (board.BoardSize == 11)
//                influenceMaps = SizeNineInfluenceMaps;

//            if (board.BoardSize == 21)
//                influenceMaps = SizeNineTeenInfluenceMaps;

//            //Create an influence map big enough to hold the board
//            float* resultMap = stackalloc float[board.BoardSize * board.BoardSize];
//            var length = board.BoardSize * board.BoardSize;



//            //Foreach vertex do an additive or sutractive copy from the appropriate influence map onto the result
//            //for (int i = 0; i < board.BoardVertices.Length; i++)
//            //{
//            fixed (BoardSquares* cellPointer = board.BoardVertices)
//            {
//                for (int i = 0; i < board.BoardVertices.Length; i++)
//                {
//                    var item = cellPointer[i]; //implicit shallow copy here probably

//                    if (item == BoardSquares.White)
//                        fixed (float* mapsPointer = influenceMaps[i])
//                        {
//                            AdditiveCopy(resultMap, mapsPointer, resultMap, length);
//                        }


//                    if (item == BoardSquares.Black)
//                        fixed (float* mapsPointer = influenceMaps[i])
//                            SubtractiveCopy(resultMap, mapsPointer, resultMap, length);
//                }


//                float sum = 0.0f;

//                for (int i = 0; i < length; i++)
//                {
//                    var item = cellPointer[i];

//                    if (item == BoardSquares.White)
//                    {
//                        resultMap[i] = 1.0f;
//                    }

//                    else if (item == BoardSquares.Black)
//                    {
//                        resultMap[i] = -1.0f;
//                    }
//                    else
//                    {
//                        resultMap[i] = Math.Min(1.0f, resultMap[i]);
//                        resultMap[i] = Math.Max(-1.0f, resultMap[i]);
//                    }

//                    sum += resultMap[i];

//                }
                
//                return sum;
//            }

//        }

//        public static unsafe void AdditiveCopy(float* input1, float* input2, float* output, int length)
//        {
//            //Yeppp.Core.Add_V32fV32f_V32f(input1, input2, output, length);
//            for (int i = 0; i < length; i++)
//            {
//                output[i] = input1[i] + input2[i];
//            }
//        }

//        public static unsafe void SubtractiveCopy(float* input1, float* input2, float* output, int length)
//        {
//            // Yeppp.Core.Subtract_V32fV32f_V32f(input1, input2, output, length);

//            for (int i = 0; i < length; i++)
//            {
//                output[i] = input1[i] - input2[i];
//            }
//        }

//        public static float[] CalcInfluence(int stone, BoardSquares color, IFastBoard board)
//        {
//            var influence = new float[board.BoardVertices.Length];
//            float startInfluence = 0.0f;

//            if (color == BoardSquares.White)
//                startInfluence = 1.0f;
//            else
//                startInfluence = -1.0f;

//            for (int i = 0; i < board.BoardVertices.Length; i++)
//            {
//                if (board.BoardVertices[i] == BoardSquares.Edge)
//                    continue;

//                if (board.BoardVertices[i] == BoardSquares.Empty)
//                {
//                    var stoneXy = board.PosToXY(stone);
//                    var currentXy = board.PosToXY(i);
//                    float distance = (float)Math.Sqrt(Math.Pow(stoneXy.Item1 - currentXy.Item1, 2f) + Math.Pow(stoneXy.Item2 - currentXy.Item2, 2f));
//                    influence[i] = startInfluence / (float)Math.Pow((distance), 2.0f);
//                }
//            }

//            influence[stone] = startInfluence;

//            return influence;
//        }

//    }
//}
