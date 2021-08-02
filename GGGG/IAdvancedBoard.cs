using System.Collections.Generic;
using GGGG.Interface;
using GGGG.NewBoardAgain;

namespace GGGG
{
    public interface IAdvancedBoard : IFastBoard
    {
        GoString2[] StringsIndex { get; }
        IList<GoString2> Strings { get; }

    }
}