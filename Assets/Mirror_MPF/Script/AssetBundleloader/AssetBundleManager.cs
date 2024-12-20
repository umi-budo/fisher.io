using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AssetBundleManager : MonoBehaviour
{
    // AssetBundleのバージョン管理に使用する変数
    private string m_AssetBundleURL = "https://github.com/user/repository/assetbundle";
    private string m_VersionURL = "https://github.com/user/repository/version.txt"; // バージョン管理ファイルのURL
    private int m_LocalVersion = 1;  // ローカルに保持するバージョン

    // AssetBundleとその依存関係の管理用
    private AssetBundle m_AssetBundle;

    // 複数のAssetBundleのバージョン情報を保持する
    private Dictionary<string, int> m_AssetBundleVersions = new Dictionary<string, int>();

    // バージョン情報をダウンロードしてバージョンチェックを行う
    IEnumerator Start()
    {
        // GitHub上のバージョンファイルをダウンロード
        UnityWebRequest versionRequest = UnityWebRequest.Get(m_VersionURL);
        yield return versionRequest.SendWebRequest();

        if (versionRequest.result == UnityWebRequest.Result.Success)
        {
            // サーバーから取得した最新バージョン
            int latestVersion = int.Parse(versionRequest.downloadHandler.text);

            // バージョンチェック
            if (latestVersion > m_LocalVersion)
            {
                // 新しいバージョンがある場合はダウンロードを開始
                Debug.Log("新しいバージョンが見つかりました。AssetBundleをダウンロードします。");
                StartCoroutine(DownloadAssetBundle(latestVersion));
            }
            else
            {
                // ローカルバージョンが最新
                Debug.Log("AssetBundleは最新です。");
            }
        }
        else
        {
            Debug.LogError("バージョン情報の取得に失敗しました: " + versionRequest.error);
        }
    }

    // AssetBundleのダウンロード処理
    IEnumerator DownloadAssetBundle(int latestVersion)
    {
        // AssetBundleのURLにバージョンパラメータを追加してダウンロード
        string bundleURL = m_AssetBundleURL + "?v=" + latestVersion;
        UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);
        yield return assetBundleRequest.SendWebRequest();

        if (assetBundleRequest.result == UnityWebRequest.Result.Success)
        {
            // AssetBundleの読み込み
            m_AssetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);
            m_LocalVersion = latestVersion;  // ローカルのバージョン情報を更新
            Debug.Log("AssetBundleのダウンロードに成功しました。");
        }
        else
        {
            Debug.LogError("AssetBundleのダウンロードに失敗しました: " + assetBundleRequest.error);
        }
    }
}
