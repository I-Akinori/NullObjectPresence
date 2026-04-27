using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CubeController : MonoBehaviour
{
    [SerializeField] private SceneMode targetMode;
    [SerializeField] private bool isActive;
    [SerializeField] private UnityEvent<SceneMode> action;
    [SerializeField] private UnityEvent<SceneMode, Vector3> effect;
    void Start()
    {
        
    }
    void Update()
    {
        
    }

    public void SetMaterial(Material mat)
    {
        this.GetComponent<Renderer>().material = new Material(mat);
    }
    public void SetAlpha(float alpha)
    {
        this.GetComponent<Renderer>().material.SetFloat("_Alpha", alpha);
    }
    public void SetTexture(Texture2D tex)
    {
        this.GetComponent<Renderer>().material.mainTexture = tex;
    }
    public void SetMode(SceneMode mode)
    {
        isActive = true;
        targetMode = mode;
    }

    public void SetInactive()
    {
        isActive = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log(targetMode);
            action.Invoke(targetMode);
            effect.Invoke(targetMode, this.transform.position);
            isActive = false;
        }
    }
}
