using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FixedMath;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Jobs;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class PathFindSystem : SystemBase
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    protected override void OnCreate()
    {
       endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        int gridWidth = GridInit.Instance.pathfindingGrid.GetWidth();
        int gridHeight = GridInit.Instance.pathfindingGrid.GetHeight();
        int2 gridSize = new int2(gridWidth, gridHeight);


        List<FindPathJob> findPathJobList = new List<FindPathJob>();
        NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);


        NativeArray<PathNode> pathNodeArray = GetPathNodeArray();
   

        Entities.ForEach((Entity entity, ref PathFindParams pathfindParams  ) => {
            NativeArray<PathNode> tmpPathNodeArray = new NativeArray<PathNode>(pathNodeArray, Allocator.TempJob);
            EntityCommandBuffer ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

            FindPathJob findPathJob = new FindPathJob {
                  
                gridSize = gridSize,
                pathNodeArray = tmpPathNodeArray,
                startPosition = pathfindParams.startPosition,
                endPosition = pathfindParams.endPosition,
                entity = entity,
                
            };

            
            findPathJobList.Add(findPathJob);
            jobHandleList.Add(findPathJob.Schedule());
            
                        
            ecb.RemoveComponent<PathFindParams>(entity);
            
            
        }).WithoutBurst().Run();

        JobHandle.CompleteAll(jobHandleList);
        
        
       

        foreach (FindPathJob findPathJob in findPathJobList) {
            new SetBufferPathJob {
                entity = findPathJob.entity,
                gridSize = findPathJob.gridSize,
                pathNodeArray = findPathJob.pathNodeArray,
                pathfindingParamsComponentDataFromEntity = GetComponentDataFromEntity<PathFindParams>(),
                pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollow>(),
                pathPositionBufferFromEntity = GetBufferFromEntity<PathPosition>(),
            }.Run();
        }

        pathNodeArray.Dispose();
        jobHandleList.Dispose();
         
         
    }




    [BurstCompile]
    private struct FindPathJob : IJob
    {
       
        public  int2 gridSize;
        public NativeArray<PathNode> pathNodeArray;
        public int2 startPosition;
        public int2 endPosition;

        public Entity entity;
 
     
        





        public void Execute()
        {
            // for (int i = 0; i < pathNodeArray.Length; i++) {
            //     PathNode pathNode = pathNodeArray[i];
            //     pathNode.hCost = CalculateDistanceCost(new int2(pathNode.x, pathNode.y), endPosition);
            //     pathNode.cameFromNodeIndex = -1;

            //     pathNodeArray[i] = pathNode;
            // }
   

            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left
            neighbourOffsetArray[1] = new int2(+1, 0); // Right
            neighbourOffsetArray[2] = new int2(0, +1); // Up
            neighbourOffsetArray[3] = new int2(0, -1); // Down
            neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
            neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
            neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
            neighbourOffsetArray[7] = new int2(+1, +1); // Right Up
            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);

             while (openList.Length > 0) {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex) {
                    // Reached our destination!
                    break;
                }

                // Remove current node from Open List
                for (int i = 0; i < openList.Length; i++) {
                    if (openList[i] == currentNodeIndex) {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentNodeIndex);

                for (int i = 0; i < neighbourOffsetArray.Length; i++) {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                    if (!IsPositionInsideGrid(neighbourPosition, gridSize)) {
                        // Neighbour not valid position
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                    if (closedList.Contains(neighbourNodeIndex)) {
                        // Already searched this node
                        continue;
                    }

                    PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                    if (!neighbourNode.isWalkable) {
                        // Not walkable
                        // closedList.Add(neighbourNodeIndex);
                        continue;
                    }

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

	                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
	                if (tentativeGCost < neighbourNode.gCost) {
		                neighbourNode.cameFromNodeIndex = currentNodeIndex;
		                neighbourNode.gCost = tentativeGCost;
		                neighbourNode.CalculateFCost();
		                pathNodeArray[neighbourNodeIndex] = neighbourNode;

		                if (!openList.Contains(neighbourNode.index)) {
			                openList.Add(neighbourNode.index);
		                }
	                }

                }
            }

            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();



            
            
            
             
        }
    }




    [BurstCompile]
    private struct SetBufferPathJob : IJob {
        
        public int2 gridSize;

        [DeallocateOnJobCompletion]
        public NativeArray<PathNode> pathNodeArray;

        public Entity entity;

        public ComponentDataFromEntity<PathFindParams> pathfindingParamsComponentDataFromEntity;
        // [NativeDisableContainerSafetyRestriction]
        public ComponentDataFromEntity<PathFollow> pathFollowComponentDataFromEntity;
        public BufferFromEntity<PathPosition> pathPositionBufferFromEntity;

        public void Execute() {
            DynamicBuffer<PathPosition> pathPositionBuffer = pathPositionBufferFromEntity[entity];
            pathPositionBuffer.Clear();

            PathFindParams pathfindingParams = pathfindingParamsComponentDataFromEntity[entity];
            int endNodeIndex = CalculateIndex(pathfindingParams.endPosition.x, pathfindingParams.endPosition.y, gridSize.x);
            PathNode endNode = pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1) {
                // Didn't find a path!
                //Debug.Log("Didn't find a path!");
                pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = -1 };
            } else {
                // Found a path
                CalculatePath(pathNodeArray, endNode, pathPositionBuffer);
                         
                SmoothingPath(pathPositionBuffer );

                RemoveCorner(pathNodeArray, pathPositionBuffer, gridSize.x);
    

                pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = pathPositionBuffer.Length - 2 };
            }

        }
    }
    
    
    
    
    private NativeArray<PathNode> GetPathNodeArray() {
        Grid<GridNode> grid = GridInit.Instance.pathfindingGrid;

        int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());
        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.TempJob);

        for (int x = 0; x < grid.GetWidth(); x++) {
            for (int y = 0; y < grid.GetHeight(); y++) {
                PathNode pathNode = new PathNode();
                pathNode.x = x;
                pathNode.y = y;
                pathNode.index = CalculateIndex(x, y, gridSize.x);

                pathNode.gCost = int.MaxValue;
                
                pathNode.isWalkable = grid.GetNode(x, y).IsWalkable;
                pathNode.cameFromNodeIndex = -1;

                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        return pathNodeArray;
    }
    
    
    
    
    
    
    private static void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<PathPosition> pathPositionBuffer) {
        if (endNode.cameFromNodeIndex == -1) {
            // Couldn't find a path!
        } else {
            // Found a path
            pathPositionBuffer.Add(new PathPosition { position = new int2(endNode.x, endNode.y) });

            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1) {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                pathPositionBuffer.Add(new PathPosition { position = new int2(cameFromNode.x, cameFromNode.y) });
                currentNode = cameFromNode;
            }
        }
    }

    private static void SmoothingPath(DynamicBuffer<PathPosition> pathPositionBuffer){
        
        NativeList<PathPosition> newPath = new NativeList<PathPosition>(pathPositionBuffer.Length,Allocator.Temp);
      
        //除去共线的点
        if(pathPositionBuffer.Length <=1) return;
        newPath.Add(pathPositionBuffer[0]);
        int2  dir = pathPositionBuffer[1].position - pathPositionBuffer[0].position;
 
        for(int i=1;i<pathPositionBuffer.Length ;i++){
            int2 currentDir = pathPositionBuffer[i].position - pathPositionBuffer[i-1].position; 
             
            if( currentDir.x != dir.x  ||  currentDir.y != dir .y ){   
         
                newPath.Add(  pathPositionBuffer[i - 1]  );
                dir = currentDir;
            }
        }
        newPath.Add( pathPositionBuffer[pathPositionBuffer.Length-1]);
        pathPositionBuffer.CopyFrom(newPath);
         
        newPath.Dispose();
    }
    private static void RemoveCorner(NativeArray<PathNode> pathNodeArray,DynamicBuffer<PathPosition> pathPositionBuffer, int gridWidth){
    //    Debug.Log(string.Format("长度 {0}", pathPositionBuffer.Length));
    // if(pathPositionBuffer.Length ==3 ) return;
 
 
        NativeList<PathPosition> newPath = new NativeList<PathPosition>(pathPositionBuffer.Length,Allocator.Temp);
        newPath.Add(pathPositionBuffer[0]);
        int i = 0;
        int j = 2;
        while( j < pathPositionBuffer.Length ){
            
            if(IsExistBarrier(pathNodeArray,
                                pathPositionBuffer[i].position,
                                pathPositionBuffer[j].position, 
                                gridWidth)){
                                    // Debug.Log(string.Format("exist barrier {0} {1} ||||{2} {3}",pathPositionBuffer[i].position.x,
                                    //                                                             pathPositionBuffer[i].position.y,
                                    //                                                             pathPositionBuffer[j].position.x,
                                    //                                                             pathPositionBuffer[j].position.y)  );
                                    newPath.Add(pathPositionBuffer[j - 1]);
                                    // Debug.Log(string.Format("Add {0} {1}", pathPositionBuffer[j - 1].position.x, pathPositionBuffer[j - 1].position.y));
                                    i = j - 1;

            }
            else{
                j++;
            }
        }
        newPath.Add(pathPositionBuffer[pathPositionBuffer.Length - 1]);
 
        pathPositionBuffer.CopyFrom(newPath);
 
        //     for(int q = 0;q<pathPositionBuffer.Length;q++){
        //     Debug.Log(string.Format("path = {0} {1}", pathPositionBuffer[q].position.x,pathPositionBuffer[q].position.y));
        // }

    


    }
    private static bool IsExistBarrier(NativeArray<PathNode> pathNodeArray,int2 A,int2 B, int gridWidth){
    
    //   Debug.Log(string.Format("Check barrier {0} {1} |{2} {3}", A.x,A.y,B.x,B.y));
        bool noBarrier;
 
        //先特殊处理k = 0和 k = 无穷两种情况，即 x = x and y = y
        if( A.x == B.x){
            for(int y = math.min(A.y, B.y);y< math.max( B.y, A.y); y++){
                noBarrier = CheckWalkable(A.x,y);
                if(!noBarrier) return true;               
                  
            }
            return false;
        }
        if(A.y == B.y){
            for(int x = math.min(A.x,B.x)  ;x< math.max(A.x,B.x);x++){
                 noBarrier = CheckWalkable(x,A.y);
                if(!noBarrier) return true; 
                         
            }
            return false;
        }


        if(A.x > B.x){
            Swap(ref A, ref B);
        }
        CalulateLinerFunc(new FixedVector2(A) ,new FixedVector2(B), out  FixedInt k, out FixedInt b);
 
        bool CheckWalkable(int x,int y) =>pathNodeArray[CalculateIndex(x,y,gridWidth)].isWalkable;       

        if( FixedCalculate.Abs(k) == 1){
            if(k == 1){
                for(int2 x = A  ; x.x<  B.x; x++){
                    noBarrier =   CheckWalkable(x.x  ,x.y) ;
                    if(!noBarrier) return true;
                }
            }
            else  {
                for(int2 x = A  ; x.x >  B.x; x += new int2(1,-1)){
                    noBarrier =   CheckWalkable(x.x  ,x.y)  ;
                    if(!noBarrier) return true;
                }
            }
 

           
        }
        else{
            void UpdateIndex( ref FixedInt index){if(k>0) index++; else index--;}               
            bool checkIndex(FixedInt a, FixedInt b) => k>0? a<b: a>b;
 
           if(FixedCalculate.Abs(k)< 1){ 
                for(FixedInt x=A.x + FixedInt.half;x<B.x; x++){
                    FixedInt y = k * x + b;
                   
                    noBarrier = CheckWalkable((x-FixedInt.half).RawInt, y.round.RawInt ) 
                                && CheckWalkable((x+FixedInt.half).RawInt, y.round.RawInt);
                    if(!noBarrier) return true;
                    
                }
            }
            else if (FixedCalculate.Abs(k)>1) {
     
                for(FixedInt y=A.y + FixedInt.half;checkIndex(y,B.y);UpdateIndex(ref y) ){
                    FixedInt x = (y - b)/k;
  
                    noBarrier = CheckWalkable(x.round.RawInt, (y +FixedInt.half).RawInt ) 
                                && CheckWalkable(x.round.RawInt, (y -FixedInt.half).RawInt ) ;
                     if(!noBarrier) return true;
                
                }

            }
        }
            return false;

 
    }
    public static void Swap<T>(ref T a,ref T b){
        T c = a;
        a = b;
        b = c;
        

    }
    private static void CalulateLinerFunc(FixedVector2 A, FixedVector2 B, out FixedInt k,out FixedInt b){
           k = (B.Y - A.Y) / (B.X - A.X);
        b = A.Y -k* A.X;
    }
    private static NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode) {
        if (endNode.cameFromNodeIndex == -1) {
            // Couldn't find a path!
            return new NativeList<int2>(Allocator.Temp);
        } else {
            // Found a path
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            path.Add(new int2(endNode.x, endNode.y));

            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1) {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                path.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }

            return path;
        }
    }

    private static int CalculateDistanceCost(int2 aPosition, int2 bPosition) {
        int xDistance = math.abs(aPosition.x - bPosition.x);
        int yDistance = math.abs(aPosition.y - bPosition.y);
        int remaining = math.abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private static int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray) {
        PathNode lowestCostPathNode = pathNodeArray[openList[0]];
        for (int i = 1; i < openList.Length; i++) {
            PathNode testPathNode = pathNodeArray[openList[i]];
            if (testPathNode.fCost < lowestCostPathNode.fCost) {
                lowestCostPathNode = testPathNode;
            }
        }
        return lowestCostPathNode.index;
    }

    private static int CalculateIndex(int x, int y, int gridWidth) {
        return x + y * gridWidth;
    }

    private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) {
        return
            gridPosition.x >= 0 && 
            gridPosition.y >= 0 &&
            gridPosition.x < gridSize.x &&
            gridPosition.y < gridSize.y ;
    }


}
