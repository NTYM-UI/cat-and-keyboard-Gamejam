using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class FadeEffect : MonoBehaviour
{
    [Header("淡入淡出设置")]
    public float fadeDuration = 1.5f;
    public CanvasGroup fadeCanvasGroup;
    public GameObject fadeImage; // 引用淡入淡出图片对象

    void Start()
    {
        // 初始禁用图片
        if (fadeImage != null) fadeImage.SetActive(false);

        // 游戏开始时自动淡入
        FadeIn(null);
    }
    
    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.FADE_IN_START, FadeIn);
        EventManager.Instance.Subscribe(GameEventNames.FADE_OUT_START, FadeOut);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.FADE_IN_START, FadeIn);
        EventManager.Instance.Unsubscribe(GameEventNames.FADE_OUT_START, FadeOut);
    }

    // 淡入(从不透明到透明)
    public void FadeIn(object data) => StartCoroutine(FadeCanvasGroup(1, 0));

    // 淡出(从透明到不透明)
    public void FadeOut(object data) => StartCoroutine(FadeCanvasGroup(0, 1));

    // 淡出后淡入
    public void FadeOutThenIn() => StartCoroutine(FadeOutIn());

    // 激活图片的辅助方法
    private void ActivateFadeImage()
    {
        if (fadeImage != null && !fadeImage.activeSelf)
            fadeImage.SetActive(true);
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha)
    {
        // 激活图片
        ActivateFadeImage();
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
        
        // 淡出完成事件
        if (endAlpha == 1)
            EventManager.Instance.Publish(GameEventNames.FADE_OUT_COMPLETE, null);
        
        // 淡入完成后禁用图片
        if (endAlpha == 0 && fadeImage != null)
            fadeImage.SetActive(false);
    }

    private IEnumerator FadeOutIn()
    {
        yield return StartCoroutine(FadeCanvasGroup(0, 1));
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeCanvasGroup(1, 0));
    }
}