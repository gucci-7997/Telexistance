# 名前
テレイグジスタンスシステム

## image
![IMG_5645](https://github.com/user-attachments/assets/ffacb648-230c-4b93-8927-735847fe2bf7)


## 概要
位置情報を取得し、3DアバターとしてMAP上に可視化するシステム

## 詳細
携帯端末から位置情報取得→firebaseに保存<br>
指定したエリアに該当する位置情報のみ取得→速度向きを計算しアバターで出力


## 環境
* アプリケーション(Unity)
  * macOS 14.6.1
  * Unity 2020.3.48f
  * Maps SDk for Unity 2.1.1

* サーバ(Firebase)
  * Firebase SDK 12.2.1
  * Firebase-admin 12.0.0
  * Firebase-function 6.1.0
  * Firebase-functions-test: 3.3.0
