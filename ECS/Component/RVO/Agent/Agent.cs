 

using System;
using System.Collections.Generic;
using FixedMath;
using Unity.Entities;
 
 
namespace RVO{


    public struct Agent :IComponentData
    {
            
        
        
            public Vector2 position_;
            public Vector2 prefVelocity_;
            public Vector2 velocity_;
            public int id_  ;
            public int maxNeighbors_  ;
            public FixedInt maxSpeed_  ;
            public FixedInt neighborDist_  ;
            public FixedInt radius_ ;
            public FixedInt timeHorizon_  ;
            public FixedInt timeHorizonObst_  ;
            public bool needDelete_  ;

            public Vector2 newVelocity_;

        public static bool operator ==(Agent a, Agent b) => a.id_ == b.id_;
        public static bool operator !=(Agent a, Agent b) => a.id_ != b.id_;



    }
}
 
