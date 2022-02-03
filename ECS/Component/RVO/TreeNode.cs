using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
namespace RVO{


    public  struct AgentTreeNode
    {
        public int begin_;
        public int end_;
        public int left_;
        public int right_;
        public FixedInt maxX_;
        public FixedInt maxY_;
        public FixedInt minX_;
        public FixedInt minY_;
    }

    public struct FixedIntPair
    {
        private FixedInt a_;
        private FixedInt b_;
    
        public FixedIntPair(FixedInt a, FixedInt b)
        {
            a_ = a;
            b_ = b;
        }
    
        public static bool operator <(FixedIntPair pair1, FixedIntPair pair2)
        {
            return pair1.a_ < pair2.a_ || !(pair2.a_ < pair1.a_) && pair1.b_ < pair2.b_;
        }
    
        public static bool operator <=(FixedIntPair pair1, FixedIntPair pair2)
        {
            return (pair1.a_ == pair2.a_ && pair1.b_ == pair2.b_) || pair1 < pair2;
        }

    
        public static bool operator >(FixedIntPair pair1, FixedIntPair pair2)
        {
            return !(pair1 <= pair2);
        }

    
        public static bool operator >=(FixedIntPair pair1, FixedIntPair pair2)
        {
            return !(pair1 < pair2);
        }
    }


    public struct ObstacleTreeNode
    {
        public int obstacleIndex;
        public int left_index;
        public int right_index;
        // public Obstacle obstacle_;
        // public ObstacleTreeNode left_;
        // public ObstacleTreeNode right_;
    }

}
