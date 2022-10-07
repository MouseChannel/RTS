using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class FOWMono : MonoBehaviour
{
    private FOWSystem fOWSystem;
    Material material;
    void Start()
    {
        fOWSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<FOWSystem>();
        material = GetComponent<Renderer>().material;
        GetComponent<Transform>().localScale = new Vector3(StaticData.gridWidth / 10, 1, StaticData.gridWidth / 10);
    }

    // Update is called once per frame
    void Update()
    {
        if (fOWSystem.FogTexture != null)
        {

            material.SetTexture("_MainTex", fOWSystem.FogTexture);
        }
    }
}
