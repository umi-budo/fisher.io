using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections.Generic; // プレイヤーリスト操作に便利

/// <summary>
/// Server側からのメッセージ送信
/// PlayerInMessageTesterのRpcDisplayMessageと連動する
/// </summary>
public class ServerMessageTester : NetworkBehaviour
{

    public void SendAllPlayersName()
    {
        Debug.Log("全プレイヤー送信");
        string PlayerNames = "";
        foreach (var conn in NetworkServer.connections.Values)
        {
            // プレイヤーを取得
            PlayerInMessageTester player = conn.identity.GetComponent<PlayerInMessageTester>();
            PlayerNames += player.m_PlayerName + "\n";
            // 全プレイヤーにメッセージ送信
            player.RpcDisplayMessage(player.m_PlayerName);
        }
        foreach (var conn in NetworkServer.connections.Values)
        {
            // プレイヤーを取得
            PlayerInMessageTester player = conn.identity.GetComponent<PlayerInMessageTester>();
            // 全プレイヤーにメッセージ送信
            player.RpcDisplayMessage(player.m_PlayerName);
        }
    }


    /// <summary>
    /// 全プレイヤーにメッセージを送信する
    /// </summary>
    /// <param name="message">送信内容</param>
    [Server] // サーバー側のみで実行される
    public void SendMessageToAll(string message)
    {
        Debug.Log("全プレイヤー送信");
        foreach (var conn in NetworkServer.connections.Values)
        {
            // プレイヤーを取得
            PlayerInMessageTester player = conn.identity.GetComponent<PlayerInMessageTester>();
            // 全プレイヤーにメッセージ送信
            player.RpcDisplayMessage(message);
        }
    }

    /// <summary>
    /// 特定のプレイヤーに送信する
    /// </summary>
    /// <param name="message">送信内容</param>
    /// <param name="targetName">送信対象のプレイヤー名</param>
    [Server]
    public void SendMessageToSpecificPlayer(string message, string targetName)
    {
        Debug.Log("特定プレイヤー送信");
        //全ての接続対象者を走査
        foreach (var conn in NetworkServer.connections.Values)
        {
            //相手側のデータ受信コンポーネントを取得
            PlayerInMessageTester player = conn.identity.GetComponent<PlayerInMessageTester>();
            //相手側のプレイヤー名と比較
            if (player.m_PlayerName == targetName)
            {
                // 指定プレイヤーはメッセージ送信
                player.RpcDisplayMessage(message);
            }
        }
    }

    /// <summary>
    /// 特定のグループに送信する
    /// </summary>
    /// <param name="message">送信内容</param>
    /// <param name="targetGroup">送信対象のグループ名</param>
    [Server]
    public void SendMessageToGroup(string message, string targetGroup)
    {
        Debug.Log("特定グループ送信");
        //全ての接続対象者を走査
        foreach (var conn in NetworkServer.connections.Values)
        {
            //相手側のデータ受信コンポーネントを取得
            PlayerInMessageTester player = conn.identity.GetComponent<PlayerInMessageTester>();
            //相手側のグループ名と比較
            if (player.m_GroupName == targetGroup)
            {
                // 指定グループに所属しているプレイヤーはメッセージ送信
                player.RpcDisplayMessage(message);
            }
        }
    }

    /// <summary>
    /// 接続番号1番のプレイヤー送信(テスト)
    /// </summary>
    /// <param name="message">送信内容</param>
    [Server]
    public void SendMessageToFirstPlayer(string message)
    {
        Debug.Log("1番プレイヤー送信");
        //現在の接続者が１人でもいる
        if (NetworkServer.connections.Count > 0)
        {
            //接続された最初のプレイヤーを取得
            NetworkConnectionToClient firstConn = NetworkServer.connections.Values.First();
            //相手側のデータ受信コンポーネントを取得
            PlayerInMessageTester player = firstConn.identity.GetComponent<PlayerInMessageTester>();
            //相手にメッセージ送信
            player.RpcDisplayMessage(message);
        }
    }

    private void Update()
    {
        //自身のサーバーである
        if (isServer)
        {
            //各種Buttonで、Server側から送信
            //1キーを押した
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SendMessageToAll("全員にMessageを送ります。");

            //2キーを押した
            if (Input.GetKeyDown(KeyCode.Alpha2))
                SendMessageToSpecificPlayer("「かつや」のプレイヤーのみ送ります。", "かつや");

            //3キーを押した
            if (Input.GetKeyDown(KeyCode.Alpha3))
                SendMessageToGroup("「WolfTeam」に所属している人だけ送ります。", "WolfTeam");

            //4キーを押した
            if (Input.GetKeyDown(KeyCode.Alpha4))
                SendMessageToFirstPlayer("一番最初にログインした人に送ります。");
        }
    }
}
