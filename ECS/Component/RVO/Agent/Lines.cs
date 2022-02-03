using System.Collections;
using System.Collections.Generic;
 
using Unity.Entities;

namespace RVO{


    public struct Line :IBufferElementData
    {
        public Vector2 direction;
        public Vector2 point;
    }
}
