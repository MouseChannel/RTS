using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

[DisableAutoCreation]
public class PathFollowSystem : JobComponentSystem {

    private Unity.Mathematics.Random random;

    protected override void OnCreate() {
        random = new Unity.Mathematics.Random(56);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;
        Debug.Log("job");
        return Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer, ref Translation translation, ref PathFollow pathFollow) => {
            if (pathFollow.pathIndex >= 0) {
                // Has path to follow
                PathPosition pathPosition = pathPositionBuffer[pathFollow.pathIndex];

                float3 targetPosition = new float3(pathPosition.position.x, pathPosition.position.y, 0);
                float3 moveDir = math.normalizesafe(targetPosition - translation.Value);
                float moveSpeed = 3f;

                translation.Value += moveDir * moveSpeed * deltaTime;
                
                if (math.distance(translation.Value, targetPosition) < .1f) {
                    // Next waypoint
                    pathFollow.pathIndex--;
                }
            }
            
        }).Schedule(inputDeps);
    }

    private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, GridInit.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, GridInit.Instance.pathfindingGrid.GetHeight() - 1);
    }

}

[UpdateAfter(typeof(PathFollowSystem))]
[DisableAutoCreation]
public class PathFollowGetNewPathSystem : JobComponentSystem {
    
    private Unity.Mathematics.Random random;

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        random = new Unity.Mathematics.Random(56);

        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        int mapWidth = GridInit.Instance.pathfindingGrid.GetWidth();
        int mapHeight = GridInit.Instance.pathfindingGrid.GetHeight();
        float3 originPosition = float3.zero;
        float cellSize = GridInit.Instance.pathfindingGrid.GetCellSize();
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(this.random.NextUInt(1, 10000));
        
        EntityCommandBuffer.ParallelWriter entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        JobHandle jobHandle = Entities.WithNone<PathFindParams>().ForEach((Entity entity, int entityInQueryIndex, in PathFollow pathFollow, in Translation translation) => { 
            if (pathFollow.pathIndex == -1) {
                
                GetXY(translation.Value + new float3(1, 1, 0) * cellSize * +.5f, originPosition, cellSize, out int startX, out int startY);

                ValidateGridPosition(ref startX, ref startY, mapWidth, mapHeight);

                int endX = random.NextInt(0, mapWidth);
                int endY = random.NextInt(0, mapHeight);

                entityCommandBuffer.AddComponent(entityInQueryIndex, entity, new PathFindParams { 
                    startPosition = new int2(startX, startY), endPosition = new int2(endX, endY) 
                });
            }
        }).Schedule(inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

    private static void ValidateGridPosition(ref int x, ref int y, int width, int height) {
        x = math.clamp(x, 0, width - 1);
        y = math.clamp(y, 0, height - 1);
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y) {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }

}
