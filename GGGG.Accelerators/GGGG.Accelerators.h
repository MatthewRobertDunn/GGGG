// GGGG.Accelerators.h

#pragma once

using namespace System;

namespace GGGGAccelerators {


	public struct Int512
	{
		unsigned long int0;
		unsigned long int1;
		const int WhiteBit = 127;
		const int BlackBit = 126;
		const int EdgeBit = 125;
		const int EmptyBit = 124;

		bool operator== (const Int512 &c1) {
			return (int0 == c1.int0 && int1 == c1.int1);
		}

		Int512 operator|( const Int512 &left)
		{
		
		}

	};
}
