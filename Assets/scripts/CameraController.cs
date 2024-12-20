using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // カメラが追従するターゲット（例: プレイヤー）
    public float distance = 10f; // 初期距離
    public float zoomSpeed = 2f; // ズーム速度
    public float minDistance = 5f; // 最小距離
    public float maxDistance = 20f; // 最大距離
    public Vector3 offset = Vector3.zero; // ターゲットからのオフセット

    void LateUpdate()
    {
        if (target == null) return;

        // マウスホイール入力を取得
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // 距離を調整
        distance -= scrollInput * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // カメラの位置を更新
        Vector3 direction = (transform.position - target.position).normalized; // ターゲットからカメラへの方向
        transform.position = target.position + direction * distance + offset;

        // ターゲットを向く
        transform.LookAt(target);
    }
}
