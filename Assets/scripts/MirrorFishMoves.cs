//MirrorFishMoves.cs
using UnityEngine;
//Mirrorを使用可能とする
using Mirror;
using UnityEngine.UI;
/// <summary>
/// 簡単なMirror移動処理方法
/// 基本的にプレイヤー側の入力をサーバー側に渡して処理を行います。
/// 1.クライアント(移動量)をサーバーへ転送
/// 2.サーバーが、移動量を受け取り、キャラクターを移動
/// 3.サーバー側から、NetworkTransformReliableを経由して、クライアントのキャラクターを移動
/// 
/// これには理由があり、クライアント側から実装の移動処理を行う場合、クライアントのデータで
/// 動かすと、チートがし放題である為。
/// サーバー側に移動量を渡した際、サーバーが「移動力おかしくね?」と判断できるため、
/// サーバーからクライアントへ移動制御が入っている。
/// </summary>

///NetworkIdentityを追加(通信に必要)
[RequireComponent(typeof(NetworkIdentity))]

///NetworkTransformReliableを追加(座標・向き情報をサーバーからクライアントへ流す)
///こっちの設定を[ServerToClient]にする事
[RequireComponent(typeof(NetworkTransformReliable))]

///NetworkAnimatorを追加(アニメーション情報をサーバーからクライアントへ流す)
///こっちの設定を[ServerToClient]にする事
//[RequireComponent(typeof(NetworkAnimator))]

public class MirrorFishMoves : NetworkBehaviour
{
    [Header("アニメーターリンク")]
    public Animator m_Animator;
    [Header("キャラクターの移動速度")]
    public float m_MoveSpeed = 10f;
    [Header("キャラクターの最大速度")]
    public float m_MaxSpeed = 5f;
    [Header("移動力減衰値[0.5秒で速度が0になるようにする]")]
    public float m_DecelerationTime = 0.5f;
    [Header("物理"), SerializeField]
    private Rigidbody m_Rigidbody;
    [Header("サーバーへ渡す移動力"), SerializeField]
    private Vector3 m_InputVector;
    [Header("移動フラグ"), SerializeField]
    private bool m_IsMoving = false;
    //[Header("移動アニメーションスピード"), SerializeField]
    //private float m_AnimeMoveSpeed = 0;

    [Header("外部カメラリンク"), SerializeField]
    private GameObject m_CameraLink;

    [Header("キャラクターの旋回力"), SerializeField]
    private float m_RotationSpeed = 10.0f;

    [Header("[Shadow]新しい位置情報"), SerializeField, SyncVar]
    private Vector3 m_NewPosition;

    [Header("[Shadow]新しい向き情報"), SerializeField, SyncVar]
    private Quaternion m_NewRotation;

    [Header("[Shadow]入力情報"), SerializeField, SyncVar]
    private Vector3 m_NewDirection;

    [Header("カメラの旋回力"), SerializeField]
    private float cam_RotationSpeed = 3.0f;

    // 画像を表示するオブジェクト
    [Header("ゲームクリア画像"), SerializeField]
    private GameObject gameClearImage; // GameClear の画像
    [Header("ゲームオーバー画像"), SerializeField]
    private GameObject gameOverImage;  // GameOver の画像

    // リプレイボタン
    [Header("リプレイボタン") ,SerializeField] 
    private Button replayButton;

    // 初期サイズ
    private Vector3 initialScale;

    void Start()
    {
        // 非アクティブ状態も含めてImageを探す
        gameClearImage = FindInactiveObjectByName("GameClearImage");
        gameOverImage = FindInactiveObjectByName("GameOverImage");

        // 見つけたオブジェクトの初期状態を設定
        if (gameClearImage != null) gameClearImage.SetActive(false);
        if (gameOverImage != null) gameOverImage.SetActive(false);
        // シーン上のボタンを探して設定
        if (replayButton == null)
        {
            // シーン内のすべての Button コンポーネントを検索（非アクティブも含む）
            Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();

            // ReplayButton を名前で検索
            foreach (var button in buttons)
            {
                if (button.name == "ReplayButton")
                {
                    replayButton = button;
                    break;
                }
            }

            // 見つかった場合、設定を行う
            if (replayButton != null)
            {
                replayButton.onClick.AddListener(OnReplayButtonClicked);
            }
            else
            {
                Debug.LogWarning("ReplayButton が見つかりませんでした。");
            }

        }
        // 初期サイズを記録
        initialScale = transform.localScale;

        // リプレイボタンが設定されている場合、非アクティブ化
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(false);
            replayButton.onClick.AddListener(OnReplayButtonClicked);
        }

        //アニメーター獲得
        m_Animator = GetComponent<Animator>();
        //物理を取得
        m_Rigidbody = GetComponent<Rigidbody>();
        if (m_Rigidbody == null)
        {
            Debug.LogError("Rigidbodyがアタッチされていません！");
            return;
        }
        //NetworkTransformReliableに自身のtransformをリンクさせる
        GetComponent<NetworkTransformReliable>().target = this.transform;
        //NetworkAnimatorに自身のAnimatorをリンクさせる
        //GetComponent<NetworkAnimator>().animator = m_Animator;

        if (!isLocalPlayer)
        {
            m_Rigidbody.useGravity = false;
        }
    }

    void Update()
    {

        m_Animator.SetFloat("speed", m_Rigidbody.velocity.magnitude);
        //クライアント/プレイヤーである場合、プレイヤー入力を許可
        if (isLocalPlayer)
        {
            PlayerMove();
        }
        CameraControl();

        // スケールが5以上になったらゲームクリア
        if (transform.localScale.x >= 5f)
        {
            GameClear();
        }
    }

    // ゲームクリア処理
    private void GameClear()
    {
        Debug.Log("Game Clear!");

        // 画像を表示
        if (gameClearImage != null)
        {
            gameClearImage.SetActive(true);
        }

        // プレイヤーの経験値を初期化
        PlayerStats stats = GetComponent<PlayerStats>();
        stats.ResetExperience();

        // リプレイボタンを表示
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(true);
        }

        // プレイヤーの操作を無効化
        if (isLocalPlayer)
        {
            enabled = false; // このスクリプトのUpdateを無効化
        }

        // 他に必要なゲームクリア処理を追加
        // 例: UIに「ゲームクリア」のメッセージを表示
        ShowGameClearUI();
    }

    // ゲームクリアUI表示処理（例）
    private void ShowGameClearUI()
    {
        // UIを管理する別のスクリプトやGameObjectにアクセスして処理を記述
        Debug.Log("Display Game Clear UI");
    }

    private void LateUpdate()
    {
        //クライアント/プレイヤーである場合、プレイヤー入力を許可
        if (isLocalPlayer)
        {
            //カメラリンクがない場合、カメラリンクを探して繋げ、ある場合は、プレイヤーに追従させる。
            if (!m_CameraLink)
                m_CameraLink = GameObject.Find("カメラリンク");
            else
            {
                m_CameraLink.transform.position = this.transform.position;
                m_CameraLink.transform.GetChild(0).Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));
            }
        }
        else
        {
            // 他のクライアントのデータを受け取ってキャラクターに適用
            transform.position = Vector3.Lerp(transform.position, m_NewPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, m_NewRotation, Time.deltaTime * 10);
            //移動アニメーション処理
            //MoveAnimator(m_NewDirection);
        }

    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"collision objectname: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        // 自分がサーバーでない場合は何もしない
        if (!isServer) return;

        // 衝突した相手がプレイヤーか確認
        MirrorFishMoves otherPlayer = collision.gameObject.GetComponent<MirrorFishMoves>();
        //if (otherPlayer == null) return; // 相手がプレイヤーでない場合は何もしない
        Debug.LogError("collision object is server");

        if (otherPlayer != null)
        {
            // プレイヤーとぶつかった場合の処理
            Debug.Log("collision another player");
            HandlePlayerCollision(otherPlayer);
        }
        //else if (collision.gameObject.CompareTag("Wall"))
        //{
        //    // 壁とぶつかった場合の処理
        //    Debug.Log("collision Wall");
        //    GameOver(); // 壁に当たったらゲームオーバー処理
        //}
    }

    private void HandlePlayerCollision(MirrorFishMoves otherPlayer)
    {
        // 自分と相手の大きさ（スケール）を比較
        float thisSize = transform.localScale.magnitude;
        float otherSize = otherPlayer.transform.localScale.magnitude;

        if (thisSize > otherSize)
        {
            // 相手の経験値の80%を計算
            PlayerStats otherStats = otherPlayer.GetComponent<PlayerStats>();
            int otherExperience = otherStats != null ? otherStats.experience : 0;
            int gainedExperience = Mathf.FloorToInt(otherExperience * 0.8f);

            // 自分が大きい場合、経験値を獲得し相手を削除
            AddExperience(gainedExperience);
            otherPlayer.GameOver();
        }
    }

    [Server]
    private void AddExperience(int amount)
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.AddExperience(amount); // プレイヤーの経験値を加算
        }
    }

    [Server]
    private void GameOver()
    {
        Debug.Log("GameOver");

        // ゲームオーバー画像を表示
        if (gameOverImage != null)
        {
            gameOverImage.SetActive(true);
        }


        // プレイヤーの経験値を初期化
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.ResetExperience();
            Debug.Log("Player experience reset");
        }
        else
        {
            Debug.LogWarning("PlayerStats component is missing");
        }


        // 非表示にする処理
        RpcHidePlayer();
        Debug.Log("HidePlayer");
        // リプレイボタンを表示
        RpcShowReplayButton();
        Debug.Log("ShowReplayButton");
    }

    [ClientRpc]
    private void RpcShowReplayButton()
    {
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(true);
            Debug.Log("show ReplayButton");
        }
        else
        {
            Debug.LogError("replayButton is null");
        }
    }

    [Client]
    private void OnReplayButtonClicked()
    {
        if (!isLocalPlayer)return;

        // ボタンを非アクティブに戻す
        if (replayButton != null)
        {
            replayButton.gameObject.SetActive(false);
        }
        // 見つけたオブジェクトの初期状態を設定
        if (gameClearImage != null) gameClearImage.SetActive(false);
        if (gameOverImage != null) gameOverImage.SetActive(false);

        // サーバーにリプレイリクエストを送信
        Debug.Log("Call CmdRequestReplay");
        CmdRequestReplay();

    }

    [Command]
    private void CmdRequestReplay()
    {
        Debug.Log("Call RpcRespawnPlayer");
        RpcRespawnPlayer();
    }

    [ClientRpc]
    private void RpcRespawnPlayer()
    {
        Debug.Log("Call RpcRespawnPlayer");
        if (!isServer && !isClient)
        {
            Debug.LogError("サーバーまたはクライアントがアクティブではありません");
            Debug.Log($"isServer: {isServer}, isClient: {isClient}, isLocalPlayer: {isLocalPlayer}");

            return;
        }

        // プレイヤーを初期位置・初期サイズにリセット
        transform.localScale = initialScale;
        transform.position = Vector3.zero; // 初期位置（適宜変更）

        // レンダラーを有効にして表示
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        // コライダーを有効化
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        // 物理演算を有効化（必要に応じて）
        if (m_Rigidbody != null)
        {
            m_Rigidbody.isKinematic = false;
        }
        // プレイヤーのスクリプトを再び有効化
        enabled = true; // Updateの動作を再開
    }

    //[ClientRpc]
    //private void RpcShowPlayer()
    //{
    //    Debug.Log("Call RpcShowPlayer");
    //    // レンダラーを有効にして表示
    //    Renderer[] renderers = GetComponentsInChildren<Renderer>();
    //    foreach (var renderer in renderers)
    //    {
    //        renderer.enabled = true;
    //    }

    //    // コライダーを有効化
    //    Collider[] colliders = GetComponentsInChildren<Collider>();
    //    foreach (var collider in colliders)
    //    {
    //        collider.enabled = true;
    //    }

    //    // 物理演算を有効化（必要に応じて）
    //    if (m_Rigidbody != null)
    //    {
    //        m_Rigidbody.isKinematic = false;
    //    }
    //}

    [ClientRpc]
    private void RpcHidePlayer()
    {
        Debug.Log("HidePlayer");
        // レンダラーを無効にして非表示にする
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // コライダーを無効にして衝突しないようにする
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // 物理演算を無効化（必要に応じて）
        if (m_Rigidbody != null)
        {
            m_Rigidbody.isKinematic = true;
        }
    }

    private void CameraControl()
    {
        if (m_CameraLink)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Y軸（水平回転）はワールドのY軸を基準に回転
            m_CameraLink.transform.rotation = Quaternion.Euler(
                m_CameraLink.transform.eulerAngles.x,
                m_CameraLink.transform.eulerAngles.y + mouseX * m_RotationSpeed,
                0
            );

            // X軸（垂直回転）はワールドのX軸を基準に回転
            m_CameraLink.transform.GetChild(0).rotation = Quaternion.Euler(
                m_CameraLink.transform.GetChild(0).eulerAngles.x - mouseY * m_RotationSpeed,
                m_CameraLink.transform.GetChild(0).eulerAngles.y,
                0
            );

            // 孫オブジェクトのカメラ距離を変更
            Transform cameraTransform = m_CameraLink.transform.GetChild(0).GetChild(0); // 孫オブジェクトのカメラ
            // マウスホイールでカメラ距離を調整
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                Vector3 localPos = cameraTransform.localPosition;
                localPos.z += scrollInput * 5f; // 距離変更の速度
                localPos.z = Mathf.Clamp(localPos.z, -20f, -2f); // 最小・最大距離を設定
                cameraTransform.localPosition = localPos;
            }
        }
    }

    /// <summary>
    /// クライアント側/プレイヤー側である場合、移動入力を許可し、サーバーへ移動量を渡す
    /// </summary>
    public void PlayerMove()
    {
        if (!m_CameraLink)
            return;

        // 入力を取得
        m_InputVector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Height"), Input.GetAxis("Vertical"));
        //入力値が0以外であれば、m_IsMoveingをtrueとする。
        m_IsMoving = m_InputVector != Vector3.zero;

        //プレイヤーの向きを再設定する
        PlayerRotation(
            m_InputVector,
            m_CameraLink.transform.GetChild(0).forward,
            m_CameraLink.transform.GetChild(0).right,
            ref m_NewPosition,
            ref m_NewRotation);

        //サーバーへ、移動位置と向き情報を転送
        ServerMove(this.transform.position, m_NewRotation, m_InputVector);

        //サーバーへ移動量を転送
        /*
        ShadowMove(m_InputVector, m_CameraLink.transform.GetChild(0).forward, m_CameraLink.transform.GetChild(0).right);
        */
        //自身へ転送

    }

    /// <summary>
    /// サーバー側が受け取る処理[移動処理]
    /// </summary>
    /// <param name="direction">移動量</param>
    [Command]
    void ServerMove(Vector3 N_Position, Quaternion N_Rotation, Vector3 N_InputVector)
    {
        if (!isLocalPlayer)
        {
            //座標更新
            m_NewPosition = N_Position;
            //向き更新
            m_NewRotation = N_Rotation;

            m_NewDirection = N_InputVector;

            // 他のクライアントに通知
            //ShadowMove(N_Position, N_Rotation,N_InputVector);
        }

        //移動アニメーション処理
        /*MoveAnimator(direction);
        */
    }

    // クライアントにブロードキャストするRPC (Remote Procedure Call)
    //[ClientRpc]
    //void ShadowMove(Vector3 N_Position, Quaternion N_Rotation, Vector3 N_InputVector)
    //{
    //    // 自分自身のクライアントでは実行しない
    //    if (!isLocalPlayer)
    //    {
    //        m_NewPosition = N_Position;
    //        m_NewRotation = N_Rotation;
    //        m_NewDirection = N_InputVector;
    //    }
    //}

    /// <summary>
    /// キャラクターの向きをカメラ向きと、入力から割り出す
    /// </summary>
    public void PlayerRotation(
        //direction = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Height"), Input.GetAxis("Vertical"));
        Vector3 direction,
        Vector3 CameraForward,
        Vector3 CameraRight,
        ref Vector3 N_Position,
        ref Quaternion N_Rotation)
    {
        // カメラの前後左右方向を基準に移動方向を計算
        Vector3 moveDirection = (CameraForward * direction.z) + (CameraRight * direction.x) + (Vector3.up * direction.y);

        // もし入力がない場合、減速してreturn
        if (moveDirection.magnitude < 0.01f)
        {
            // 入力がない場合は速度を減衰させる
            m_Rigidbody.velocity = Vector3.Lerp(m_Rigidbody.velocity, Vector3.zero, Time.deltaTime / m_DecelerationTime);
            // 現在の位置・回転をサーバーに更新
            N_Position = transform.position;
            N_Rotation = transform.rotation;
            return;
        }

        // プレイヤーの向きを計算（Y軸回転）
        Vector3 flatDirection = new Vector3(moveDirection.x, 0, moveDirection.z); // 水平方向の移動のみ
        // Z軸の回転（上下回転）を計算
        float tiltZ = Input.GetAxis("Height") * 30f; // 回転角度を調整するための倍率（30fは適宜調整可能）
        Quaternion currentRotation = transform.rotation;
        Quaternion tiltRotationZ = Quaternion.Euler(0, 0, tiltZ);

        Quaternion finalRotation;

        if (flatDirection.magnitude > 0.01f)
        {
            // 水平移動がある場合、Y軸回転を更新
            Quaternion targetRotationY = Quaternion.LookRotation(flatDirection) * Quaternion.Euler(0, -90, 0);
            finalRotation = targetRotationY * tiltRotationZ;
        }
        else
        {
            // 水平移動がない場合は、現在のY回転を維持しつつZ軸の回転のみ適用
            float currentY = currentRotation.eulerAngles.y;
            Quaternion yRotationPreserved = Quaternion.Euler(0, currentY, 0);
            finalRotation = yRotationPreserved * tiltRotationZ;
        }

        // 回転を適用
        transform.rotation = Quaternion.Slerp(
        transform.rotation,
        finalRotation,
        Time.deltaTime * m_RotationSpeed
    );

        // Rigidbodyを使ってプレイヤーを移動させる
        m_Rigidbody.AddForce(moveDirection.normalized * m_MoveSpeed, ForceMode.Force);

            // 現在の速度を取得
        Vector3 velocity = m_Rigidbody.velocity;

        // 最大速度を超えないように速度を制限
        if (velocity.magnitude > m_MaxSpeed)
            {
                m_Rigidbody.velocity = velocity.normalized * m_MaxSpeed;
        }
        // サーバーに送るための新しい位置と回転を設定
        N_Position = this.transform.position;
            N_Rotation = this.transform.rotation;

        //移動アニメーション処理
        //MoveAnimator(direction);

    }

    /// <summary>
    /// サーバー側での移動アニメーション処理
    /// </summary>
    /// <param name="direction"></param>
    //public void MoveAnimator(Vector3 direction)
    //{
    //    //移動アニメーション実行
    //    //サーバーが受けたプレイヤーからの移動力が0ではない。
    //    if (direction != Vector3.zero)
    //    {
    //        //移動アニメーションスピード減衰(2倍秒)
    //        m_AnimeMoveSpeed += 2 * Time.deltaTime;
    //        if (m_AnimeMoveSpeed > 1) m_AnimeMoveSpeed = 1;
    //    }
    //    else
    //    {
    //        //移動アニメーションスピード増加(2倍秒)
    //        m_AnimeMoveSpeed -= 2 * Time.deltaTime;
    //        if (m_AnimeMoveSpeed <= 0) m_AnimeMoveSpeed = 0;
    //    }
    //    //サーバー側のアニメーションを変更
    //    m_Animator.SetFloat("Speed", m_AnimeMoveSpeed);
    //}
    // 非アクティブ状態のオブジェクトを探すメソッド
    private GameObject FindInactiveObjectByName(string name)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name && obj.hideFlags == HideFlags.None)
            {
                return obj;
            }
        }
        Debug.LogWarning($"{name} が見つかりませんでした。");
        return null;
    }
}
