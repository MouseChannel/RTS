using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
    {
        // private Grid<PathNode> grid;
        public int x;
        public int z;


        public int gCost;
        public int hCost;
        public int fCost;
        private bool isWalkable;
        public bool IsWalkable{
            get => isWalkable;
        }

        public bool isInit;

        public GridNode cameFromNode;

        public void Init( int _x, int _z){
           
            x = _x;
            z = _z;
            isWalkable = true;
        }


        public void CalculateFCost(){
            fCost = gCost + hCost;
        }


        public void SetIsWalkable(bool isWalkable){
            this.isWalkable = isWalkable;
        }




        public override string ToString()
        {
            return x + "," + z;
        }

    }
