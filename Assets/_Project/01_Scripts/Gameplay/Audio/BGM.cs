using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM : MonoBehaviour
{
    public AudioSource mainAudioSource;  // 主背景音乐来源
    public AudioSource bossAudioSource;  // Boss战斗背景音乐来源
    public AudioSource TypeSource;      // 对话类型音效来源
    public AudioSource chaosSource;     // 混沌控制音效来源 
    public AudioSource buttonSource;  // 按钮音效来源
    public AudioSource groundCrackedSource;  // 地面破裂音效来源
    
    [Header("BOSS音效")]
    public AudioSource bossHitSource;  // BOSS被点击音效来源
    public AudioSource bossCapeOpenSource;  // BOSS张开斗篷音效来源
    public AudioSource bossCastSource;  // BOSS施法音效来源
    public AudioSource bossAppearSource;  // BOSS爆发_现身音效来源
    public AudioSource bossTeleportSource;  // BOSS瞬移音效来源
    public AudioSource bossChargeSource;  // BOSS蓄力音效来源
    public AudioSource fistSource;    // 拳头音效来源
    public AudioSource bombSource;    // 炸弹音效来源
    public AudioSource playerDeathSource; // 玩家死亡音效来源

    void Start()
    {
        // 音乐播放：确保场景切换时背景音乐持续播放
        DontDestroyOnLoad(gameObject);
        PlayMainBGM(null);
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe(GameEventNames.PLAY_MAIN_BGM, PlayMainBGM);  // 订阅播放主背景音乐事件
        EventManager.Instance.Subscribe(GameEventNames.BOSS_BATTLE_START, PlayBossBGM);  // 订阅播放Boss背景音乐事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_MAIN_BGM_STOP, StopMainBGM);  // 订阅停止主背景音乐事件
        EventManager.Instance.Subscribe(GameEventNames.BOSS_DEFEATED, StopBossBGM);  // 订阅停止Boss背景音乐事件
        EventManager.Instance.Subscribe(GameEventNames.DIALOG_TYPE_SOUND, PlayTypeSound);  // 订阅播放对话类型音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL, PlayChaosSound);  // 订阅播放混沌控制音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BUTTON_SOUND, PlayButtonSound);  // 订阅播放按钮音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_GROUND_CRACKED_SOUND, PlayGroundCrackedSound);  // 订阅播放地面破裂音效事件
        
        // BOSS音效事件订阅
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BOSS_HIT_SOUND, PlayBossHitSound);  // 订阅播放BOSS受击音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BOSS_CAPE_OPEN_SOUND, PlayBossCapeOpenSound);  // 订阅播放BOSS张开斗篷音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BOSS_CAST_SOUND, PlayBossCastSound);  // 订阅播放BOSS施法音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BOSS_APPEAR_SOUND, PlayBossAppearSound);  // 订阅播放BOSS爆发_现身音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BOSS_TELEPORT_SOUND, PlayBossTeleportSound);  // 订阅播放BOSS瞬移音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BOSS_CHARGE_SOUND, PlayBossChargeSound);  // 订阅播放BOSS蓄力音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_FIST_SOUND, PlayFistSound);  // 订阅播放拳头音效事件
        EventManager.Instance.Subscribe(GameEventNames.PLAY_BOMB_SOUND, PlayBombSound);  // 订阅播放炸弹音效事件
        
        // 玩家音效事件订阅
        EventManager.Instance.Subscribe(GameEventNames.PLAY_PLAYER_DEATH_SOUND, PlayPlayerDeathSound);  // 订阅播放玩家死亡音效事件
    }

    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_MAIN_BGM, PlayMainBGM);
        EventManager.Instance.Unsubscribe(GameEventNames.BOSS_BATTLE_START, PlayBossBGM);
        EventManager.Instance.Unsubscribe(GameEventNames.BOSS_BATTLE_START, StopMainBGM);
        EventManager.Instance.Unsubscribe(GameEventNames.BOSS_DEFEATED, StopBossBGM);
        EventManager.Instance.Unsubscribe(GameEventNames.DIALOG_TYPE_SOUND, PlayTypeSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAYER_TOGGLE_REVERSE_CONTROL, PlayChaosSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BUTTON_SOUND, PlayButtonSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_GROUND_CRACKED_SOUND, PlayGroundCrackedSound);
        
        // BOSS音效事件取消订阅
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BOSS_HIT_SOUND, PlayBossHitSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BOSS_CAPE_OPEN_SOUND, PlayBossCapeOpenSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BOSS_CAST_SOUND, PlayBossCastSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BOSS_APPEAR_SOUND, PlayBossAppearSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BOSS_TELEPORT_SOUND, PlayBossTeleportSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BOSS_CHARGE_SOUND, PlayBossChargeSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_FIST_SOUND, PlayFistSound);
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_BOMB_SOUND, PlayBombSound);
        
        // 玩家音效事件取消订阅
        EventManager.Instance.Unsubscribe(GameEventNames.PLAY_PLAYER_DEATH_SOUND, PlayPlayerDeathSound);
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

    // 播放玩家死亡音效
    public void PlayPlayerDeathSound(object data)
    {        
        // 获取AudioSource组件并播放死亡音效
        AudioSource audioSource = playerDeathSource;
        if (audioSource != null)
        {            
            audioSource.Play();
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
    public void PlayButtonSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = buttonSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    // 播放地面破裂音效 - 使用PlayOneShot支持重叠播放
    public void PlayGroundCrackedSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = groundCrackedSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
    
    // 播放BOSS受击音效 - 使用PlayOneShot支持重叠播放
    public void PlayBossHitSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = bossHitSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
    
    // 播放BOSS张开斗篷音效 - 使用PlayOneShot支持重叠播放
    public void PlayBossCapeOpenSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = bossCapeOpenSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
    
    // 播放BOSS施法音效 - 使用PlayOneShot支持重叠播放
    public void PlayBossCastSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = bossCastSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
    
    // 播放BOSS现身音效 - 使用PlayOneShot支持重叠播放
    public void PlayBossAppearSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = bossAppearSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
    
    // 播放BOSS瞬移音效 - 使用PlayOneShot支持重叠播放
    public void PlayBossTeleportSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = bossTeleportSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
    
    // 播放BOSS蓄力音效 - 使用PlayOneShot支持重叠播放
    public void PlayBossChargeSound(object data)
    {
        // 获取AudioSource组件并播放音效
        AudioSource audioSource = bossChargeSource;
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
