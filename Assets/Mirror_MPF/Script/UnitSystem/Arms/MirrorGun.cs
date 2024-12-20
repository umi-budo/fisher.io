using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// クライアント側のみ実装
/// 弾を発射するプログラム
/// </summary>
public class MirrorGun : NetworkBehaviour
{
    [Header("弾")]
    public GameObject m_BulletPrefab;
    [Header("銃口")]
    public Transform m_Muzzle;

    void Update()
    {
        //プレイヤーかつ、弾発射実行した
        if (isLocalPlayer && Input.GetButtonDown("Fire1"))
        {
            //サーバー側に発砲処理
            CmdShoot();
        }
    }

    /// <summary>
    /// サーバー側の発砲処理
    /// </summary>
    [Command]
    void CmdShoot()
    {
        //サーバー側の弾出現
        GameObject bullet = Instantiate(m_BulletPrefab, m_Muzzle.position, m_Muzzle.rotation);
        //サーバー経由でクライアントに弾出現実行
        //プレハブは NetworkManager の Registered Spawnable Prefabs リストに登録されている必要があります。
        //これにより、サーバーがオブジェクトをスポーンした際にクライアント側でも認識されます。
        NetworkServer.Spawn(bullet);
    }
}
