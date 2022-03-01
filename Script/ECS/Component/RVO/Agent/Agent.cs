 

using System;
using System.Collections.Generic;
using FixedMath;
using Unity.Entities;
 
 
namespace RVO{


    public struct Agent :IComponentData
    {
        public FixedVector2 position_;
        public FixedVector2 prefVelocity_;
        public FixedVector2 velocity_;
        public int id_  ;
        public int maxNeighbors_  ;
        public FixedInt maxSpeed_  ;
        public FixedInt neighborDist_  ;
        public FixedInt radius_ ;
        public FixedInt timeHorizon_  ;
        public FixedInt timeHorizonObst_  ;
        public FixedVector2 newVelocity_;
        public bool needDelete_;
     
        public int faction_;
        #region  auto attack
        public bool needCheckClosestEnemy_;
        public int closestEnemy_;
        public FixedInt attackRange_;
        #endregion
        
        #region  auto heal
        public bool needCheckRangeNeighbor;
        // public int closestEnemy_;

        #endregion
        



        // public int GetGridIndex(){
        //     return GridSystem.Instance.GetGridIndex(position_);
        // }

 
        // public static bool operator ==(Agent a, Agent b) => a.id_ == b.id_;
        // public static bool operator !=(Agent a, Agent b) => a.id_ != b.id_;



    }
}
 
