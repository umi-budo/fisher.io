using UnityEngine;
using Mirror;

public class FoodSpawner : NetworkBehaviour
{
    public GameObject foodPrefab; // 餌のPrefab
    public int maxFoodCount = 50; // シーン内に存在する餌の最大数
    public float spawnInterval = 2f; // 餌を補充する間隔（秒）

    [SerializeField]
    private Transform parentObject; // 親オブジェクトを指定するためのTransform
    [SerializeField]
    private Transform floor;  // 床のCube
    [SerializeField]
    private Transform ceiling;  // 天井のCube
    [SerializeField]
    private Transform leftWall;  // 左側の壁
    [SerializeField]
    private Transform rightWall; // 右側の壁
    [SerializeField]
    private Transform frontWall; // 前側の壁
    [SerializeField]
    private Transform backWall;  // 後側の壁

    private int currentFoodCount = 0; // 現在の餌の数

    public override void OnStartServer()
    {
        Debug.Log("Server started, invoking repeating food spawn...");
        // 初期生成
        for (int i = 0; i < maxFoodCount; i++)
        {
            SpawnFood();
        }

        // 一定間隔で餌を補充する処理を開始
        InvokeRepeating(nameof(CheckAndSpawnFood), spawnInterval, spawnInterval);
    }

    [Server]
    private void CheckAndSpawnFood()
    {
        // 必要な餌の数を計算
        int foodToSpawn = maxFoodCount - currentFoodCount;

        for (int i = 0; i < foodToSpawn; i++)
        {
            SpawnFood();
        }
    }

    [Server]
    private void SpawnFood()
    {
        // 6つのCubeの範囲内でランダムな位置を生成
        Vector3 spawnPosition = GetRandomPositionWithinCubes();

        //Debug.Log($"Food spawned at {spawnPosition}");

        GameObject food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
        // 親オブジェクトが指定されていれば、生成した餌をその子として設定
        if (parentObject != null && parentObject.TryGetComponent<NetworkIdentity>(out NetworkIdentity parentIdentity))
        {
            /// 親のネットワークIDを設定
            food.GetComponent<Food>().parentNetId = parentIdentity.netId;
            //Debug.Log($"Assigned parentNetId: {parentIdentity.netId}");
        }
        else
        {
            //Debug.LogWarning("Parent object is not set!");
        }
        
        NetworkServer.Spawn(food); // クライアントにオブジェクトを同期
        currentFoodCount++;

        // 餌が破壊されたときにカウントを減らすコールバックを設定
        food.GetComponent<Food>().OnFoodDestroyed += () => currentFoodCount--;
    }
    private Vector3 GetRandomPositionWithinCubes()
    {
        // 各壁のワールドスケールを取得（lossyScale）
        Vector3 floorScale = floor.lossyScale;
        Vector3 ceilingScale = ceiling.lossyScale;
        Vector3 leftWallScale = leftWall.lossyScale;
        Vector3 rightWallScale = rightWall.lossyScale;
        Vector3 frontWallScale = frontWall.lossyScale;
        Vector3 backWallScale = backWall.lossyScale;

        // 床・天井のY軸範囲
        float minY = floor.position.y + (floorScale.y / 2);
        float maxY = ceiling.position.y - (ceilingScale.y / 2);

        // 左右のX軸範囲
        float minX = leftWall.position.x + (leftWallScale.x / 2);
        float maxX = rightWall.position.x - (rightWallScale.x / 2);

        // 前後のZ軸範囲
        float minZ = backWall.position.z + (backWallScale.z / 2);
        float maxZ = frontWall.position.z - (frontWallScale.z / 2);

        // Cube内部に出現しないように、少し内側にマージンを設ける
        float margin = 0.1f;  // 必要に応じて調整
        minX += margin; maxX -= margin;
        minY += margin; maxY -= margin;
        minZ += margin; maxZ -= margin;

        // ランダムな位置を計算
        return new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            Random.Range(minZ, maxZ)
        );
    }
}
