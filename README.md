# LightController
プレハブを設置するだけで明るさ調整メニューを生成するツール。  
![image](https://github.com/Gomorroth/LightController/assets/70315656/23c36800-d120-4229-a2d1-e76bfb6cba95)

lilToonにのみ対応しています。  
https://github.com/lilxyzw/lilToon

## 使い方
導入後、自動で生成される`Assets/LightController/LightController.prefab`をアバター直下に設置してください。
プレイモードに入った時、またはアバターがビルド（アップロード等）された際に自動でメニューが生成されます。

### プレハブがない場合
アバター直下に空のゲームオブジェクトを生成し、`Light Controller Generator`コンポーネントを付けてください。
