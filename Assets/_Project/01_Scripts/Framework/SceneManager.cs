using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 场景管理系统，用于处理游戏中的场景加载和切换
/// 使用示例：
/// SceneManager.Instance.LoadScene("GameScene");
/// SceneManager.Instance.LoadSceneAsync("GameScene", OnSceneLoaded);
/// </summary>
public class SceneManager : Singleton<SceneManager>
{
    // 当前场景名称
    private string _currentSceneName = string.Empty;
    // 加载进度
    private float _loadingProgress = 0f;
    // 是否正在加载场景
    private bool _isLoadingScene = false;

    // 场景加载事件名称常量
    public const string EVENT_SCENE_LOAD_STARTED = "SceneLoadStarted";
    public const string EVENT_SCENE_LOAD_PROGRESS = "SceneLoadProgress";
    public const string EVENT_SCENE_LOAD_COMPLETED = "SceneLoadCompleted";

    /// <summary>
    /// 当前场景名称
    /// </summary>
    public string CurrentSceneName { get { return _currentSceneName; } }

    /// <summary>
    /// 加载进度（0-1）
    /// </summary>
    public float LoadingProgress { get { return _loadingProgress; } }

    /// <summary>
    /// 是否正在加载场景
    /// </summary>
    public bool IsLoadingScene { get { return _isLoadingScene; } }

    protected override void Awake()
    {
        base.Awake();
        // 初始化当前场景名称
        _currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneManager] 场景名称不能为空");
            return;
        }

        if (_isLoadingScene)
        {
            Debug.LogWarning("[SceneManager] 已经有场景加载操作正在进行中");
            return;
        }

        try
        {
            _isLoadingScene = true;
            _loadingProgress = 0f;
            
            // 触发场景加载开始事件
            EventManager.Instance.Publish(EVENT_SCENE_LOAD_STARTED, sceneName);
            
            // 同步加载场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            
            // 更新当前场景名称
            _currentSceneName = sceneName;
            
            // 触发场景加载完成事件
            _loadingProgress = 1f;
            EventManager.Instance.Publish(EVENT_SCENE_LOAD_PROGRESS, _loadingProgress);
            EventManager.Instance.Publish(EVENT_SCENE_LOAD_COMPLETED, sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SceneManager] 加载场景失败: " + sceneName + "\n" + e.ToString());
        }
        finally
        {
            _isLoadingScene = false;
        }
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <param name="onComplete">加载完成回调</param>
    public void LoadSceneAsync(string sceneName, System.Action<string> onComplete = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneManager] 场景名称不能为空");
            return;
        }

        if (_isLoadingScene)
        {
            Debug.LogWarning("[SceneManager] 已经有场景加载操作正在进行中");
            return;
        }

        StartCoroutine(LoadSceneAsyncCoroutine(sceneName, onComplete));
    }

    /// <summary>
    /// 异步加载场景的协程
    /// </summary>
    private IEnumerator LoadSceneAsyncCoroutine(string sceneName, System.Action<string> onComplete)
    {
        _isLoadingScene = true;
        _loadingProgress = 0f;

        // 触发场景加载开始事件
        EventManager.Instance.Publish(EVENT_SCENE_LOAD_STARTED, sceneName);

        // 异步加载场景
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        // 不自动激活场景，以便在加载完成后可以进行一些初始化操作
        asyncOperation.allowSceneActivation = false;

        // 等待加载进度达到0.9（Unity的加载机制，最后0.1需要手动激活）
        while (asyncOperation.progress < 0.9f)
        {
            _loadingProgress = asyncOperation.progress;
            EventManager.Instance.Publish(EVENT_SCENE_LOAD_PROGRESS, _loadingProgress);
            yield return null;
        }

        // 在这里可以进行一些场景加载前的准备工作
        // 例如：预加载资源、初始化游戏数据等
        yield return null;

        // 激活场景
        asyncOperation.allowSceneActivation = true;
        
        // 等待场景完全加载
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // 更新当前场景名称
        _currentSceneName = sceneName;

        // 触发场景加载完成事件
        _loadingProgress = 1f;
        EventManager.Instance.Publish(EVENT_SCENE_LOAD_PROGRESS, _loadingProgress);
        EventManager.Instance.Publish(EVENT_SCENE_LOAD_COMPLETED, sceneName);

        // 调用用户回调
        onComplete?.Invoke(sceneName);

        _isLoadingScene = false;
    }

    /// <summary>
    /// 加载场景叠加（添加场景而不卸载当前场景）
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    public void LoadSceneAdditive(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneManager] 场景名称不能为空");
            return;
        }

        try
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            Debug.Log("[SceneManager] 已叠加加载场景: " + sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SceneManager] 叠加加载场景失败: " + sceneName + "\n" + e.ToString());
        }
    }

    /// <summary>
    /// 异步加载场景叠加
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    /// <param name="onComplete">加载完成回调</param>
    public void LoadSceneAdditiveAsync(string sceneName, System.Action<string> onComplete = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneManager] 场景名称不能为空");
            return;
        }

        StartCoroutine(LoadSceneAdditiveAsyncCoroutine(sceneName, onComplete));
    }

    /// <summary>
    /// 异步加载场景叠加的协程
    /// </summary>
    private IEnumerator LoadSceneAdditiveAsyncCoroutine(string sceneName, System.Action<string> onComplete)
    {
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return asyncOperation;

        Debug.Log("[SceneManager] 已异步叠加加载场景: " + sceneName);
        onComplete?.Invoke(sceneName);
    }

    /// <summary>
    /// 卸载指定场景
    /// </summary>
    /// <param name="sceneName">场景名称</param>
    public void UnloadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneManager] 场景名称不能为空");
            return;
        }

        if (sceneName == _currentSceneName)
        {
            Debug.LogError("[SceneManager] 不能卸载当前激活的场景: " + sceneName);
            return;
        }

        try
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            Debug.Log("[SceneManager] 已卸载场景: " + sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SceneManager] 卸载场景失败: " + sceneName + "\n" + e.ToString());
        }
    }

    /// <summary>
    /// 获取当前所有已加载的场景名称
    /// </summary>
    /// <returns>场景名称列表</returns>
    public string[] GetLoadedScenes()
    {
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
        string[] sceneNames = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            sceneNames[i] = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name;
        }

        return sceneNames;
    }
}