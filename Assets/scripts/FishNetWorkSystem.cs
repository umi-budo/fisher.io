using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MirrorChatSystems
{
    /// <summary>
    /// Mirrorのプレイヤーオブジェクト側にセットされる
    /// チャットなどのプレイヤー名などを確保し、共有させるシステム
    /// </summary>
    public class FishNetWorkSystem : NetworkBehaviour
    {

        [SyncVar(hook = nameof(OnNameChanged)), Header("プレイヤー名")]
        public string m_PlayerName;


        // プレイヤー名を設定するメソッド（サーバー側でのみ呼ばれる）
        [Server]
        public void SetPlayerName(string name)
        {
            m_PlayerName = name;
        }

        // プレイヤー名が変更されたときのクライアント側での処理
        private void OnNameChanged(string oldName, string newName)
        {
            Debug.Log($"プレイヤー名が変更されました: {newName}");
            // ここで名前の変更に関連するUIやゲーム内表示を更新
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Debug.Log($"ローカルプレイヤーが開始されました: {m_PlayerName}");
        }
        /*
        /// <summary>
        /// Serverが起動時
        /// </summary>
        public override void OnStartServer()
        {
            m_PlayerName = (string)connectionToClient.authenticationData;
        }

        /// <summary>
        /// ローカルプレイヤーが起動
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            //プレイヤー名を代入
            ChatUISystem.m_LocalPlayerName = m_PlayerName;
        }
        */
    }
}
