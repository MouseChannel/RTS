using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
using Unity.Mathematics;

public class GridInit : MonoBehaviour
{
    
      RaycastHit hit;


    public static GridInit Instance { get; private set;}
    public Grid<GridNode> pathfindingGrid;    
    private void Awake() {
        Instance = this;
    }
    
    void Start()
    {
         pathfindingGrid = new Grid<GridNode>(300,300 ,1 ,new FixedVector3(-99,0,-99));
            //   pathfindingGrid.GetNode(11,3).SetIsWalkable(false);
            //        pathfindingGrid.GetNode(11,4).SetIsWalkable(false);
            //            pathfindingGrid.GetNode(11,5).SetIsWalkable(false);
            //            pathfindingGrid.GetNode(11,6).SetIsWalkable(false);


        
    }

      private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        for(int i= 0 ;i<200;i++)
            for(int j=0;j<200;j++){
                bool iswalkable = pathfindingGrid.GetNode(i,j).IsWalkable;
                Vector3 center = new Vector3(i,1,j);
				Vector3 size = Vector3.one  ;
                if(iswalkable)				Gizmos.DrawWireCube(center, size);
                else Gizmos.DrawCube(center, size);
            }

    }

    public void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, pathfindingGrid.GetHeight() - 1);
    }
}
