using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GGGG.GoGui
{
    public class GoGuiReader
    {
        public static IEnumerable<GTextMove> ReadGoGuiCrap(string moveString)
        {
            string[] individualMoves = moveString.Split(';');

            foreach (var move in individualMoves.Where(x=>!String.IsNullOrWhiteSpace(x)))
            {
                GoColor color;
                if (move[0] == 'B')
                    color = GoColor.Black;
                else
                    color = GoColor.White;

                int x = (int)move[2] - (int)'a';
                int y = (int)move[3] - (int)'a';

                yield return new GTextMove(color, (uint) x, (uint)y);
            }
        
        }
    }
}
