# Light Controller
プレハブを設置するだけで明るさ調整メニューを生成するツール。  
![image](https://github.com/Gomorroth/LightController/assets/70315656/23c36800-d120-4229-a2d1-e76bfb6cba95)

lilToonにのみ対応しています。  
https://github.com/lilxyzw/lilToon

## 導入
[このページ](https://gomorroth.github.io/vpm-repos/)に飛んで`Add to VCC`を押してリポジトリを追加、  
プロジェクト管理画面で`Light Controller`を導入してください。  
![image](https://github.com/Gomorroth/LightController/assets/70315656/eb955951-ce31-4ab1-96c4-331862c0a86e)

## 使い方
導入後、自動で生成される`Assets/LightController/LightController.prefab`をアバター直下に設置してください。  
プレイモードに入った時、またはアバターがビルド（アップロード等）された際に自動でメニューが生成されます。

### プレハブがない場合
アバター直下に空のゲームオブジェクトを生成し、`Light Controller Generator`コンポーネントを付けてください。
