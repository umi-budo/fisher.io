using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]             //NetworkIdentityを追加
public class MirrorBullet : NetworkBehaviour
{
    [Header("弾の移動速度")]
    public float m_Speed = 20f;
    [Header("弾のダメージ値")]
    public int m_Damage = 10;
    [Header("物理"),SerializeField]
    private Rigidbody m_Rigidbody;
    [Header("破棄するまでの時間")]
    public float m_DestroyTime = 1.0f;

    void Start()
    {
        // 弾丸を一定時間後に破壊するコルーチンを開始
        StartCoroutine(DestroyAfterTime(m_DestroyTime));

        //物理獲得
        m_Rigidbody = GetComponent<Rigidbody>();

        //弾の向いている方向へ移動処理
        m_Rigidbody.velocity = transform.forward * m_Speed;
    }

    /// <summary>
    /// 接触した場合
    /// </summary>
    /// <param name="other">当たった対象</param>
    void OnTriggerEnter(Collider other)
    {
        //サーバー側である場合
        if (isServer)
        {
            //パラメーターがある場合代入
            MirrorParameta target = other.GetComponent<MirrorParameta>();
            //パラメーターがある
            if (target != null)
            {
                //ダメージを与える
                target.Damage(m_Damage);
            }
            //弾は消滅
            DestroyBullet();
        }
    }
    /// <summary>
    /// サーバー側で弾を破棄し、自動でクライアント側も破棄する
    /// </summary>
    [Server]
    void DestroyBullet()
    {
        // サーバー側でオブジェクトを破壊
        NetworkServer.Destroy(gameObject);
    }
    /// <summary>
    /// 指定した時間に達したら、サーバー経由で弾を破壊し、クライアント側も破壊する
    /// </summary>
    /// <param name="time">指定時間(秒)</param>
    /// <returns></returns>
    IEnumerator DestroyAfterTime(float time)
    {
        //一定時間まで待機
        yield return new WaitForSeconds(time);
        //サーバーであれば、オブジェクト破壊を実行
        if (isServer)
            DestroyBullet();
    }
}
