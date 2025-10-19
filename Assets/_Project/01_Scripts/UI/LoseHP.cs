using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface LoseHP
{
    abstract void LoseTheHP();
    //减血逻辑，是否有高伤减血，无敌，伤害减免
    abstract void TakeDamage();
    //设置音效等
    abstract void ApplyLoseHPAttribute();
}
