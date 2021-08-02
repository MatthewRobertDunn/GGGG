using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGGG.ExperimentalBoard
{
    public static class TestYepp
    {

        public unsafe static void Test()
        {

            float* x = stackalloc float[100];
            float* y = stackalloc float[100];
            float* z = stackalloc float[100];

            Yeppp.Core.Add_V32fV32f_V32f(x, y, z, 100);
        }
    }
}
