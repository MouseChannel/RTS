using System.Collections;
using System.Collections.Generic;
 
using Unity.Entities;

namespace RVO{


    public struct Line :IBufferElementData
    {
        public FixedVector2 direction;
        public FixedVector2 point;
    }
}
