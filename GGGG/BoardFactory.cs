using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GGGG.Interface;
using GGGG.NewBoardAgain;

namespace GGGG
{
    public static class BoardFactory
    {
        public static IFastBoard GetBoard(int size)
        {
            return new StringBasedBoard(size);
            //return new FastBoard(size, size);
        }
    }
}
