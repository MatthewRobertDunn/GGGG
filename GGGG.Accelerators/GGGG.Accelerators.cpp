// This is the main DLL file.

#include "stdafx.h"

#include "GGGG.Accelerators.h"

namespace GGGGAccelerators {

	public ref class Class1
	{
		// TODO: Add your methods for this class here.
	public:
		
		void  MarkStones(void* arr, int height, int width)
		{
			int length = height*width;

			Int512* cellPointer = (Int512*)arr;
			for(int i=0;i<length;i++)
			{
				Int512* currentCell = (cellPointer + i);

			
			}
		
		}
	};

}