using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM : MonoBehaviour
{
    public AudioSource mainAudioSource;  // 主背景音乐来源
    public AudioSource bossAudioSource;  // Boss战斗背景音乐来源
    public AudioSource TypeSource;      // 对话类型音效来源
    public AudioSource chaosSource;     // 混沌控制音效来源 
    public AudioSource fistSource;    // 拳头音效来源
    public AudioSource bombSource;    // 炸弹音效来源
    public AudioSource buttonSource;  // 按钮音效来源

    void Start()
    {
        // 音乐播放：确保场景切换时背景音乐持续播放
        DontDestroyOnLoad(gameObject);
        PlayMainBGM(null);
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.PLAY_MAIN_BGM, PlayMainBGM);
        EventManager.Instance.Subscribe(GameEventNames.BOSS_BATTLE_START, PlayBossBGM);
        EventManager.Instance.Subscribe(GameEventNames.BOSS_BATTLE_START, StopMainBGM);
        EventManager.Instance.Subscribe(GameEventNames.BOSS_DEFEATED, StopBossBGM);
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_TYPE_SOUND, PlayTypeSound);
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL, PlayChaosSound); 
        EventManager.Instance.Subscribe(GameEventNames.PLAY_FIST_SOUND, PlayFistSound);
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BOMB_SOUND, PlayBombSound);
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_MAIN_BGM, PlayMainBGM);
        EventManager.Instance.Unsubscribe(GameEventNames.BOSS_BATTLE_START, PlayBossBGM);
        EventManager.Instance.Unsubscribe(GameEventNames.BOSS_BATTLE_START, StopMainBGM);
        EventManager.Instance.Unsubscribe(GameEventNames.BOSS_DEFEATED, StopBossBGM);
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_TYPE_SOUND, PlayTypeSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL, PlayChaosSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_FIST_SOUND, PlayFistSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BOMB_SOUND, PlayBombSound);
    }

    // 播放主背景音乐
    public void PlayMainBGM(object data)
    {
        // 获取AudioSource组件并播放背景音乐
        AudioSource audioSource = mainAudioSource;
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // 停止主背景音乐
    public void StopMainBGM(object data)
    {
        // 获取AudioSource组件并停止背景音乐
        AudioSource audioSource = mainAudioSource;
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // 播放Boss背景音乐
    public void PlayBossBGM(object data)
    {
        // 获取AudioSource组件并播放背景音乐
        AudioSource audioSource = bossAudioSource;
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // 停止Boss背景音乐
    public void StopBossBGM(object data)
    {
        // 获取AudioSource组件并停止背景音乐
        AudioSource audioSource = bossAudioSource;
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    //打字音效 - 使用PlayOneShot支持重叠播放
    public void PlayTypeSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = TypeSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    //混沌音效 - 使用PlayOneShot支持重叠播放
    public void PlayChaosSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = chaosSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
    
    // 播放拳头音效 - 使用PlayOneShot支持重叠播放
    public void PlayFistSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = fistSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    // 播放炸弹音效 - 使用PlayOneShot支持重叠播放
    public void PlayBombSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = bombSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    // 播放按钮音效 - 使用PlayOneShot支持重叠播放
    public void PlayButtonSound()
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = buttonSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
