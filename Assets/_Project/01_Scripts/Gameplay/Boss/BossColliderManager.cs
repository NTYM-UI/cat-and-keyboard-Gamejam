using UnityEngine;

public class BossColliderManager : MonoBehaviour
{
    // 动画状态枚举
    public enum AnimationState
    {
        None,
        Idle,
        OpenCloak,
        CloseCloak,
        Charge
    }

    public AnimationState _currentState = AnimationState.None;
    
    // 在Unity编辑器中可通过枚举选择调用的方法
    public void SetAnimationState(AnimationState newState)
    {
        // 检测状态是否变化
        if (newState != _currentState)
        {
            // 记录新的状态选择
            _currentState = newState;

            // 确保先移除旧的碰撞器（如果存在）
            RemovePolygonCollider();
            // 延迟一会儿再添加新的碰撞器，确保状态切换完成
            Invoke(nameof(AddPolygonCollider), 0.01f);
            // 然后添加新的碰撞器
            AddPolygonCollider();
        }
    }

    public void AddPolygonCollider()
    {
        if (GetComponent<PolygonCollider2D>() == null)
        {
            PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
            collider.isTrigger = true;
        }
    }

    public void RemovePolygonCollider()
    {
        PolygonCollider2D collider = GetComponent<PolygonCollider2D>();
        if (collider != null)
            Destroy(collider);
    }
}