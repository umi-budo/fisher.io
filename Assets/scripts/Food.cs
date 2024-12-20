using UnityEngine;
using Mirror;
using System.Collections;

public class Food : NetworkBehaviour
{
    public int experiencePoints = 10; // この餌がプレイヤーに与える経験値
    // 餌が破壊されたときに呼ばれるイベント
    public event System.Action OnFoodDestroyed;
    [SyncVar]
    public uint parentNetId; // サーバーから渡された親のネットワークID
    public override void OnStartClient()
    {
        base.OnStartClient();

        // クライアント側で親を検索して再設定
        StartCoroutine(WaitForParentAndSet());
    }
    private IEnumerator WaitForParentAndSet()
    {
        NetworkIdentity parentIdentity = null;

        // 親オブジェクトがスポーンされるまで待機
        while ((parentIdentity = GetParentNetworkIdentity()) == null)
        {
            //Debug.Log($"Waiting for parentNetId: {parentNetId}");
            yield return null; // 次のフレームまで待機
        }

        // 親子関係を設定
        transform.SetParent(parentIdentity.transform);
        //Debug.Log($"Parent set to {parentIdentity.name} on client");
    }
    private NetworkIdentity GetParentNetworkIdentity()
    {
        // NetworkClientを使って親のNetworkIdentityを検索
        foreach (var identity in NetworkClient.spawned.Values)
        {
            if (identity.netId == parentNetId)
            {
                return identity;
            }
        }
        return null;
    }
    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーに経験値を付与
            other.GetComponent<PlayerStats>()?.AddExperience(experiencePoints);

            // 餌を破壊
            OnFoodDestroyed?.Invoke();
            NetworkServer.Destroy(gameObject);
        }
    }
}
