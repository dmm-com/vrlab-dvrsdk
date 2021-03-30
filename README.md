# DMM VR Connect SDK
DMM VR Connect SDKはDMM VR Connectと連携して、ゲーム内でのアバター読み出しと配信機能が利用できるUnity Packageです。

# 内容物
各必要アセットに記載のバージョンは動作確認済みバージョンです。  
  
**UnityPackage内** 

- DVRAuth

認証用ライブラリ

- Examples/DVRAuth

認証とAPIの機能を使用するサンプル

- DVRAvatar

アバターを読み込む (使用するには[UniVRM 0.62.0](https://github.com/vrm-c/UniVRM)が必要です)

- Examples/2DUIExample

主にスマホ用のアバターを読み込みサンプルです

- DVRStreaming

Oculus Questから映像と音声をRTMPで配信するライブラリ

- Examples/DVRStreaming

Oculus Questから映像と音声をRTMPで設定した先に配信するサンプル (使用するには[Oculus Integration 20.1](https://developer.oculus.com/downloads/package/unity-integration/)が必要です)  
Oculus/OculusProjectConfigの以下の項目をONにするとURLの入力にキーボードが使用できるようになります  
・Focus Aware  
・Requires System Keyboard  
  

- DVRAvatarCalibrator

アバターをVR機器を用いて制御するためのライブラリ (使用するには[Final IK 2.0](https://assetstore.unity.com/packages/tools/animation/final-ik-14290?locale=ja-JP)が必要です)

- Examples/SteamVRExample
- Examples/OculusVRExample
- Examples/UnityXRExample

VR空間内でログインからアバターのキャリブレーションまで自動で行うサンプルUIです  
(OculusVRExampleを使用するには[Oculus Integration 20.1](https://developer.oculus.com/downloads/package/unity-integration/)が必要です)  
(SteamVRExampleを使用するには[SteamVR Unity Plugin v2.6.1](https://github.com/ValveSoftware/steamvr_unity_plugin/releases)が必要です)  
  
**zip内**

- DMM VR Connect SDK 利用規約.pdf

利用規約です。必ずお読みください

- DMM VR Connect SDK ご利用ガイドライン.pdf

ご利用にあたってのガイドラインです。使用前にご覧ください

- README.md

この説明書です

- third-party-licenses.txt

サードパーティライセンス一覧
  
**unitypackageをインポートする前に必要なライブラリを事前にインポートしておいて下さい。使用しない機能はインポート時にチェックを外して下さい**

# 使用方法
## 1.DMM VR Connectに接続して認証する

まず初めに取得したAPIキーを設定します。  
Unity上で`Assets/Resources`フォルダを開き右クリック->Create->DVRSDK->Create Configrationを選択し、出来たファイル名を`SdkSettings`とします  
InspectorでClient_idに発行されたAPIキーを入力してください。  

```csharp
//Unity用の初期化を行う
var config = new DVRAuthConfiguration(client_id, new UnitySettingStore(), new UniWebRequest(), new NewtonsoftJsonSerializer());

//認証処理の初期化を行う
Authentication.Instance.Init(config);

//DMM VR Connectに接続して認証する
Authentication.Instance.Authorize(
    openBrowser: url =>
    {
        verificationUri = url;
        Application.OpenURL(url);
    },
    onAuthSuccess: isSuccess =>
    {
        if (isSuccess)
        {
            LoginSuccess();
        }
        else
        {
            LoginFailed();
        }
    },
    onAuthError: exception =>
    {
        Debug.LogError(exception);
    });
```
認証処理を行うことでDMM VR Connect上の機能を利用できるようになります。  
利用できる機能のサンプルは`Assets/DVRSDK/Examples/DVRAuth/Scenes`内にサンプルシーンがあるのでご覧ください。

## 2. DMM VR Connectにアップロードされたアバターを読み込む
```csharp
//VRMローダー初期化
vrmLoader = new VRMLoader();
//カレントユーザーを取得
var currentUser = await Authentication.Instance.Okami.GetCurrentUserAsync();
//自身のユーザーIDからユーザー情報を取得してカレントアバターを取得(データ暗号化)
var myUser = await Authentication.Instance.Okami.GetUserAsync(currentUser.id);
var currentAvatar = myUser.current_avatar;
//アバターをダウンロードしてGameObjectを取得
CurrentModel = await Authentication.Instance.Okami.LoadAvatarVRMAsync(currentAvatar, vrmLoader.LoadVRMModelFromConnect) as GameObject;
//ロードしたVRMを表示する
vrmLoader.ShowMeshes();
//自動まばたきを使用する
vrmLoader.AddAutoBlinkComponent();
```
認証後は簡単にDMM VR Connectにアップロードされたアバターをロードしてゲーム内に表示できます。

## 3. アバターをVR内の自分の姿として動くようにする
```csharp
calibrator.LoadModel(CurrentModel);
calibrator.DoCalibration();
```
シーンに置いたVRMCalibratorにHMD、左手、右手を設定してDoCalibrationするだけでアバターを動かすことが出来ます

## テスト環境
Windows 10 x64  
Unity 2018.4.20f1

## アプリ内や紹介ページにロゴを使用する
ガイドラインに従ってDMM VR Connectのロゴを使用できます。  
[ブランドアセット/ロゴガイドライン](https://connect.vrlab.dmm.com/support/brand-assets/)
