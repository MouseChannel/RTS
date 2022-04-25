using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

public static class SpawnUtil
{
    public static ComponentType[] UnitArchetype
    {
        get
        {
            if (unitArchetype == null)
            {
                unitArchetype = new ComponentType[]{
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
                
                // typeof(_AnimInfo),
                // typeof(_AnimTimeInfo),
                // typeof(_AnimTextureIndex),
                // typeof(_AnimScalar),
                // typeof(_CrossfadeAnimInfo),
                // typeof(_CrossfadeAnimScalar),
                // typeof(_CrossfadeAnimTextureIndex),
                // typeof(_CrossfadeStartTime),
                // typeof(_CrossfadeEndTime),
                typeof(_AnimData),
                typeof(_CrossfadeData),
                typeof(_RenderRange),
                
                };

            }
            return unitArchetype;
        }
    }
    private static ComponentType[] unitArchetype;
    /*

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
                         typeof(LocalToWorld)
                // typeof(LocalToParent)

                );
                return exposedTransformArchetype;
            }
            set
            {
                exposedTransformArchetype = value;
            }
        }
        private static EntityArchetype exposedTransformArchetype;
        

    public static ComponentType[] ItemArchetype
    {
        get
        {
            if (itemArchetype == null)
            {
                itemArchetype = new ComponentType[]{
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
                };
            }
            return itemArchetype;
        }
    }
    private static ComponentType[] itemArchetype;
*/

 

 




}
