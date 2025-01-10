using UnityEngine;
using Mirror;

public class PlayerStats : NetworkBehaviour
{
    [SyncVar] public int experience = 0; // 経験値（同期される）
    public float sizeIncreasePerExperience = 0.01f; // 経験値1につき増えるサイズ

    [Server]
    public void AddExperience(int amount)
    {
        experience += amount;

        // プレイヤーのスケールを変更
        float newScale = 1 + experience * sizeIncreasePerExperience;
        //Debug.Log("sizeIncreasePerExperience:" + sizeIncreasePerExperience);
        transform.localScale = Vector3.one * newScale;

        // クライアントにもサイズ変更を通知
        RpcUpdateScale(transform.localScale);
    }
    [Server]
    public void ResetExperience()
    {
        experience = 0; // 経験値をリセット
    }

    [ClientRpc]
    void RpcUpdateScale(Vector3 newScale)
    {
        transform.localScale = newScale; // クライアント側でスケールを更新
    }
}
