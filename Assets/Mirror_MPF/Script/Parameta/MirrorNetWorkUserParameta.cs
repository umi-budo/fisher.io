using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Mirrorを使用可能とする
using Mirror;

/// <summary>
/// Clientでユーザー情報が記載されているもの
/// キャラクターなどでも良いので、出現物に入れておく。
/// 但し1つのみに限定する事
/// </summary>

//NetworkIdentityを追加
[RequireComponent(typeof(NetworkIdentity))]
public class MirrorNetWorkUserParameta : NetworkBehaviour
{
    [SyncVar, Header("プレイヤー名[AutoServerLink]")]
    public string m_PlayerName;
    [SyncVar, Header("グループ名[AutoServerLink]")]
    public string m_GroupName;
}
