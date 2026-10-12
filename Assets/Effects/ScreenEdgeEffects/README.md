# 画面エフェクト再生基盤

共通画面にポストエフェクトを表示する機能。
演出をキーで指定し、一回再生または継続表示できる。

演出ごとにMaterial・再生時間・強さ・Curveを設定できる。
駒配置やスキルなど、「いつ再生するか」は呼び出し元で実装する。

## 各データの役割

| 名前 | 役割 |
|---|---|
| Shader | 霧・ノイズなど、画面の加工方法を決める |
| Material | 使用するShaderと、色・範囲などの設定を持つ |
| Preset | Material・再生方法・時間・強さの変化をまとめる |
| Controller | Presetに従って描画と再生時間を管理する |
| Manager | キーからPresetを探し、Controllerへ再生を指示する |

主なファイル：

- `ScreenEffectPreset.cs`：Presetを作成するためのコード。
- `ScreenEdgeEffectController.cs`：再生・停止・Material切り替えの処理。
- `ScreenEffectManager.cs`：キーの登録と再生の受付。
- `ScreenEdgeFog.shader`：画面周辺に静止した霧状の模様を描くサンプル。

## 使う流れ

1. Sceneの描画先とController・Managerを設定する。
2. Materialを指定したPresetを作る。
3. ManagerにキーとPresetを登録する。
4. ゲーム側のコードからキーを指定して再生する。

初回のScene設定が済んでいれば、演出の追加は主に手順2〜4で行う。

## 1. Sceneへの導入

### 描画先の準備

1. 使用するURP Renderer Dataに、Full Screen Pass Renderer Featureを追加する。
2. Feature内のNameを `ScreenEdgeFog` にする。
3. Injection Pointを `After Rendering Post Processing` にする。
4. Requirementsを `None` にする。
5. Fetch Color Bufferをオン、Bind Depth-Stencilをオフにする。
6. Pass Materialに `ScreenEdgeFogMaterial` を設定する。
7. Pass Indexが表示される場合は `0` にする。
8. 対象Cameraが、このRendererを使用するように設定する。

新しいRenderer Dataを使う場合は、使用中のURP AssetのRenderer Listへ登録する。
共有RendererやURP Assetを変更するときは、担当者と調整すること。

上記は付属の霧Shaderを使う場合の設定。
別Shaderを使う場合は、必要な入力や描画タイミングも確認すること。

### ControllerとManagerの準備

1. Sceneに空のGameObjectを作り、名前を `ScreenEdgeEffect` にする。
2. ScreenEdgeEffectControllerとScreenEffectManagerを追加する。
3. 各項目を以下のように設定する。

| コンポーネント／項目 | 設定するもの |
|---|---|
| Controller／Renderer Data | Full Screen Passを追加したRenderer Data |
| Controller／Feature Name | Feature内のNameと同じ文字列。今回は `ScreenEdgeFog` |
| Controller／Initial Preset | コードから再生する場合はNone |
| Manager／Controller | 同じGameObjectにあるController |

Initial PresetにPresetを設定すると、Controllerが有効になった際に自動再生する。
通常の呼び出しで使う場合はNoneにすること。

Scene内のManagerは1つ、同じFeatureを操作するControllerも1つにする。
シーンをまたぐ利用やGameSystemへの登録は、現時点では未対応。

## 2. 演出のPresetを作る

Project欄で右クリックし、
Create → Effects → Screen Effect Presetを選択します。

作成したアセットのInspectorで設定する。

| 項目 | 説明 |
|---|---|
| Effect Material | この演出で使用するMaterial |
| Intensity Property | Shaderが強さを受け取る項目名。付属Shaderでは `_Intensity` |
| Max Intensity | 最大の強さ。0〜1 |
| Playback Mode | 一回再生ならOneShot、継続表示ならContinuous |
| Duration | OneShot全体の再生時間。秒単位 |
| Intensity Curve | OneShotの強さの変化 |
| Fade In Duration | Continuousが最大の強さになるまでの時間。0なら即時 |
| Fade In Curve | Continuousの表示開始時の変化 |
| Fade Out Duration | Stopを呼んでから消えるまでの時間。0なら即時 |
| Fade Out Curve | 停止時の強さの変化 |

色・表示範囲・模様など、見た目固有の項目はMaterialで設定します。

### Curveの読み方

- 横軸：演出の進み具合。0が開始、1が終了。
- 縦軸：強さの倍率。0が見えない状態、1がMax Intensityの強さ。

横軸の1は「1秒」ではない。
例えばDurationが3秒なら、横軸0.5は1.5秒、横軸1は3秒になる。

停止用のFade Out Curveは、縦軸が1から0へ下がる形にすること。

### 再生方法の違い

**OneShot**

Durationの時間内でIntensity Curveに沿って再生し、自動終了される。
終了時に追加のFade Outは行わない。
自然に消すには、Intensity Curveの終点を0にする。

**Continuous**

Fade In後、最大の強さで表示を維持する。
Stopを呼ぶと、その時点の強さからFade Outして終了する。
DurationとIntensity Curveは使用しない。

## 3. Managerへ登録する

SceneのScreen Effect ManagerでEffectsの件数を増やし、KeyとPresetを設定する。

| Keyの入力例 | Presetに設定するアセット |
|---|---|
| `PlaceEffect` | FogOneShot |
| `JammingEffect` | FogContinuous |

Keyはコードから呼び出すための名前。
アセット名とは自動で連動しないので、異なる名前でも構わない。

ただし、コードで指定する文字列とは大文字・小文字まで一致させること。
キーの重複はできない。

登録内容は起動時に読み込むため、再生前に設定すること。

## 4. コードから再生・停止する

### Managerへの参照を設定する

呼び出し元のMonoBehaviourのクラス内に、以下を追加する。

```csharp
[SerializeField]
private ScreenEffectManager screenEffectManager;

// 後で停止する演出の再生番号を保存する。
private long effectPlaybackId;
```

Unityへ戻ると、呼び出し元コンポーネントのInspectorに
Screen Effect Manager欄が表示される。

そこへ、Managerが付いたScene内のGameObjectをドラッグ。

### 一回再生

表示したい処理の中で呼ぶ。

```csharp
screenEffectManager.Play("PlaceEffect");
```

例えば駒配置に連動させる場合は、配置が成功した後に呼ぶ。
今回、駒配置処理への接続は行ってない。

### 継続表示と停止

開始時に再生番号を受け取る。

```csharp
effectPlaybackId =
    screenEffectManager.Play("JammingEffect");
```

終了時に、その番号を渡す。

```csharp
screenEffectManager.Stop(effectPlaybackId);
```

再生番号は「どの再生を停止するか」を識別する番号。
古い演出への停止指示で、新しい演出まで消えないように使用する。

Playは失敗時に0を返す。
Stopには、同じManagerから受け取った番号を渡すこと。

呼び出しは、ManagerとControllerの初期化後に行う。
Scene開始時の呼び出しならStartなどを使用し、別オブジェクトのAwakeからの呼び出しは避けること。

## 別の見た目を追加する方法

1. 対応するShaderを使ったMaterialを用意する。
2. 新しいPresetを作り、Effect Materialへ設定する。
3. 時間やCurveを設定する。
4. Managerへ新しいキーとPresetを登録する。
5. コードから、そのキーを指定して再生する。

既存キーの登録先Presetを変更すれば、
呼び出しコードを変えずに演出を差し替えられる。

Materialは実行中に複製する。
再生処理で、元のMaterialの設定を上書きしない。

## 再生中に呼び直した場合

- 同時表示は1種類。
- OneShotを再度呼ぶと、最初から再生し直す。
- 同じPresetのContinuousを表示中に呼ぶと、表示と再生番号を維持する。
- 停止フェード中に呼ぶと、新しく再生し直す。
- 別の演出を呼ぶと、古い演出を即座に終了して切り替える。
- 異なるMaterial同士を混ぜるクロスフェードは行わない。
- 同じ継続演出を複数の処理が要求する管理には対応してない。

## 対応範囲と注意点

- 共通画面向け。同じRendererを使うCameraに影響する。
- Time.timeScaleが0の間は、演出の経過時間も停止する。
- Controllerを無効にすると演出を終了し、開始前のMaterialとFeatureの有効状態へ戻す。元の状態によっては、エフェクトが表示される。
- 任意のMaterialを登録するだけで使えるわけじゃない。
- Shaderは対象のURP・Full Screen Passで使え、指定した強さの項目を持つ必要がある。
- フェードには、強さ0で元の映像になるShaderが必要。
- 爆発などの空間内エフェクト、カメラ揺れ、UI演出を管理する機能はない。

## 表示されない場合の確認

- Cameraが対象Rendererを使用しているか。
- ControllerのRenderer Dataが設定されているか。
- Feature NameがFull Screen PassのNameと一致しているか。
- ManagerのControllerが設定されているか。
- 呼び出したキーが登録内容と一致しているか。
- PresetのMaterialとIntensity Propertyが正しいか。
- Materialに表示を無効にする設定がないか。

## 確認状況

個人用Sceneで、以下を確認済み。

- OneShotのCurve再生と自動終了。
- Continuousの表示維持と停止フェード。
- OneShotの再呼び出し。
- 古い再生番号への停止指示が、新しい再生を止めないこと。
- キーによる再生・停止。
- 同じ霧Shaderを使う、別Materialへの切り替え。

別Shaderとの互換性、統合先Scene、GameSystemとの連携、
駒配置・スキルとの連動は未確認。