using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private bool wasGrounded;
    
    // 空中跳跃和掉落的精灵图片
    public Sprite jumpingSprite;
    public Sprite fallingSprite;
    // 保存原始的地面精灵，用于恢复
    private Sprite originalGroundSprite;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 保存原始精灵
        if (spriteRenderer != null)
        {
            originalGroundSprite = spriteRenderer.sprite;
        }
    }

    void Update()
    {
        // 获取刚体的垂直速度
        float verticalVelocity = rb.velocity.y;
        
        // 保存上一帧的地面状态
        wasGrounded = isGrounded;
        
        // 尝试从PlayerController组件获取isGrounded状态
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            // 使用反射获取私有字段isGrounded的值
            System.Reflection.FieldInfo fieldInfo = typeof(PlayerController).GetField("isGrounded", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo != null)
            {
                isGrounded = (bool)fieldInfo.GetValue(controller);
            }
        }

        // 检测A/D键或左右方向键持续按下
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || 
             Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && isGrounded)
        {
            animator.SetBool("isMoving", true);
            animator.SetBool("isStopping", false);
        }
        else
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isStopping", true);
        }

        // 跳跃和掉落精灵控制
        if (!isGrounded && spriteRenderer != null)
        {
            // 角色在空中，直接更换精灵图片
            if (Mathf.Abs(verticalVelocity) > 0.1f) // 只要在空中有明显速度就切换图片
            {
                // 暂停动画控制器，防止覆盖我们设置的精灵
                if (animator != null)
                {
                    animator.enabled = false;
                }
                
                if (verticalVelocity > 0.1f) // 向上移动 - 跳跃
                {
                    if (jumpingSprite != null)
                    {
                        spriteRenderer.sprite = jumpingSprite;
                    }
                }
                else if (verticalVelocity < -0.1f) // 向下移动 - 掉落
                {
                    if (fallingSprite != null)
                    {
                        spriteRenderer.sprite = fallingSprite;
                    }
                }
            }
        }
        else if (spriteRenderer != null)
        {
            // 恢复动画控制器
            if (animator != null)
            {
                animator.enabled = true;
            }
            
            // 角色在地面上，恢复原始精灵
            if (originalGroundSprite != null)
            {
                spriteRenderer.sprite = originalGroundSprite;
            }
        }
    }
}
