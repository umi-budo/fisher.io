using UnityEngine;
using UnityEngine.UI; // UI操作用
using Mirror; // Mirrorの機能を利用

public class ClientChatSystemTester : NetworkBehaviour
{
    [Header("チャットメッセージTextと連動")]
    public Text m_PlayerMessageText;

    [Header("プレイヤー名[自動同期]"),SyncVar]
    public string m_PlayerName;

    [Header("グループ名名[自動同期]"), SyncVar]
    public string m_GroupName;

    /// <summary>
    /// 自身がプレイヤーである場合、起動してMessageテキストと連動する
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        //使用するTextを探し出す
        GameObject D = GameObject.Find("チャットウィンドゥ");
        //Textとリンクする
        m_PlayerMessageText = D.GetComponent<Text>();
        Debug.Log($"チャットテキストと連動しました: {m_PlayerName}");
    }

    /// <summary>
    /// 受信メッセージを表示
    /// Serverから送信された個人チャットMessageを受け取る
    /// </summary>
    /// <param name="message">受信内容</param>
    [ClientRpc]
    public void RpcDisplayMessage(string message)
    {
        //指定されたメッセージをUIに表示
        if (m_PlayerMessageText != null)
            m_PlayerMessageText.text += message + "\n";
    }

    /// <summary>
    /// クライアントから他のクライアントへメッセージを送信
    /// クライアント→サーバー→クライアント経由へメッセージが流れる
    /// </summary>
    /// <param name="message">送信内容</param>
    /// <param name="targetName">送信したい相手名[ない場合はnull]</param>
    /// <param name="targetGroup">送信したいグループ名[ない場合はnull]</param>
    /// <param name="targetIndex">送信したい相手番号[ない場合は-1]</param>
    [Command] // クライアントからサーバーへのリクエスト
    public void CmdSendMessage(
        string message,
        string targetName,
        string targetGroup,
        int targetIndex)
    {
        // サーバーでメッセージを処理（ServerChatSystemTesterに処理を委託）
        ServerChatSystemTester.Instance.HandleMessage(
            this, message,
            targetName,
            targetGroup,
            targetIndex);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 全プレイヤーに「待たせたな、みんなっ!!」を送信
            CmdSendMessage(m_PlayerName + ": 待たせたな、みんなっ!!",null,null,-1);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            // プレイヤー名「かつや」に「後は任せて、先に行け! かつやぁっ!!」を送信
            CmdSendMessage(m_PlayerName + ": 後は任せて、先に行け! かつやぁっ!!", "かつや", null, -1);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            // グループ「WolfTeam」に「だからよ…止まるんじゃねぇぞ…。」を送信
            CmdSendMessage(m_PlayerName + ": だからよ…止まるんじゃねぇぞ…。", null, "WolfTeam", -1);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // 1番目のプレイヤーに「お前が…ナンバーワンだ!」を送信
            CmdSendMessage(m_PlayerName + ": お前が…ナンバーワンだ!", null, null, 0);
        }
    }
}