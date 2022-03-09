using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using FixedMath;

 
public class ViewUnit : MonoBehaviour
{
    [HideInInspector] public Entity entity;
    public UnityEngine.Vector2 poss;
    void Start(){
        ResponseNetSystem.Instance.allGameobject.Add(this);
    }


    void Update()
    {
        if (entity == Entity.Null) return;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var agent = entityManager.GetComponentData<Agent>(entity);
        var pos = agent.position_;
        poss = new UnityEngine.Vector2(pos.X.RawFloat, pos.Y.RawFloat);
        var dir = agent.velocity_;
        // Debug.Log(dir);
        transform.position = Vector3.Lerp(transform.position, new Vector3(pos.X.RawFloat, 0, pos.Y.RawFloat), Time.deltaTime * 4);
        if ( FixedCalculate.absSq(dir) > (FixedInt)0.001)
            transform.forward = Vector3.Lerp(transform.forward, new Vector3(dir.X.RawFloat, 0, dir.Y.RawFloat), Time.deltaTime * 20);

        
    }



}
