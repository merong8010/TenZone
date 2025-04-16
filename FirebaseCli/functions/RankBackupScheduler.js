import functions from "firebase-functions";
import admin from "firebase-admin";
import {Storage} from "@google-cloud/storage";
import zlib from "zlib";

if (!admin.apps.length) {
  admin.initializeApp();
}
const storage = new Storage();

const DIFFICULTIES = ["Easy", "Normal", "Hard", "Expert"];
const BUCKET_NAME = "gs://maketen-2631f.firebasestorage.app"; // 실제 Firebase Storage 버킷 이름

export const RankBackupScheduler = functions.pubsub.schedule("0 1 * * *") // 새벽 1시마다
    .timeZone("Europe/London")
    .onRun(async () => {
      const db = admin.database();
      const today = new Date().toISOString().split("T")[0]; // yyyy-MM-dd

      for (const difficulty of DIFFICULTIES) {
        const baseRef = db.ref(`Leaderboard/${difficulty}`);
        const snapshot = await baseRef.once("value");

        if (!snapshot.exists()) continue;

        for (const [dateKey, data] of Object.entries(snapshot.val())) {
          if (dateKey === "ALL" || dateKey >= today) continue;

          const jsonData = JSON.stringify(data, null, 2);
          const compressed = zlib.gzipSync(jsonData); // gzip 압축

          const filePath = `leaderboard_backup/${difficulty}/${dateKey}.json.gz`;

          const file = storage.bucket(BUCKET_NAME).file(filePath);
          await file.save(compressed, {
            metadata: {
              contentType: "application/gzip",
            },
          });

          console.log(`[✅ 백업 저장됨] gs://${BUCKET_NAME}/${filePath}`);

          // (선택) 원본 삭제
          await baseRef.child(dateKey).remove();
        }
      }

      return null;
    });
