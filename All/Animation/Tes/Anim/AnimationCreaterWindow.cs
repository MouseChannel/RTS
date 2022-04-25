using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using Unity.Mathematics;


public class AnimationCreaterWindow : EditorWindow
{

    [MenuItem("Window/AnimationCreaterWindow")]
    private static void ShowWindow()
    {
        var window = GetWindow<AnimationCreaterWindow>();
        window.titleContent = new GUIContent("AnimationCreaterWindow");
        window.Show();
    }
    private GameObject prefab;
    private UnitScriptableObject unit;
    private Animator animator;
    private Avatar animAvatar;
    private RuntimeAnimatorController animController;
    private List<AnimationClip> customClips = new List<AnimationClip>();
    private bool customCompression;
    private int fps = 60;
    private int globalBake;
    private int previousGlobalBake;
    private ShaderTextureSize selectedTextureSize;
    private ShaderTextureQuality selectedTextureQuality;
    private string outputPath;
    private float compressionAccuracy = 1;
    private bool shaderGraphSupport = true;
    private List<KeyValuePair<int, float>> lodDistances = new List<KeyValuePair<int, float>>();
    private List<MeshFilter> meshFilters = new List<MeshFilter>();
    private List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();
    private GameObject spawnedAsset;
    private GameObject previousPrefab;
    private Object outputFolder;
    private Dictionary<string, bool> bakeAnims = new Dictionary<string, bool>();

    private Dictionary<string, int> frameSkips = new Dictionary<string, int>();
    private Vector2 scroll;
    private static readonly List<ShaderTextureSize> textureSizes = System.Enum.GetValues(typeof(ShaderTextureSize)).Cast<ShaderTextureSize>().ToList();
    private static readonly string[] textureSizeNames = System.Enum.GetNames(typeof(ShaderTextureSize)).Select(x => x.Remove(0, 5).Replace("__", " - ").Replace("_", " ")).ToArray();
    private static readonly List<ShaderTextureQuality> textureQualities = System.Enum.GetValues(typeof(ShaderTextureQuality)).Cast<ShaderTextureQuality>().ToList();
    private static readonly string[] textureQualityNames = System.Enum.GetNames(typeof(ShaderTextureQuality)).Select(x => x.Remove(0, 8).Replace("__", " - ").Replace("_", " ")).ToArray();


    private List<AnimationClip> clipsCache = new List<AnimationClip>();
    private bool requiresAnimator;
    private bool isHumanoid;
    private List<AnimationScriptableObject> createdAnimations = new List<AnimationScriptableObject>();
    private List<Texture2D> createdTextures = new List<Texture2D>();
    private List<AnimationStuff> animationStuffs = new List<AnimationStuff>();
    private List<string> ItemName = new List<string>();
    private enum ShaderTextureSize
    {
        Size_Smallest_Common = 0,
        Size_Largest_Common = 1,
        Size_Most_Common = 2,
        Size_32 = 32,
        Size_64 = 64,
        Size_128 = 128,
        Size_256 = 256,
        Size_512 = 512,
        Size_1024 = 1024,
        Size_2048 = 2048,
        Size_4096 = 4096
    }
    private enum ShaderTextureQuality
    {
        Quality_8_Bit__Low = TextureFormat.RGBA32,
        Quality_16_Bit__Medium = TextureFormat.RGBAHalf,
        Quality_32_Bit__High = TextureFormat.RGBAFloat,
    }

    private void OnGUI()
    {
        if (GUILayout.Button("刷新信息", GUILayout.Height(30)))
        {
            OnPrefabChanged();

        }
        using (new EditorGUILayout.HorizontalScope())
        {
            prefab = EditorGUILayout.ObjectField("Asset to Bake", prefab, typeof(GameObject), true) as GameObject;
        }
        // outputFolder = EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(Object), false);
        // if (outputFolder != null)
        // {
        //     outputPath = AssetDatabase.GetAssetPath(outputFolder);
        // }


        // using (new GUILayout.ScrollViewScope(scroll))
        // {

        // }
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Animation setup");
        }
        // EditorGUI.BeginChangeCheck();
        if (prefab != null)
        {


            if (previousPrefab != prefab)
                OnPrefabChanged();
            if (spawnedAsset == null)
                OnPrefabChanged();

            animController = EditorGUILayout.ObjectField("Animation Controller", animController, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;
            animAvatar = EditorGUILayout.ObjectField("Avatar", animAvatar, typeof(Avatar), true) as Avatar;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Animations");
            }
            var clipNames = bakeAnims.Keys.ToArray();
            bool modified = false;
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Select All", GUILayout.Width(100)))
                {
                    foreach (var clipName in clipNames)
                        bakeAnims[clipName] = true;
                }
                if (GUILayout.Button("Deselect All", GUILayout.Width(100)))
                {
                    foreach (var clipName in clipNames)
                        bakeAnims[clipName] = false;
                }
            }
            GUILayout.EndHorizontal();


            foreach (var clipName in clipNames)
            {
                if (frameSkips.ContainsKey(clipName) == false)
                    frameSkips.Add(clipName, globalBake);
                float frameSkip = 1;
                AnimationClip clip = clipsCache.Find(q => q.name == clipName);

                int framesToBake = clip ? (int)(clip.length * fps / frameSkip) : 0;

                GUILayout.BeginHorizontal();
                {
                    bakeAnims[clipName] = EditorGUILayout.Toggle(bakeAnims[clipName], GUILayout.MaxWidth(40));
                    GUI.enabled = bakeAnims[clipName];

                    GUILayout.Space(10);
                    GUI.enabled = true;
                    GUILayout.Label(string.Format("{0} ({1} frames)", clipName, framesToBake));
                }
                GUILayout.EndHorizontal();
                if (framesToBake > 500)
                {
                    GUI.skin.label.richText = true;
                    EditorGUILayout.LabelField("<color=red>Long animations degrade performance, consider using a higher frame skip value.</color>", GUI.skin.label);
                }

            }
            //meshfilter
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Meshfilter Setup");
            }
            for (int i = 0; i < meshFilters.Count; i++)
            {
                bool remove = false;
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.ObjectField("Mesh Filter " + i, meshFilters[i], typeof(MeshFilter), true);
                    if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                        remove = true;
                }
                GUILayout.EndHorizontal();
                if (remove)
                {
                    meshFilters.RemoveAt(i);
                    break;
                }
            }
            if (GUILayout.Button("+ Add MeshFilter"))
                meshFilters.Add(null);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Bake Preferences Setup");
            }

            int selectedIndex = textureSizes.IndexOf(selectedTextureSize);
            if (selectedIndex == -1)
                selectedIndex = 0;
            selectedIndex = EditorGUILayout.Popup("Bake Texture Size", selectedIndex, textureSizeNames);
            selectedTextureSize = textureSizes[selectedIndex];

            selectedIndex = textureQualities.IndexOf(selectedTextureQuality);
            if (selectedIndex == -1)
                selectedIndex = 0;
            selectedIndex = EditorGUILayout.Popup("Bake Texture Quality", selectedIndex, textureQualityNames);
            selectedTextureQuality = textureQualities[selectedIndex];

            if (GUILayout.Button("生成", GUILayout.Height(30)))
            {
                unit = ScriptableObject.CreateInstance<UnitScriptableObject>();

                animationStuffs.Clear();


                UnityEditor.AssetDatabase.CreateAsset(unit, "Assets/Resources/aaa.asset");
                FinalBuild();
                foreach(var i in createdAnimations){
                    if(unit.animations == null){
                        unit.animations = new List<AnimationScriptableObject>();
                    }
                    unit.animations.Add(i);
                }


            }


        }

    }

    private void FinalBuild()
    {
        HashSet<string> allAssets = new HashSet<string>();
        var clips = GetClips();
        foreach (var clip in clips)
            allAssets.Add(AssetDatabase.GetAssetPath(clip));
        var sampleGO = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        animator = sampleGO.GetComponent<Animator>();
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.applyRootMotion = false;
        int vertexCount = 0;
        createdAnimations.Clear();
        createdTextures.Clear();




        foreach (AnimationClip animClip in clips)
        {
            if (bakeAnims.ContainsKey(animClip.name) && bakeAnims[animClip.name] == false) continue;
            // var tempAnimS = new AnimationScriptableObject();
            // tempAnimS.name = animClip.name;
            // tempAnimS.length = animClip.length;
            int frameSkip = 1;
            int bakeFrames = Mathf.CeilToInt(animClip.length * fps / (float)frameSkip);

            int frame = 0;

            List<Vector3> meshesInFrame = new List<Vector3>();
            List<Vector3> normalsInFrame = new List<Vector3>();
            float lastFrameTime = 0;
            List<List<Vector3>> framePositions = new List<List<Vector3>>();
            List<List<Vector3>> frameNormals = new List<List<Vector3>>();


            for (int i = 0; i <= bakeFrames; i++)
            {
                float bakeDelta = Mathf.Clamp01((float)i / bakeFrames);
                // EditorUtility.DisplayProgressBar("Baking Animation", string.Format("Processing: {0} Frame: {1}", animClip.name, i), bakeDelta);
                float animationTime = bakeDelta * animClip.length;
                {
                    GameObject sampleObject = sampleGO;
                    Animation legacyAnimation = sampleObject.GetComponentInChildren<Animation>();
                    if (animator && animator.gameObject != sampleObject)
                        sampleObject = animator.gameObject;
                    else if (legacyAnimation && legacyAnimation.gameObject != sampleObject)
                        sampleObject = legacyAnimation.gameObject;
                    animClip.SampleAnimation(sampleObject, animationTime);
                }
                meshesInFrame.Clear();
                normalsInFrame.Clear();


                Mesh m = null;
                List<MeshFilter> sampleMeshFilters = new List<MeshFilter>();
                List<SkinnedMeshRenderer> sampleSkinnedRenderers = new List<SkinnedMeshRenderer>();

                for (int j = 0; j < meshFilters.Count; j++)
                {
                    var sampleMF = FindMatchingTransform(prefab.transform, meshFilters[j].transform, sampleGO.transform).GetComponent<MeshFilter>();
                    sampleMeshFilters.Add(sampleMF);
                    // var sampleMR = sampleMF.gameObject.GetComponent<MeshRenderer>();
                    // bool filterEnabled = sampleMR == null || sampleMR.enabled;
                    // m = Instantiate(sampleMF.sharedMesh) as Mesh;
                    // Vector3[] v = m.vertices;
                    // Vector3[] n = m.normals;
                    // for (int vIndex = 0; vIndex < v.Length; vIndex++)
                    // {
                    //     if (!filterEnabled)
                    //     {
                    //         v[vIndex] = sampleMR.bounds.center;
                    //     }
                    //     else
                    //     {
                    //         v[vIndex] = sampleMF.transform.TransformPoint(v[vIndex]);
                    //     }

                    //     if (selectedAnimatorType == MeshAnimationCreator.MeshAnimatorType.ShaderAnimated &&
                    //        meshNormalMode == MeshNormalMode.UseOriginal)
                    //     {
                    //         n[vIndex] = Vector3.zero;
                    //     }
                    //     else
                    //     {
                    //         n[vIndex] = sampleMF.transform.TransformDirection(n[vIndex]);
                    //     }
                    // }
                    // meshesInFrame.AddRange(v);
                    // normalsInFrame.AddRange(n);
                    // DestroyImmediate(m);
                }
                for (int j = 0; j < skinnedRenderers.Count; j++)
                {
                    var sampleSR = FindMatchingTransform(prefab.transform, skinnedRenderers[j].transform, sampleGO.transform).GetComponent<SkinnedMeshRenderer>();
                    sampleSkinnedRenderers.Add(sampleSR);
                    bool filterEnabled = sampleSR.enabled;
                    m = new Mesh();
                    sampleSR.BakeMesh(m);
                    Vector3[] v = m.vertices;
                    Vector3[] n = m.normals;
                    sampleSR.transform.localScale = Vector3.one;
                    for (int vIndex = 0; vIndex < v.Length; vIndex++)
                    {
                        if (!filterEnabled)
                        {
                            v[vIndex] = sampleSR.bounds.center;
                        }
                        else
                        {
                            v[vIndex] = sampleSR.transform.TransformPoint(v[vIndex]);
                        }

                        // if (selectedAnimatorType == MeshAnimationCreator.MeshAnimatorType.ShaderAnimated &&
                        //    meshNormalMode == MeshNormalMode.UseOriginal)
                        // {
                        //     n[vIndex] = Vector3.zero;
                        // }
                        // else
                        {
                            n[vIndex] = sampleSR.transform.TransformDirection(n[vIndex]);
                        }
                    }
                    meshesInFrame.AddRange(v);
                    normalsInFrame.AddRange(n);
                    DestroyImmediate(m);
                }





                var combinedInFrame = GenerateCombinedMesh(sampleMeshFilters, sampleSkinnedRenderers);
                meshesInFrame = combinedInFrame.vertices.ToList();
                normalsInFrame = combinedInFrame.normals.ToList();

                // Transform rootMotionBaker = new GameObject().transform;
                // // --- 
                // rootMotionBaker.position = animator.rootPosition;
                // rootMotionBaker.rotation = animator.rootRotation;
                // for (int j = 0; j < meshesInFrame.Count; j++)
                // {
                //     meshesInFrame[j] = rootMotionBaker.TransformPoint(meshesInFrame[j]);
                // }
                // ---
                DestroyImmediate(combinedInFrame);
                vertexCount = meshesInFrame.Count;










                // Instantiate(sampleGO, frame * Vector3.right, Quaternion.identity);
                framePositions.Add(meshesInFrame.ToList());
                frameNormals.Add(normalsInFrame.ToList());
                frame++;











            }
            // tempAnimS.animationName = animClip.name;
            var animationStuff = new AnimationStuff(
                framePositions,
                frameNormals,
                unit,
                animClip.name,
                animClip.length
            );
            animationStuffs.Add(animationStuff);
        }



        // createdAnimations.Add(tempAnimS);
        // EditorUtility.SetDirty(tempAnimS);



        object[] parameters = null;
        {
            Dictionary<int2, int> commonCounts = new Dictionary<int2, int>();
            foreach (var anim in animationStuffs)
            {
                if (!commonCounts.ContainsKey(anim.textureSize))
                    commonCounts.Add(anim.textureSize, 1);
                else
                    commonCounts[anim.textureSize]++;
            }
            int2 textureSize;
            // Debug.Log(commonCounts.Count);
            switch (selectedTextureSize)
            {
                case ShaderTextureSize.Size_Smallest_Common:
                    textureSize = commonCounts.OrderBy(x => x.Key.x).First().Key;
                    break;
                case ShaderTextureSize.Size_Largest_Common:
                    textureSize = commonCounts.OrderByDescending(x => x.Key.x).First().Key;
                    break;
                case ShaderTextureSize.Size_Most_Common:
                    textureSize = commonCounts.OrderByDescending(x => x.Value).First().Key;
                    break;
                default:
                    textureSize = new int2((int)selectedTextureSize, (int)selectedTextureSize);
                    break;
            }
            // Debug.Log(textureSize);
            foreach (var anim in animationStuffs)
                anim.textureSize = textureSize;
            Debug.Log(textureSize);
            parameters = new object[] { (int)selectedTextureQuality, compressionAccuracy };





        }



        foreach (var anim in animationStuffs)
        {
            anim.CompleteBake(1, parameters);

            // var temp = ScriptableObject.CreateInstance<AnimationScriptableObject>();
            var temp = new AnimationScriptableObject();
            temp.animationName = anim.animationName;
            temp.name = anim.animationName;
            temp.animScalar = anim.animScalar;
            Debug.Log(anim.animScalar);

            temp.textureCount = anim.textureCount;
            temp.textureSize = anim.textureSize;
            temp.length = anim.length;
            temp.textures = anim.textures.ToList();
            temp.vertexCount = anim.vertexCount;
            temp.totalFrames = anim.totalFrames;
            createdAnimations.Add(temp);

            // // Debug.Log(temp.textures.Count);
            UnityEditor.AssetDatabase.AddObjectToAsset(temp, unit);
            // foreach (var i in temp.textures)
            // {
            //     UnityEditor.AssetDatabase.AddObjectToAsset(i, unit);
            // }
            // var e = new Mesh();
            // UnityEditor.AssetDatabase.AddObjectToAsset(e, unit);





            EditorUtility.SetDirty(temp);
        }
        var newMesh = new Mesh();
        newMesh.name = "MainMesh";
        // var combineMesh = GenerateCombinedMesh();
        TransferMesh(GenerateCombinedMesh(), newMesh);
        UnityEditor.AssetDatabase.AddObjectToAsset(newMesh, unit);
        unit.mainMesh = newMesh;



        UnityEditor.AssetDatabase.SaveAssets();











    }

    #region  bake Stuff
    private int vertexCount;
    private List<List<Vector3>> frameBakeData;
    private List<List<Vector3>> normalBakeData;
    private Vector2Int textureSize;



    public void CreateBakedAssets(string path, List<List<Vector3>> framePositions, List<List<Vector3>> frameNormals)
    {
        vertexCount = framePositions[0].Count;
        frameBakeData = framePositions;
        normalBakeData = frameNormals;
        int TEX_SIZE = 1024;
        int frameVertexCount = vertexCount * frameBakeData.Count * 2;
        while (TEX_SIZE * TEX_SIZE > frameVertexCount)
        {
            if (TEX_SIZE * TEX_SIZE / 2 < frameVertexCount)
                break;
            TEX_SIZE /= 2;
        }
        textureSize = new Vector2Int(TEX_SIZE, TEX_SIZE);
    }
    private Vector3 animScalar;
    private Texture2D[] textures;
    private int textureCount;
    private Mesh GenerateCombinedMesh()
    {
        GameObject tempCombinePrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        List<MeshFilter> combineMeshFilters = new List<MeshFilter>();
        List<SkinnedMeshRenderer> combineSkinnedRenderers = new List<SkinnedMeshRenderer>();
        for (int j = 0; j < meshFilters.Count; j++)
        {
            var sampleMF = FindMatchingTransform(prefab.transform, meshFilters[j].transform, tempCombinePrefab.transform).GetComponent<MeshFilter>();
            combineMeshFilters.Add(sampleMF);
        }
        for (int j = 0; j < skinnedRenderers.Count; j++)
        {
            var sampleSR = FindMatchingTransform(prefab.transform, skinnedRenderers[j].transform, tempCombinePrefab.transform).GetComponent<SkinnedMeshRenderer>();
            combineSkinnedRenderers.Add(sampleSR);
        }
        Mesh combinedMesh = GenerateCombinedMesh(combineMeshFilters, combineSkinnedRenderers, true);
        DestroyImmediate(tempCombinePrefab);
        return combinedMesh;
    }
    private void TransferMesh(Mesh from, Mesh to)
    {
        to.vertices = from.vertices;
        to.subMeshCount = from.subMeshCount;
        for (int i = 0; i < from.subMeshCount; i++)
        {
            to.SetTriangles(from.GetTriangles(i), i);
        }
        to.normals = from.normals;
        to.tangents = from.tangents;
        to.colors = from.colors;
        to.uv = from.uv;
        to.uv2 = from.uv2;
        to.uv3 = from.uv3;
        to.uv4 = from.uv4;
    }

    public Mesh GenerateCombinedMesh(List<MeshFilter> filters, List<SkinnedMeshRenderer> renderers, bool lastone = false)
    {

        int totalMeshes = filters.Count + renderers.Count;
        List<Mesh> tempMeshes = new List<Mesh>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        int vert = 0;
        foreach (SkinnedMeshRenderer sr in renderers)
        {
            Material[] materials = sr.sharedMaterials.Where(q => q != null).ToArray();

            if (sr == null || sr.sharedMesh == null)
                continue;

            for (int i = 0; i < sr.sharedMesh.subMeshCount; i++)
            {
                // // Debug.Log("skinnemEsh" + i);
                Mesh t = new Mesh();
                sr.BakeMesh(t);
                tempMeshes.Add(t);
                var m = sr.transform.localToWorldMatrix;
                Matrix4x4 scaledMatrix = Matrix4x4.TRS(MatrixUtils.GetPosition(m), MatrixUtils.GetRotation(m), sr.enabled ? Vector3.one : Vector3.zero);
                combineInstances.Add(new CombineInstance()
                {
                    mesh = t,
                    transform = scaledMatrix,
                    subMeshIndex = i

                });
                // vert += t.vertexCount;
                // Debug.Log(t.vertexCount);
            }
        }
        int index = 0;
        if (lastone)
        {




            if (unit.renderRange == null)
                unit.renderRange = new List<Item>();
            unit.renderRange.Add(new Item
            {
                name = "MainMesh",
                renderRange = new int2(0, combineInstances[0].mesh.vertexCount)
            });
            index += combineInstances[0].mesh.vertexCount;

        }



        foreach (MeshFilter mf in filters)
        {
            Material[] materials = new Material[0];
            if (mf == null)
                continue;
            Mesh m = mf.sharedMesh;
            // if (lastone)
            //     // Debug.Log(m.name);
            if (m == null) m = mf.mesh;
            if (m == null)
                continue;
            var mr = mf.GetComponent<MeshRenderer>();
            if (mr)
            {
                materials = mr.sharedMaterials.Where(q => q != null).ToArray();
            }
            var matrix = mf.transform.localToWorldMatrix;
            if (mr && !mr.enabled)
            {
                matrix = Matrix4x4.zero;
            }
            for (int i = 0; i < m.subMeshCount; i++)
            {
                combineInstances.Add(new CombineInstance()
                {
                    mesh = m,
                    transform = matrix,
                    subMeshIndex = i

                });
            }

            if (lastone)
            {
                int tem = index;
                index += combineInstances.Last().mesh.vertexCount;
                unit.renderRange.Add(new Item
                {
                    name = mf.name,
                    renderRange = new int2(tem, index)
                });
            }

        }
        var mesh = new Mesh();

        // foreach (var i in combineInstances)
        // {
        //     if (unit.renderRange == null)
        //         unit.renderRange = new List<int2>();

        //     index += i.mesh.vertexCount;

        // }


        mesh.CombineMeshes(combineInstances.ToArray(), true, true);
        Debug.Log(mesh.vertexCount);
        // mesh.CombineMeshes(combines);
        //----
        // CombineInstance[] finalCombines = materialMeshes.Select(q => new CombineInstance() { mesh = q.Value }).ToArray();
        // mesh = new Mesh();

        // mesh.CombineMeshes(finalCombines, false, false);

        mesh.RecalculateBounds();

        foreach (Mesh m in tempMeshes)
        {
            DestroyImmediate(m);
        }


        var vertexIndexUvs = new Vector2[mesh.vertexCount];
        for (int i = 0; i < vertexIndexUvs.Length; i++)
        {
            vertexIndexUvs[i] = new Vector2(i, 0);
        }
        mesh.uv4 = vertexIndexUvs;

        return mesh;






    }



    private Transform FindMatchingTransform(Transform parent, Transform source, Transform newParent)
    {
        List<int> stepIndexing = new List<int>();
        while (source != parent && source != null)
        {
            if (source.parent == null)
                break;
            for (int i = 0; i < source.parent.childCount; i++)
            {
                if (source.parent.GetChild(i) == source)
                {
                    stepIndexing.Add(i);
                    source = source.parent;
                    break;
                }
            }
        }
        stepIndexing.Reverse();
        for (int i = 0; i < stepIndexing.Count; i++)
        {
            newParent = newParent.GetChild(stepIndexing[i]);
        }
        return newParent;
    }

    public class AnimationStuff
    {
        private List<List<Vector3>> frameBakeData;
        private List<List<Vector3>> normalBakeData;
        public int vertexCount;
        public int2 textureSize;
        public Vector3 animScalar = Vector3.one;
        public Texture2D[] textures;
        public int textureCount;
        public UnitScriptableObject unit;
        public string animationName;
        public int totalFrames;
        public float length;

        public AnimationStuff(List<List<Vector3>> _frameBakeData,
                                List<List<Vector3>> _normalBakeData,
                                UnitScriptableObject _unit,
                                string _animationName,
                                float _length)
        {
            unit = _unit;
            animationName = _animationName;
            length = _length;


            frameBakeData = _frameBakeData;
            normalBakeData = _normalBakeData;
            vertexCount = frameBakeData[0].Count;
            int TEX_SIZE = 1024;
            int frameVertexCount = vertexCount * frameBakeData.Count * 2;
            while (TEX_SIZE * TEX_SIZE > frameVertexCount)
            {
                if (TEX_SIZE * TEX_SIZE / 2 < frameVertexCount)
                    break;
                TEX_SIZE /= 2;
            }
            textureSize = new int2(TEX_SIZE, TEX_SIZE);
            totalFrames = _frameBakeData.Count();
            // Debug.Log("init" + totalFrames);





        }
        public void CompleteBake(int a = 1, params object[] parameters)
        {
            float compressionAccuracy = (float)parameters[1];
            if (frameBakeData.Count > 0)
                vertexCount = frameBakeData[0].Count;
            double[][] offsets = new double[vertexCount * frameBakeData.Count][];
            double[] scaler = new double[3];
            // generate texture
            for (int frameIndex = 0; frameIndex < frameBakeData.Count; frameIndex++)
            {
                var meshFrame = frameBakeData[frameIndex];
                var meshNormal = normalBakeData[frameIndex];
                for (int vert = 0; vert < meshFrame.Count; vert++)
                {
                    int arrayPos = (frameIndex * meshFrame.Count) + vert;
                    var framePos = meshFrame[vert];
                    Vector3 frameNormal = Vector3.zero;
                    if (meshNormal.Count > vert)
                    {
                        frameNormal = meshNormal[vert];
                    }
                    double[] data = new double[6]
                    {
                        framePos.x,
                        framePos.y,
                        framePos.z,
                        frameNormal.x,
                        frameNormal.y,
                        frameNormal.z
                    };
                    if (compressionAccuracy != 1)
                    {
                        data[0] = System.Math.Round(data[0] * compressionAccuracy) / compressionAccuracy;
                        data[1] = System.Math.Round(data[1] * compressionAccuracy) / compressionAccuracy;
                        data[2] = System.Math.Round(data[2] * compressionAccuracy) / compressionAccuracy;
                    }
                    offsets[arrayPos] = data;

                    for (int s = 0; s < 3; s++)
                    {
                        // Debug.Log(data[s]);
                        if (System.Math.Abs(data[s]) > scaler[s])
                            scaler[s] = System.Math.Abs(data[s]);
                    }
                }
            }
            animScalar = new Vector3((float)scaler[0], (float)scaler[1], (float)scaler[2]);

            List<Texture2D> bakeTextures = new List<Texture2D>();
            int xPos = 0;
            int yPos = 0;
            int textureIndex = 0;
            int frame = 0;
            int pixelsLeft = textureSize.x * textureSize.y;
            int verticesLeftInFrame = vertexCount * 2;
            for (int vert = 0; vert < offsets.Length; vert++)
            {
                double[] data = offsets[vert];
                if (data == null)
                    continue;
                for (int s = 0; s < data.Length; s++)
                {
                    if (s < 3)
                    {
                        if (scaler[s] != 0)
                            data[s] /= scaler[s];
                    }
                    // convert all negatives to positives
                    data[s] = data[s] * 0.5d + 0.5d;
                }

                for (int c = 0; c < data.Length; c += 3)
                {
                    Color color = new Color((float)data[c + 0], (float)data[c + 1], (float)data[c + 2], 1);
                    if (yPos == textureSize.y)
                    {
                        xPos++;
                        yPos = 0;
                        if (xPos == textureSize.x)
                        {
                            xPos = 0;
                            textureIndex++;
                            pixelsLeft = textureSize.x * textureSize.y;
                        }
                    }
                    if (bakeTextures.Count <= textureIndex)
                    {
                        bakeTextures.Add(new Texture2D(textureSize.x, textureSize.y, (TextureFormat)(int)parameters[0], false, false));
                    }
                    var bakeTexture = bakeTextures[textureIndex];
                    bakeTexture.SetPixel(xPos, yPos, color);
                    yPos++;

                    pixelsLeft--;
                    verticesLeftInFrame--;
                    // advance texture if whole frame next doesn't fit on it
                    if (verticesLeftInFrame == 0)
                    {
                        verticesLeftInFrame = vertexCount * 2;
                        frame++;
                        if (pixelsLeft < vertexCount * 2)
                        {
                            textureIndex++;
                            pixelsLeft = textureSize.x * textureSize.y;
                            xPos = 0;
                            yPos = 0;
                        }
                    }
                }
            }
            // string path = UnityEditor.AssetDatabase.GetAssetPath(this);
            // var existingTextures = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path).Where(a => a is Texture2D).ToArray();
            for (int t = 0; t < bakeTextures.Count; t++)
            {
                bakeTextures[t].name = string.Format("{0}_{1}", animationName, t);
                // foreach (var existing in existingTextures)
                //     DestroyImmediate(existing, true);
                UnityEditor.AssetDatabase.AddObjectToAsset(bakeTextures[t], unit);
            }

            textures = bakeTextures.ToArray();
            textureCount = textures.Length;
        }




    }




    #endregion



    #region  meshBake
    [System.Serializable]
    public class MeshBakePreferences
    {
        public int type { get; set; }
        public int texSize { get; set; }
        public int texQuality { get; set; }
        public int fps { get; set; }
        public int previousGlobalBake { get; set; }
        public int globalBake { get; set; }
        public int meshNormalMode { get; set; }
        public string[] customClips { get; set; }
        public bool customCompression { get; set; }
        public int rootMotionMode { get; set; }
        public string animController { get; set; }
        public string animAvatar { get; set; }
        public bool combineMeshes { get; set; }
        public string[] exposedTransforms { get; set; }
        public int[] lodDistanceKeys { get; set; }
        public float[] lodDistanceValues { get; set; }
        public string outputPath { get; set; }
        public float compressionAccuracy { get; set; }
        public bool shaderGraphSupport { get; set; }
    }
    #endregion

    #region OnPrefabChange
    private void OnPrefabChanged()
    {
        if (spawnedAsset != null)
        {
            DestroyImmediate(spawnedAsset.gameObject);
        }
        if (Application.isPlaying)
        {
            return;
        }
        animator = null;
        animAvatar = null;
        if (prefab != null)
        {
            if (spawnedAsset == null)
            {
                spawnedAsset = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                SetChildFlags(spawnedAsset.transform, HideFlags.HideAndDontSave);
            }
            bakeAnims.Clear();
            frameSkips.Clear();
            AutoPopulateFiltersAndRenderers();
            AutoPopulateAnimatorAndController();
            // AutoPopulateExposedTransforms();
            LoadPreferencesForAsset();
            clipsCache = GetClips();
        }
        previousPrefab = prefab;

        void SetChildFlags(Transform t, HideFlags flags)
        {
            Queue<Transform> q = new Queue<Transform>();
            q.Enqueue(t);
            for (int i = 0; i < t.childCount; i++)
            {
                Transform c = t.GetChild(i);
                q.Enqueue(c);
                SetChildFlags(c, flags);
            }
            while (q.Count > 0)
            {
                q.Dequeue().gameObject.hideFlags = flags;
            }
        }
    }

    private List<AnimationClip> GetClips()
    {
        if (prefab == null)
            return new List<AnimationClip>();
        var dependenciesArray = EditorUtility.CollectDependencies(new Object[] { prefab });
        var dependencies = new HashSet<Object>(dependenciesArray);
        foreach (var dep in dependenciesArray)
        {
            dependencies.UnionWith(AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(dep)));
        }
        foreach (var customClip in customClips)
        {
            dependencies.Add(customClip);
        }
        dependencies.RemoveWhere(x => !(x is AnimationClip) || x == null);
        foreach (AnimationClip clip in dependencies)
        {
            string n = clip.name;
            if (!bakeAnims.ContainsKey(n))
                bakeAnims.Add(n, true);
        }
        dependencies.RemoveWhere(x =>
        {
            string n = x.name;
            return !bakeAnims.ContainsKey(n) || !bakeAnims[n];
        });

        var distinctClips = dependencies.Cast<AnimationClip>().ToList();
        // requiresAnimator = false;
        // isHumanoid = false;
        var humanoidCheck = new List<AnimationClip>(distinctClips);
        if (animController)
        {
            var controllerClips = animController.animationClips;
            foreach (var clip in controllerClips)
            {
                if (!dependencies.Contains(clip))
                {
                    distinctClips.Add(clip);
                }
                if (clip && (clip.isHumanMotion || !clip.legacy))
                {
                    requiresAnimator = true;
                    if (clip.isHumanMotion)
                    {
                        isHumanoid = true;
                    }
                }
            }
        }
        // try
        // {
        //     if (requiresAnimator == false)
        //     {
        //         var importer = GetImporter(GetPrefabPath());
        //         if (importer && importer.animationType == ModelImporterAnimationType.Human)
        //         {
        //             requiresAnimator = true;
        //             isHumanoid = true;
        //         }
        //     }
        // }
        // catch { }
        // try
        // {
        //     if (requiresAnimator == false && IsOptimizedAnimator())
        //         requiresAnimator = true;
        // }
        // catch { }
        for (int i = 0; i < distinctClips.Count; i++)
        {
            string clipName = distinctClips[i].name;
            if (!bakeAnims.ContainsKey(clipName))
                bakeAnims.Add(clipName, true);
        }
        distinctClips.Sort((x, y) => x.name.CompareTo(y.name));
        // clipsLoaded = true;
        return distinctClips;
    }
    private void LoadPreferencesForAsset()
    {
        try
        {
            string path = GetPrefabPath();
            if (string.IsNullOrEmpty(path))
                return;
            string guid = AssetDatabase.AssetPathToGUID(path);
            string prefsPath = string.Format("MeshAnimator_BakePrefs_{0}", guid);
            prefsPath = Path.Combine(Path.GetTempPath(), prefsPath);
            MeshBakePreferences bakePrefs = null;
            using (FileStream fs = new FileStream(prefsPath, FileMode.Open))
            {
                BinaryFormatter br = new BinaryFormatter();
                bakePrefs = (MeshBakePreferences)br.Deserialize(fs);
            }
            animAvatar = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(bakePrefs.animAvatar), typeof(Avatar)) as Avatar;
            animController = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(bakePrefs.animController), typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
            customClips.AddRange(bakePrefs.customClips.Select(q => (AnimationClip)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(q), typeof(AnimationClip))));
            customClips = customClips.Distinct().ToList();
            customCompression = bakePrefs.customCompression;
            // exposedTransforms.AddRange(bakePrefs.exposedTransforms);
            // exposedTransforms = exposedTransforms.Distinct().ToList();
            fps = bakePrefs.fps;
            // Debug.Log(fps);
            globalBake = bakePrefs.globalBake;
            previousGlobalBake = bakePrefs.previousGlobalBake;
            // rootMotionMode = (RootMotionMode)bakePrefs.rootMotionMode;
            // meshNormalMode = (MeshNormalMode)bakePrefs.meshNormalMode;
            // useOriginalMesh = bakePrefs.combineMeshes;
            // selectedAnimatorType = (MeshAnimatorType)bakePrefs.type;
            selectedTextureSize = (ShaderTextureSize)bakePrefs.texSize;
            selectedTextureQuality = (ShaderTextureQuality)bakePrefs.texQuality;
            outputPath = bakePrefs.outputPath;
            // compressionAccuracy = bakePrefs.compressionAccuracy;
            shaderGraphSupport = true;
            for (int i = 0; i < bakePrefs.lodDistanceKeys.Length; i++)
            {
                lodDistances.Add(new KeyValuePair<int, float>(bakePrefs.lodDistanceKeys[i], bakePrefs.lodDistanceValues[i]));
            }
        }
        catch { }
    }
    private string GetPrefabPath()
    {
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(assetPath))
        {
            Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            assetPath = AssetDatabase.GetAssetPath(parentObject);
        }
        return assetPath;
    }
    private Avatar GetAvatar()
    {
        if (animAvatar)
            return animAvatar;
        var objs = EditorUtility.CollectDependencies(new Object[] { prefab }).ToList();
        foreach (var obj in objs.ToArray())
            objs.AddRange(AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(obj)));
        objs.RemoveAll(q => q is Avatar == false || q == null);
        if (objs.Count > 0)
            animAvatar = objs[0] as Avatar;
        return animAvatar;
    }

    private void AutoPopulateAnimatorAndController()
    {
        GetAvatar();
        animator = spawnedAsset.GetComponent<Animator>();

        if (animator == null)
            animator = spawnedAsset.GetComponentInChildren<Animator>();
        if (animator && animController == null)
            animController = animator.runtimeAnimatorController;
    }
    private void AutoPopulateFiltersAndRenderers()
    {
        meshFilters.Clear();
        skinnedRenderers.Clear();
        MeshFilter[] filtersInPrefab = spawnedAsset.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < filtersInPrefab.Length; i++)
        {
            if (meshFilters.Contains(filtersInPrefab[i]) == false)
                meshFilters.Add(filtersInPrefab[i]);
            if (filtersInPrefab[i].GetComponent<MeshRenderer>())
                filtersInPrefab[i].GetComponent<MeshRenderer>().enabled = false;
        }
        SkinnedMeshRenderer[] renderers = spawnedAsset.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (skinnedRenderers.Contains(renderers[i]) == false)
                skinnedRenderers.Add(renderers[i]);
            renderers[i].enabled = false;
        }
        // useOriginalMesh = meshFilters.Count + skinnedRenderers.Count <= 1;
    }

    #endregion



}


public static class MatrixUtils
{
    public static void FromMatrix4x4(Transform transform, Matrix4x4 matrix)
    {
        transform.localPosition = GetPosition(matrix);
        transform.localRotation = GetRotation(matrix);
        transform.localScale = GetScale(matrix);
    }
    public static Quaternion GetRotation(Matrix4x4 matrix)
    {
        var f = matrix.GetColumn(2);
        if (f == Vector4.zero)
            return Quaternion.identity;
        return Quaternion.LookRotation(f, matrix.GetColumn(1));
    }
    public static Vector3 GetPosition(Matrix4x4 matrix)
    {
        return matrix.GetColumn(3);
    }
    public static Vector3 GetScale(Matrix4x4 m)
    {
        return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
    }
}