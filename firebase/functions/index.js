const functions = require("firebase-functions/v1");
const admin = require("firebase-admin");

admin.initializeApp(); // Firebase Admin SDK の初期化

// 同じdeviceIDの古いデータを削除する関数
exports.cleanupGeohashes = functions.database
    .ref("/geohashes/{geohash}/{entry}") // 各エントリ（deviceID_timestamp）を監視
    .onWrite(async (change, context) => {
      const geohash = context.params.geohash; // geohashを取得
      const ref = admin.database().ref(`/geohashes/${geohash}`);

      const snapshot = await ref.once("value");
      const data = snapshot.val();

      if (!data) {
        console.log(`geohash ${geohash} にデータがありません。処理を終了します。`);
        return null;
      }

      // データをdeviceIDごとに分類
      const deviceDataMap = {};
      Object.keys(data).forEach((key) => {
        const deviceID = key.split("_")[0]; // deviceIDを取得
        if (!deviceDataMap[deviceID]) {
          deviceDataMap[deviceID] = [];
        }
        deviceDataMap[deviceID]
            .push({key, timestamp: parseInt(key.split("_")[1])});
      });

      const updates = {};

      // 各deviceIDのデータを処理
      Object.keys(deviceDataMap).forEach((deviceID) => {
        const entries = deviceDataMap[deviceID];

        // timestamp順にソート
        entries.sort((a, b) => a.timestamp - b.timestamp);

        // 2件を超える場合、古いデータを削除
        if (entries.length > 2) {
          const keysToDelete = entries
              .slice(0, entries.length - 2).map((entry) => entry.key);
          keysToDelete.forEach((key) => {
            updates[key] = null; // 削除用にnullを設定
          });
          console.log(`geohash: ${geohash},
             deviceID: ${deviceID} のデータ削除: ${keysToDelete}`);
        }
      });

      // データベースを更新して古いデータを削除
      if (Object.keys(updates).length > 0) {
        await ref.update(updates);
      }

      return null;
    });
