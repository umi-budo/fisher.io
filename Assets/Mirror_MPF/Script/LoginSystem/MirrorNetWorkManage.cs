using Mirror;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MirrorChatSystems
{

    /// NetworkManagerを拡張して、チャット用のカスタムネットワークマネージャーを実装する
    /// <summary>
    /// Mirrorのデフォルトネットワークマネージャーを継承したシステム
    /// 以後、ネットワークマネージャーはこちらを使用し、対応する
    /// </summary>
    [AddComponentMenu("")]

    public class MirrorNetWorkManage : NetworkManager
    {
        /*
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // サーバーがクライアント接続を受け入れ、プレイヤーを生成する
            string playerName = conn.authenticationData as string;

            // プレイヤーハブの生成
            GameObject player = Instantiate(playerPrefab);
            PlayerNetWorkSystem playerNetworkSystem = player.GetComponent<PlayerNetWorkSystem>();
            playerNetworkSystem.SetPlayerName(playerName); // プレイヤー名を設定

            // プレイヤーをゲームに追加
            NetworkServer.AddPlayerForConnection(conn, player);
        }
        */



        [Header("ログインシステムリンク")]
        public MirrorNewLoginSystem m_MirrorNewLoginSystem;

        // クライアントから送信されるプレイヤー名（クライアントの接続時に設定される）
        public string m_PlayerName;


        #region クライアントログアウト処理一式
        /// <summary>
        /// クライアント側から、サーバーが切断された場合自動で実行
        /// </summary>
        public override void OnStopClient()
        {
            //クライアントを停止させる
            base.OnStopClient();
            Debug.Log("クライアントがサーバーから切断されました。");
            // ここでログアウト処理を行う
            ClientLogout();
        }

        /// <summary>
        /// クライアントログアウト処理
        /// </summary>
        void ClientLogout()
        {
            // ログアウト処理を実装
            // 例: ログイン画面に戻る、セッション情報をクリアする等
            Debug.Log("ログアウト処理を実行します。");

            //ウィンドゥをログイン前状態に戻す。
            if (!m_MirrorNewLoginSystem)
                m_MirrorNewLoginSystem = GetComponent<MirrorNewLoginSystem>();

            //UI【ログインパネル】がアクティブなら
            if (m_MirrorNewLoginSystem.m_LogInWindow.gameObject.activeSelf)
                m_MirrorNewLoginSystem.OnLogInWindows();

            //UI【ログアウトパネル】がアクティブなら
            if (m_MirrorNewLoginSystem.m_LogOutWindow.gameObject.activeSelf)
                m_MirrorNewLoginSystem.OnLogOutWindows();

            //UI【クライアント用、サーバーが起動していない警告パネル】がアクティブなら
            if (!m_MirrorNewLoginSystem.m_NotServerWindow.gameObject.activeSelf)
                m_MirrorNewLoginSystem.OnNotServerWindows();

            //m_MPF_NewLoginSystem.OnLogOutButton();
        }
        #endregion

        // サーバー側でプレイヤーが追加された際に呼ばれるメソッド
        public override void OnServerAddPlayer(NetworkConnectionToClient NCTC)
        {
            // デフォルトのプレイヤーハブを生成
            GameObject player = Instantiate(playerPrefab);

            // PlayerNetWorkSystemコンポーネントを取得
            PlayerNetWorkSystem playerNetSystem = player.GetComponent<PlayerNetWorkSystem>();

            // プレイヤー名を設定
            if (playerNetSystem != null)
            {
                // 接続情報の認証データからプレイヤー名を設定
                if (NCTC.authenticationData != null)
                {
                    playerNetSystem.m_PlayerName = (string)NCTC.authenticationData;
                }
                else if (!string.IsNullOrEmpty(m_PlayerName))
                {
                    // 代替として、NetworkManagerに設定されたplayerNameを使用
                    playerNetSystem.m_PlayerName = m_PlayerName;
                }
            }

            // プレイヤーオブジェクトをゲームに追加
            NetworkServer.AddPlayerForConnection(NCTC, player);
        }
    }
}