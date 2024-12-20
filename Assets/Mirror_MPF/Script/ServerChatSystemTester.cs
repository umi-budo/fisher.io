using UnityEngine;
using Mirror;
using System.Linq; // プレイヤーリスト操作に便利

public class ServerChatSystemTester : MonoBehaviour
{
    // Singletonインスタンスを設定（他のスクリプトからアクセス可能）
    public static ServerChatSystemTester Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this; // Singletonの初期化
        else
            Destroy(gameObject); // 重複インスタンスを削除
    }

    // クライアントから送信されたメッセージを処理する
    [Server]
    public void HandleMessage(
        ClientChatSystemTester sender,
        string message,
        string targetName,
        string targetGroup,
        int targetIndex)
    {
        if (targetName != null)
        {
            // 特定のプレイヤー（名前指定）
            foreach (var conn in NetworkServer.connections.Values)
            {
                var player = conn.identity.GetComponent<ClientChatSystemTester>();
                if (player.m_PlayerName == targetName)
                {
                    player.RpcDisplayMessage($"クライアント:{message}"); // メッセージ送信
                }
            }
        }
        else if (targetGroup != null)
        {
            // 特定のグループ（グループ名指定）
            foreach (var conn in NetworkServer.connections.Values)
            {
                var player = conn.identity.GetComponent<ClientChatSystemTester>();
                if (player.m_GroupName == targetGroup)
                {
                    player.RpcDisplayMessage($"{message}"); // メッセージ送信
                }
            }
        }
        else if (targetIndex >= 0)
        {
            // 指定されたインデックスのプレイヤー
            var playerList = NetworkServer.connections.Values.Select(conn => conn.identity.GetComponent<ClientChatSystemTester>()).ToList();
            if (targetIndex < playerList.Count)
            {
                var targetPlayer = playerList[targetIndex];
                targetPlayer.RpcDisplayMessage($"{message}"); // メッセージ送信
            }
        }
        else
        {
            // 全プレイヤーに送信
            foreach (var conn in NetworkServer.connections.Values)
            {
                var player = conn.identity.GetComponent<ClientChatSystemTester>();
                player.RpcDisplayMessage($"{message}"); // メッセージ送信
            }
        }
    }
}
