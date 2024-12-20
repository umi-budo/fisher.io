using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Mirrorを使用可能とする
using Mirror;

/// <summary>
/// 基本的に、サーバー側がデータを所持し、クライアントからの申告を受ける形で処理を実行する
/// </summary>
public class MirrorParameta : NetworkBehaviour
{
    [Header("キャラクター最大Hp")]
    public int m_MaxHp = 100;

    [SyncVar,Header("[同期]キャラクターHp"),SerializeField]
    private int m_Hp;

    void Start()
    {
        //Hpを更新
        m_Hp = m_MaxHp;
    }

    /// <summary>
    /// サーバー側判定
    /// </summary>
    /// <param name="amount">ダメージ量</param>
    [Server]
    public void Damage(int amount)
    {
        //既にHpが尽きている場合は、ダメージ無視
        if (m_Hp <= 0) return;

        //ダメージ分減算
        m_Hp -= amount;
        //Hpが0以下の場合、Hp=0とし、死亡判定を行う
        if (m_Hp <= 0)
        {
            m_Hp = 0;
            Die();
        }
    }

    /// <summary>
    /// サーバー側は死亡処理を実行
    /// </summary>
    [Server]
    void Die()
    {
        //PRC経由でクライアントに死亡処理を実行
        RpcOnDeath();
    }

    /// <summary>
    /// クライアント側で死亡処理実行
    /// </summary>
    [ClientRpc]
    void RpcOnDeath()
    {
        // クライアント側の死亡処理
        gameObject.SetActive(false);
    }
}
