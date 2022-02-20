using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
using Unity.Mathematics;

public class GridInit : Singleton<GridInit>
{





    public Grid<GridNode> pathfindingGrid;

    public override void InitInstance()
    {
        pathfindingGrid = new Grid<GridNode>(300, 300, 1, new FixedVector3(-99, 0, -99));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < 200; i++)
            for (int j = 0; j < 200; j++)
            {
                bool iswalkable = pathfindingGrid.GetNode(i, j).IsWalkable;
                Vector3 center = new Vector3(i, 1, j);
                Vector3 size = Vector3.one;
                if (iswalkable) Gizmos.DrawWireCube(center, size);
                else Gizmos.DrawCube(center, size);
            }

    }

    public void ValidateGridPosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, pathfindingGrid.GetHeight() - 1);
    }
}
