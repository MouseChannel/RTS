using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class test : MonoBehaviour
{
    FOWSystem f;
    public Material m;
    void Start()
    {
        f = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FOWSystem>();
        m = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if(f.FogTexture != null){
         
            m.SetTexture("_MainTex", f.FogTexture);
        }
    }
}
