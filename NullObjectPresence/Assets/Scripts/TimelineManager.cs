using System.Collections.Generic;
using NullObjectPresence;
using Unity.VisualScripting;
using UnityEngine;
public enum SceneMode
{
    Analog,
    Spring,
    Summer,
    Autumn,
    Winter,
    Digital,
    HollowGlobe,
    Voidoid,
    Undecided
}

public enum TimelineState
{
    Busy,
    Transitable,
    Transiting,
}
public class TimelineManager : MonoBehaviour
{
    [SerializeField] HandManager leftHand;
    [SerializeField] HandManager rightHand;
    [SerializeField] CubesManager cubesManager;
    [SerializeField] float transitionTime;      // 遷移時間
    [SerializeField] float transitionInterval;  // 遷移クールタイム
    [SerializeField] private List<BaseSceneManager> sceneManagersList = new List<BaseSceneManager>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float time = 0f;
    private SceneMode currentMode = SceneMode.Analog;
    private SceneMode nextMode = SceneMode.Undecided;
    private TimelineState state = TimelineState.Transitable;
    private Dictionary<SceneMode, BaseSceneManager> sceneManagers = new Dictionary<SceneMode, BaseSceneManager>();
    
    void Start()
    {
        foreach (BaseSceneManager manager in sceneManagersList)
        {
            sceneManagers[manager.Mode] = manager;
        }
        
        this.currentMode = SceneMode.Analog;
        this.sceneManagers[this.currentMode].enabled = true;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        time += Time.deltaTime;
        
        switch (state)
        {
            case TimelineState.Busy:
                this.CoolTime();
                break;
            case TimelineState.Transitable:
                // 通知待ち
                break;
            case TimelineState.Transiting:
                this.sceneManagers[this.nextMode].Entry(
                    time / transitionTime, this.sceneManagers[this.currentMode]);
                if (time >= transitionTime)
                {
                    this.sceneManagers[this.nextMode].FinishEntry(this.sceneManagers[this.currentMode]);
                    FinishTransition();
                }
                break;
            default:
                break;
        }
    }

    private void CoolTime()
    {
        if (time >= transitionInterval)
        {
            this.state = TimelineState.Transitable;
            this.cubesManager.PermitModeTransition(this.currentMode);
            time = 0;
        }
    }

    public void SetNextMode(SceneMode mode)
    {
        this.nextMode = mode;
        this.sceneManagers[this.nextMode].enabled = true;
        this.cubesManager.DenyModeTransition();
        this.state = TimelineState.Transiting;
        time = 0;
    }

    public void FinishTransition()
    {
        if (this.nextMode == SceneMode.Undecided)
        {
            return;
        }
        this.sceneManagers[currentMode].enabled = false;
        this.currentMode = this.nextMode;
        this.nextMode = SceneMode.Undecided;
        this.state = TimelineState.Busy;
        time = 0;
    }
}
