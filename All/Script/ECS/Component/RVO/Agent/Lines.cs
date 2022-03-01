using System.Collections;
using System.Collections.Generic;
using FixedMath;
using Unity.Entities;

 


    public struct Line :IBufferElementData
    {
        public FixedVector2 direction;
        public FixedVector2 point;
    }
 
