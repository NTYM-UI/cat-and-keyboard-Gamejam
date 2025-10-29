using UnityEngine;

/// <summary>
/// Boss战传送器 - 当角色掉落到底部时自动传送到上方继续战斗
/// </summary>
public class BossBattleTeleporter : MonoBehaviour
{
    [Header("传送设置")]
    [SerializeField] private Transform teleportDestination; // 传送目标位置
    [SerializeField] private string playerTag = "Player"; // 玩家标签
    [SerializeField] private float teleportDelay = 0.1f; // 传送延迟时间（秒）
    [SerializeField] private bool enableTeleport = true; // 是否启用传送功能

    /// <summary>
    /// 当碰撞体进入触发器时调用
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否启用传送功能、是否是玩家、是否设置了传送目标
        if (enableTeleport && other.CompareTag(playerTag) && teleportDestination != null)
        {
            StartCoroutine(PerformTeleport(other.gameObject));
        }
    }

    /// <summary>
    /// 执行传送的协程
    /// </summary>
    private System.Collections.IEnumerator PerformTeleport(GameObject player)
    {
        // 短暂延迟确保碰撞检测完成
        yield return new WaitForSeconds(teleportDelay);
        
        // 获取玩家的Rigidbody2D组件
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            // 保存当前速度，可选：保留速度或重置
            Vector2 currentVelocity = playerRb.velocity;
            
            // 禁用物理效果，防止传送过程中的异常行为
            playerRb.simulated = false;
            
            // 执行传送
            player.transform.position = teleportDestination.position;
            
            // 重新启用物理效果
            playerRb.simulated = true;
            
            // 设置速度为只有向下的分量，确保角色直线掉落
            playerRb.velocity = new Vector2(0f, -2f); // 负y值使角色向下掉落
            
            Debug.Log($"玩家已传送至上方位置: {teleportDestination.position}，并开始直线掉落");
        }
    }

    /// <summary>
    /// 设置是否启用传送功能
    /// </summary>
    public void SetTeleportEnabled(bool enabled)
    {
        enableTeleport = enabled;
    }
}