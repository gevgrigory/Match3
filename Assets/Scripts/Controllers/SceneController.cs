using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    private readonly object runnablesLock = new object();

    [SerializeField]
    protected Camera gameCamera;

    [SerializeField]
    protected SpriteRenderer background;

    public bool InteractionAllowed { get; protected set; }

    private Queue<Action> runnables = new Queue<Action>();

    protected virtual void Awake()
    {
        Instance = this;
        InteractionAllowed = true;
    }

    protected virtual void Start()
    {
        CalculateBackgroundSize();
    }

    protected void CalculateBackgroundSize()
    {
        Vector3 halfSize = background.bounds.size * 0.5f;
        Vector2 cameraHalfSize = GetHalfCameraSize();

        float coefX = cameraHalfSize.x / halfSize.x;
        float coefY = cameraHalfSize.y / halfSize.y;
        background.transform.localScale = background.transform.localScale * Mathf.Max(coefX, coefY);
    }

    protected Vector2 GetHalfCameraSize()
    {
        float cameraSizeY = gameCamera.orthographicSize;
        return new Vector2(cameraSizeY * gameCamera.aspect, cameraSizeY);
    }

    public void BlockInteraction()
    {
        InteractionAllowed = false;
    }

    public void AllowInteraction()
    {
        InteractionAllowed = true;
    }

    public void RestartTheGame()
    {
        SceneManager.LoadScene(0);
    }

    protected void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    protected void RunInMainThread(Action action)
    {
        lock (runnablesLock)
        {
            runnables.Enqueue(action);
        }
    }

    protected void Update()
    {
        if (runnables.Count > 0)
        {
            Queue<Action> curRunnables;
            lock (runnablesLock)
            {
                curRunnables = new Queue<Action>(runnables);
                runnables.Clear();
            }
            while (curRunnables.Count > 0)
            {
                Action action = curRunnables.Dequeue();
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

#if UNITY_EDITOR
    protected void LateUpdate()
    {
        CalculateBackgroundSize();
    }
#endif
}
