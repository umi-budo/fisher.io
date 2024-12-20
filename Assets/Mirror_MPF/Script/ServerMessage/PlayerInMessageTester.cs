using UnityEngine;
using UnityEngine.UI; // UI操作用
using Mirror; // Mirrorの機能を利用

/// <summary>
/// ServerからClient側への専用メッセージの受信
/// あくまで、Serverからの指示命令の受信用とする
/// 必ずふプレイヤーPrefab(もしくは、Mirrorによる生成?)に導入する事が条件
/// </summary>
public class PlayerInMessageTester : NetworkBehaviour
{
    [Header("プレイヤーのUIのText")]
    public Text m_PlayerMessageText;

    [Header("プレイヤー名[自動同期]"),SyncVar]
    public string m_PlayerName;

    [Header("グループ名[自動同期]"), SyncVar]
    public string m_GroupName;

    /// <summary>
    /// 自身がプレイヤーである場合、起動してMessageテキストと連動する
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        //使用するTextを探し出す
        GameObject D = GameObject.Find("システムメッセージ");
        //Textとリンクする
        m_PlayerMessageText = D.GetComponent<Text>();
        Debug.Log($"システムメッセージテキストと連動しました: {m_PlayerName}");
    }

    /// <summary>
    /// Serverからのメッセージ受信
    /// Serverから指定される
    /// </summary>
    /// <param name="message">受信内容</param>
    [ClientRpc]
    public void RpcDisplayMessage(string message)
    {
        Debug.Log("受信");
        if (m_PlayerMessageText != null)
        {
            // 指定されたメッセージをUIに表示
            m_PlayerMessageText.text += message + "\n";
        }
    }
}
