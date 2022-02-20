using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using Unity.Entities;
using System.Threading;

public class ResourceService : Singleton<ResourceService>
{


    [SerializeField] public Transform MainCanvas;

    public override void InitInstance()
    {    }



    public bool canTransition = false;
    private Action loadSceneAction;
    private Action transitionAction;
    public void AsyncLoadScene(string sceneName, Action<float> loadRate, Action loaded, Action changedDone)
    {
        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        sceneAsync.completed += (sceneAsync) =>
        {
            changedDone?.Invoke();
            // loaded?.Invoke();
        };
        sceneAsync.allowSceneActivation = false;
        GameRoot.Instance.MyStartCoroutine(LoadSceneProgress(sceneAsync, loadRate, loaded, changedDone));


    }

    IEnumerator LoadSceneProgress(params object[] args)
    {
        var sceneAsync = args[0] as AsyncOperation;
        var loadRate = args[1] as Action<float>;
        var loaded = args[2] as Action;
        var changedDone = args[3] as Action;


        while (sceneAsync.progress < 0.9f)
        {
            yield return null;
            loadRate?.Invoke(sceneAsync.progress);
        }
        loaded?.Invoke();
        while (!canTransition)
        {
            yield return null;
        }
        canTransition = false;
        sceneAsync.allowSceneActivation = true;




    }

    public void Destroy(GameObject go)
    {
        UnityEngine.Object.Destroy(go);
    }
    private Dictionary<string, GameObject> goDic = new Dictionary<string, GameObject>();

    public void LoadMainWindow<T>(string path, ref T windowScript)
    {
        Debug.Log("load main window");
        GameObject prefab = Resources.Load<GameObject>(path);

        GameObject go = null;
        if (prefab != null)
        {
            go = UnityEngine.Object.Instantiate(prefab, MainCanvas, false);
        }
        if (go.TryGetComponent<WindowRoot>(out WindowRoot window))
        {
            window.InitWindow();
        }
        windowScript = go.GetComponent<T>();

    }


    public void LoadSubWindow<T>(string path, int x, int y, ref T windowScript)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        GameObject go = null;
        if (prefab != null)
        {
            go = UnityEngine.Object.Instantiate(prefab, MainCanvas, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(x, y);
        }
        if (go.TryGetComponent<WindowRoot>(out WindowRoot window))
        {
            window.InitWindow();
        }
        windowScript = go.GetComponent<T>();

    }



    public GameObject LoadPrefab(string path, bool cache = false)
    {
        GameObject prefab = null;
        if (!goDic.TryGetValue(path, out prefab))
        {

            prefab = Resources.Load<GameObject>(path);

            if (cache)
            {
                goDic.Add(path, prefab);
            }
        }

        GameObject go = null;
        if (prefab != null)
        {
            go = UnityEngine.Object.Instantiate(prefab);
        }
        return go;
    }

    // public void LoadPrefab<T>(string path, ref T component)
    // {
    //     GameObject prefab = null;


    //     prefab = Resources.Load<GameObject>(path);




    //     GameObject go = null;
    //     if (prefab != null)
    //     {
    //         go = UnityEngine.Object.Instantiate(prefab);
    //     }
    //     component = go.GetComponent<>
    // }


    private Dictionary<string, AudioClip> adDic = new Dictionary<string, AudioClip>();
    public AudioClip LoadAudio(string path, bool cache = false)
    {
        AudioClip au = null;
        if (!adDic.TryGetValue(path, out au))
        {
            au = Resources.Load<AudioClip>(path);
            if (cache)
            {
                adDic.Add(path, au);
            }
        }
        return au;
    }





    public Sprite LoadSprite(string path, bool cache = false)
    {
        Sprite sprite = null;
        sprite = Resources.Load<Sprite>(path);
        return sprite;
    }

    public Material LoadMaterial(string path, bool cache = false)
    {
        Material material = null;
        material = Resources.Load<Material>(path);
        return material;

    }




}
