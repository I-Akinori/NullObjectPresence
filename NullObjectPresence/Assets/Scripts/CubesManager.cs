using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Arrangement
{
    public Vector3 position;
    public Vector3 rotation;
}
public class CubesManager : MonoBehaviour
{
    [SerializeField] private Texture2D anaTex;
    [SerializeField] private Texture2D sprTex;
    [SerializeField] private Texture2D sumTex;
    [SerializeField] private Texture2D autTex;
    [SerializeField] private Texture2D winTex;
    [SerializeField] private Texture2D digTex;
    [SerializeField] private Texture2D holTex;
    [SerializeField] private Texture2D voiTex;
    [SerializeField] private List<CubeController> cubes;

    [SerializeField] private Material material;
    private Dictionary<SceneMode, Texture2D> textures = new Dictionary<SceneMode, Texture2D>();
    private bool transitable = false;
    private int sameCategoryCount = 0; // アナログ/デジタルの連続回数
    private SceneMode pastMode = SceneMode.Analog;
    private List<SceneMode> analogs = new List<SceneMode>()
    {
        SceneMode.Analog, SceneMode.Spring, SceneMode.Summer, SceneMode.Autumn, SceneMode.Winter
    }; 
    private List<SceneMode> digitals = new List<SceneMode>()
    {
        SceneMode.Digital, SceneMode.HollowGlobe, SceneMode.Voidoid
    };
    private float fadeTime = 1f;
    private float fTime = 0f;
    private float movingTime = 5f;
    private float mTime = 0f;
    private float idlingTime = 120f;
    private float iTime = 0f;
    private bool isMoving = false;
    private List<CubeController> fadingCubes = new List<CubeController>();
    private List<Arrangement> prevArrangements = new List<Arrangement>();
    private List<Arrangement> targetArrangements = new List<Arrangement>();
    
    public void DenyModeTransition()
    {
        cubes.ForEach(cube =>
        {
            cube.SetInactive();
            fadingCubes.Add(cube);
        });
        fTime = fadeTime;
        transitable = false;
    }
    public void PermitModeTransition(SceneMode mode)
    {
        var stateList = FindStates(mode);
        var cubeIndexes = Enumerable.Range(0, cubes.Count).ToList();
        for (var i = 0; i < stateList.Count; i++)
        {
            if (cubeIndexes.Count == 0)
            {
                break;
            }
            var rand = Random.Range(0, cubeIndexes.Count);
            cubes[cubeIndexes[rand]].SetMode(stateList[i]);
            cubes[cubeIndexes[rand]].SetTexture(textures[stateList[i]]);
            fadingCubes.Add(cubes[cubeIndexes[rand]]);
            cubeIndexes.RemoveAt(rand);
        }

        fTime = fadeTime;
        transitable = true;
    }
    public void SetGravity(bool useGravity)
    {
        foreach (var cube in cubes)
        {
            var rb = cube.GetComponent<Rigidbody>().useGravity = useGravity;
        }
    }

    public void StartMove(List<Arrangement> targets)
    {
        this.isMoving = true;
        this.targetArrangements = targets;
        this.StartMove();
    }

    private List<SceneMode> FindStates(SceneMode mode)
    {
        var candidate = new List<SceneMode>();
        // analog / digitalが選択されたらカウント0に
        if (mode == SceneMode.Analog || mode == SceneMode.Digital)
        {
            sameCategoryCount = 0;
        }
        if (IsAnalog(mode))
        {
            if (IsAnalog(pastMode))
            {
                sameCategoryCount++;
            }
            switch (mode)
            {
                case SceneMode.Analog:
                    candidate = analogs.FindAll(s => s != SceneMode.Analog);
                    var index = Random.Range(0, candidate.Count);
                    var chosen = candidate[index];
                    candidate.Clear();
                    candidate.Add(chosen);
                    break;
                case SceneMode.Spring:
                    candidate.Add(SceneMode.Summer);
                    break;
                case SceneMode.Summer:
                    candidate.Add(SceneMode.Autumn);
                    break;
                case SceneMode.Autumn:
                    candidate.Add(SceneMode.Winter);
                    break;
                case SceneMode.Winter:
                    candidate.Add(SceneMode.Spring);
                    break;
                default:
                    break;
            }
            // 3回連続でアナログならデジタルが候補に入る
            if (sameCategoryCount > 2)
            {
                candidate.Add(SceneMode.Digital);
            }
        }
        if (IsDigital(mode))
        {
            if (IsDigital(pastMode))
            {
                sameCategoryCount++;
            }
            switch (mode)
            {
                case SceneMode.Digital:
                    candidate = digitals.FindAll(s => s != SceneMode.Digital);
                    var index = Random.Range(0, candidate.Count);
                    var chosen = candidate[index];
                    candidate.Clear();
                    candidate.Add(chosen);
                    break;
                case SceneMode.HollowGlobe:
                    candidate.Add(SceneMode.Voidoid);
                    break;
                case SceneMode.Voidoid:
                    candidate.Add(SceneMode.HollowGlobe);
                    break;
                default:
                    break;
            }
            // 2回連続でデジタルならアナログが候補に入る
            if (sameCategoryCount > 1)
            {
                candidate.Add(SceneMode.Analog);
            }
        }

        pastMode = mode;
        return candidate;
    }
    private bool IsAnalog(SceneMode mode)
    {
        return analogs.Contains(mode);
    }
    private bool IsDigital(SceneMode mode)
    {
        return digitals.Contains(mode);
    }
    void Start()
    {
        cubes.ForEach(cube => cube.SetMaterial(material));
        textures[SceneMode.Analog] = anaTex;
        textures[SceneMode.Spring] = sprTex;
        textures[SceneMode.Summer] = sumTex;
        textures[SceneMode.Autumn] = autTex;
        textures[SceneMode.Winter] = winTex;
        textures[SceneMode.Digital] = digTex;
        textures[SceneMode.HollowGlobe] = holTex;
        textures[SceneMode.Voidoid] = voiTex;

        var index = Random.Range(0, cubes.Count);
        var target = SceneMode.Digital;
        cubes[index].SetMode(target);
        cubes[index].SetTexture(textures[target]);
        cubes[index].SetAlpha(1);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (fadingCubes.Count >= 0)
        {
            this.Fading();
        }

        if (isMoving)
        {
            Moving();
        }
        Idling();
    }
    
    private void StartMove()
    {
        mTime = 0f;
        prevArrangements.Clear();
        foreach (var cube in cubes)
        {
            prevArrangements.Add(new Arrangement()
            {
                position = cube.transform.position,
                rotation = cube.transform.rotation.eulerAngles,
            });
            cube.GetComponent<Rigidbody>().isKinematic = false;
            cube.GetComponent<Collider>().enabled = true;
        }
    }

    private void Idling()
    {
        if (isMoving)
        {
            iTime = 0;
            return;
        }
        iTime += Time.deltaTime;
        if (iTime > idlingTime)
        {
            iTime = 0;
            this.StartMove(targetArrangements);
        }
    }

    private void Moving()
    {
        mTime += Time.deltaTime;
        
        var v = Mathf.Clamp01(mTime / movingTime * 1.2f); // 発散対策で早めに到着させる
        for (var i = 0; i < cubes.Count; i++)
        {
            if (i >= targetArrangements.Count || i >= prevArrangements.Count)
            {
                break;
            }

            var pos = Vector3.Lerp(prevArrangements[i].position, targetArrangements[i].position, v);
            var rot = Quaternion.Lerp(
                Quaternion.Euler(prevArrangements[i].rotation), Quaternion.Euler(targetArrangements[i].rotation), v
                );
            cubes[i].transform.position = pos;
            cubes[i].transform.rotation = rot;
        }
        if (mTime > movingTime)
        {
            for (var i = 0; i < cubes.Count; i++)
            {
                if (i >= targetArrangements.Count || i >= prevArrangements.Count)
                {
                    break;
                }
                cubes[i].transform.position = targetArrangements[i].position;
                cubes[i].transform.rotation = Quaternion.Euler(targetArrangements[i].rotation);
            }
            FinishMove();
        }
    }

    private void FinishMove()
    {
        mTime = 0;
        isMoving = false;
        foreach (var cube in cubes)
        {
            var rb = cube.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = cube.transform.position;
            rb.isKinematic = false;
            cube.GetComponent<Collider>().enabled = true;
        }
    }

    private void Fading()
    {
        var dt = Time.deltaTime;
        if (fTime > 0)
        {
            fTime -= dt;
            // 出現中
            if (transitable)
            {
                fadingCubes.ForEach(cube => cube.SetAlpha(1 - fTime / fadeTime));
            }
            else
            {
                fadingCubes.ForEach(cube => cube.SetAlpha(fTime / fadeTime));
            }
        }
        else
        {
            if (transitable)
            {
                fadingCubes.ForEach(cube => cube.SetAlpha(1));
            }
            else
            {
                fadingCubes.ForEach(cube => cube.SetTexture(null));
                fadingCubes.ForEach(cube => cube.SetAlpha(0));
            }
        }
    }
}
