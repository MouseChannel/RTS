using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

public static class SpawnUtil
{
    public static EntityArchetype UnitArchetype
    {
        get
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                unitArchetype = default(EntityArchetype);
                return unitArchetype;
            }
            if (unitArchetype != default(EntityArchetype)) return unitArchetype;

            unitArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(NonUniformScale),
                typeof(Translation),
                typeof(Rotation),
                typeof(RenderBounds),
                typeof(BuiltinMaterialPropertyUnity_WorldTransformParams),
                typeof(BlendProbeTag),
                typeof(PerInstanceCullingTag),
                typeof(WorldToLocal_Tag),
                typeof(BuiltinMaterialPropertyUnity_RenderingLayer),
                typeof(BuiltinMaterialPropertyUnity_LightData),

                typeof(_AnimInfo),
                typeof(_AnimTimeInfo),
                typeof(_AnimTextureIndex),
                typeof(_CrossfadeAnimInfo),
                typeof(_CrossfadeAnimTextureIndex),
                typeof(_CrossfadeStartTime),
                typeof(_CrossfadeEndTime)
            );
            return unitArchetype;
        }
        set
        {
            unitArchetype = value;
        }
    }
    private static EntityArchetype unitArchetype;


    public static EntityArchetype ExposedTransformArchetype
    {
        get
        {
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                exposedTransformArchetype = default(EntityArchetype);
                return exposedTransformArchetype;
            }
            if (exposedTransformArchetype != default(EntityArchetype)) return exposedTransformArchetype;

            exposedTransformArchetype = World.DefaultGameObjectInjectionWorld.EntityManager.CreateArchetype(

                typeof(Translation),
                typeof(Rotation),
                     typeof(LocalToWorld),
                typeof(LocalToParent)

            );
            return exposedTransformArchetype;
        }
        set
        {
            exposedTransformArchetype = value;
        }
    }
    private static EntityArchetype exposedTransformArchetype;

}
