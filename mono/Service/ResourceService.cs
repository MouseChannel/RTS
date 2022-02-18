using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using Unity.Entities;
using System.Threading;

public class ResourceService : MonoBehaviour
{
    public static ResourceService Instance;

    private SelectionSystem selectionSystem;
    [SerializeField] public Transform MainCanvas;

    public void Init()
    {

    }
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        selectionSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SelectionSystem>();
    }

    public bool canTransition = false;
    public void AsyncLoadScene(string sceneName, Action<float> loadRate, Action loaded)
    {
        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        sceneAsync.allowSceneActivation = false;
        StartCoroutine(LoadSceneProgress(sceneAsync, loadRate, loaded));
    }

    IEnumerator LoadSceneProgress(AsyncOperation sceneAsync, Action<float> loadRate, Action loaded)
    {

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

    private void Update()
    {

    }
    public void Destroy(GameObject go)
    {
        UnityEngine.Object.Destroy(go);
    }
    private Dictionary<string, GameObject> goDic = new Dictionary<string, GameObject>();

    public void LoadMainWindow<T>(string path, ref T windowScript)
    {

        GameObject prefab = Resources.Load<GameObject>(path);

        GameObject go = null;
        if (prefab != null)
        {
            go = Instantiate(prefab, MainCanvas, false);
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
            go = Instantiate(prefab, MainCanvas, false);
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
            go = Instantiate(prefab);
        }
        return go;
    }

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



    #region selection box 

    private Texture2D _whiteTexture;

    private Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.2f));
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }

    public Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }
    public void OnGUI()
    {

        if (selectionSystem.IsDragging)
        {
            var rect = GetScreenRect(selectionSystem.mouseStartPos, Input.mousePosition);
            GUI.DrawTexture(rect, WhiteTexture);
        }

    }


    #endregion
}
