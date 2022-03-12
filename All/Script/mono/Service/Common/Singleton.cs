using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public abstract class Singleton<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)Activator.CreateInstance(typeof(T), true);
                (instance as Singleton<T>).InitInstance();
                return instance;
            }
            return instance;
        }
    }

    protected Q GetSystem<Q>() where Q: SystemBase{
        return World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<Q>();
    }

    public abstract void InitInstance();








}


public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get {
            if(applicationIsQuitting){
                return null;
            }
            return instance;

        }

    }
    private static bool applicationIsQuitting = false;

    private void Awake()
    {
        instance = this as T;
        InitInstance();
        DontDestroyOnLoad(this);

    }
    public virtual void OnDestroy() {
        if(instance == this)
            applicationIsQuitting = true;
    }
    public virtual void InitInstance()
    {
    }
}



