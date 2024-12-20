using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ダメージを与えるオブジェクト自体は同期せず処理します。
/// 同期処理を行った場合、転送データが膨大になる為です。
/// このコンポーネントがコリジョンに接触し且つ、
/// MirrorParametaが接触相手の場合のみ、ダメージを与えます。
/// </summary>
public class MirrorDamageObject : MonoBehaviour
{
    [Header("ダメージ値")]
    public int m_Damage;
    [Header("無敵モード")]
    public bool m_Invincibility;
    [Header("消滅するまでの時間")]
    public float m_DestroyTime = 1.0f;
    private void Start()
    {
        Destroy(gameObject, m_DestroyTime);
    }
}
