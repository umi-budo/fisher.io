using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class MirrorNewLoginSystem : MonoBehaviour
{
    #region 配列一式
    [Header("サーバーのIP")]
    public string m_NetWorkIP = "127.0.0.1";

    [Header("ネットワークマネージャーリンク")]
    public NetworkManager m_NetworkManager;

    [SerializeField, Header("チェックするサーバーポートの入ったトランスポート")]
    private kcp2k.KcpTransport m_KCP;

    [Header("ログインパネル")]
    public GameObject m_LogInWindow;

    [Header("ログアウトパネル")]
    public GameObject m_LogOutWindow;

    [Header("クライアント用、サーバーが起動していない警告パネル")]
    public GameObject m_NotServerWindow;

    [Header("プレイヤー名入力フィールド")]
    public InputField m_UserNameField;

    public enum MirrorSystemMode
    {
        未処理,
        クライアント,
        サーバー,
    }
    [Header("[初期状態]サーバーにすると、自動でサーバー機として機能する")]
    public MirrorSystemMode m_MirrorSystemMode = MirrorSystemMode.未処理;

    [SerializeField, Header("テスト用デバックモード")]
    private bool m_DebugMode = false;


    #endregion

    #region 自動起動部分
    private void Awake()
    {

#if UNITY_EDITOR
        if (m_DebugMode)
            m_MirrorSystemMode = MirrorSystemMode.未処理;
#else
        if (m_DebugMode)
            m_MirrorSystemMode = MirrorSystemMode.サーバー;
#endif

        //ネットワークマネージャーがない場合、ネットワークマネージャーを代入
        if (!m_NetworkManager)
            m_NetworkManager = this.GetComponent<NetworkManager>();

        //KCPがない場合、KCPを代入する
        if (!m_KCP)
            m_KCP = GetComponent<kcp2k.KcpTransport>();

        // サーバーのアドレス設定
        //NetworkManager.singleton.networkAddress = m_NetWorkIP;

        //初期からサーバー起動指定している場合、全てのウィンドゥ(チャット以外)はOff
        if (m_MirrorSystemMode == MirrorSystemMode.サーバー)
        {
            //全ては非表示
            m_LogInWindow.gameObject.SetActive(false);
            m_LogOutWindow.gameObject.SetActive(false);
            m_NotServerWindow.gameObject.SetActive(false);
            //サーバーで起動
            OnServerButton();
        }
        else
        {
            //サーバーではない場合は、ログイン画面を表示状態にする

            //ログインウィンドゥを表示、それ以外は非表示
            if (m_LogInWindow.gameObject.activeSelf == false)
                OnLogInWindows();
            if (m_LogOutWindow.gameObject.activeSelf == true)
                OnLogOutWindows();
            if (m_NotServerWindow.gameObject.activeSelf == true)
                OnNotServerWindows();

            //未処理モードにして選択出来るようにする
            m_MirrorSystemMode = MirrorSystemMode.未処理;
        }
    }
#endregion

    #region ログインパネル表示時
    #region  [サーバーボタン押下時に呼ばれる]DedicatedServerを使用する為、サーバーはAuto。以下の機能はオミットします。
    public void OnServerButton()
    {
        //サーバーが起動
        m_NetworkManager.StartServer();
        //NetworkManager.singleton.StartServer();
        //現在のモードはサーバーである
        m_MirrorSystemMode = MirrorSystemMode.サーバー;
    }
    #endregion

    // クライアントボタン押下時に呼ばれる
    public void OnClientButton()
    {
        //クライアントが起動してない場合のみ実行
        if (!NetworkClient.active)
        {
            //クライアントは、サーバーが起動しているかどうか確認するまでは、全てオフ
            if (m_LogInWindow.gameObject.activeSelf)
                OnLogInWindows();
            if (!m_LogOutWindow.gameObject.activeSelf)
                OnLogOutWindows();
            if (m_NotServerWindow.gameObject.activeSelf)
                OnNotServerWindows();


            //現在のモード
            m_MirrorSystemMode = MirrorSystemMode.クライアント;
            //サーバー側のIPを指定する場合はこれをつける
            //m_NetworkManager.networkAddress = m_NetWorkIP;

            // サーバーのアドレス設定
            m_NetworkManager.networkAddress = m_NetWorkIP;

            //新規クライアントログイン
            //m_NetworkManager.StartClient();
            NetworkManager.singleton.StartClient();
        }
    }

    // サーバーが存在していない警告ボタン押下時に呼ばれる
    public void OnNotServerButton()
    {
        //クライアントは、サーバーが起動しているかどうか確認するまでは、全てオフ
        if (!m_LogInWindow.gameObject.activeSelf)
            OnLogInWindows();
        if (m_LogOutWindow.gameObject.activeSelf)
            OnLogOutWindows();
        if (m_NotServerWindow.gameObject.activeSelf)
            OnNotServerWindows();

        //現在のモード
        m_MirrorSystemMode = MirrorSystemMode.未処理;
    }
    #endregion

    #region ログアウト処理
    public void OnLogOutButton()
    {
        //サーバー、クライアント各種モードでの処理切り替え
        switch (m_MirrorSystemMode)
        {
            //自身がServerである場合
            case MirrorSystemMode.サーバー:
                //サーバーを停止する
                m_NetworkManager.StopServer();
                break;
            //自身がClientである場合
            case MirrorSystemMode.クライアント:
                //クライアントを停止する
                m_NetworkManager.StopClient();
                break;
            //それ以外の場合は基本エラーである
            default:
                Debug.LogWarning("エラー:本来では実行されない処理が実行された!!");
                break;
        }

        //サーバーが存在するので、ログアウト画面を再表示
        if (!m_LogInWindow.gameObject.activeSelf)
            OnLogInWindows();
        if (m_LogOutWindow.gameObject.activeSelf)
            OnLogOutWindows();
        if (m_NotServerWindow.gameObject.activeSelf)
            OnNotServerWindows();
        //ログアウトしたので、未処理扱いに切り替わる
        m_MirrorSystemMode = MirrorSystemMode.未処理;
    }
    #endregion

    #region ログイン・アウトウィンドゥ処理
    /// <summary>
    /// ログインウィンドゥ処理(反転処理)
    /// </summary>
    public void OnLogInWindows()
    {
        //ログインウィンドゥに干渉
        m_LogInWindow.gameObject.SetActive(!m_LogInWindow.gameObject.activeSelf);
    }
    /// <summary>
    /// ログアウトウィンドゥ処理(反転処理)
    /// </summary>
    public void OnLogOutWindows()
    {
        //ログアウトウィンドゥに干渉
        m_LogOutWindow.gameObject.SetActive(!m_LogOutWindow.gameObject.activeSelf);
    }
    /// <summary>
    /// サーバー未起動警告ウィンドゥ処理(反転処理)
    /// </summary>
    public void OnNotServerWindows()
    {
        //警告ウィンドゥに干渉
        m_NotServerWindow.gameObject.SetActive(!m_NotServerWindow.gameObject.activeSelf);
    }
    #endregion
}
