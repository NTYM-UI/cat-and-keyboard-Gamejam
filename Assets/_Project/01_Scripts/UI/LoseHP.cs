using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface LoseHP
{
    abstract void LoseTheHP();
    //��Ѫ�߼����Ƿ��и��˼�Ѫ���޵У��˺�����
    abstract void TakeDamage();
    //������Ч��
    abstract void ApplyLoseHPAttribute();
}
