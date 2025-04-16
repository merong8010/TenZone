import admin from "firebase-admin";
import functions from "firebase-functions";
if (!admin.apps.length) {
  admin.initializeApp();
}
const DIFFICULTIES = ["Easy", "Normal", "Hard", "Expert"]; // 필요한 난이도만 추가

// exports.RankUpdateScheduler = functions.pubsub.schedule("0 * * * *") // 매시 0분
export const RankUpdateScheduler = functions.pubsub.schedule("*/5 * * * *") // 매시 0분
    .timeZone("Europe/London")
    .onRun(async (context) => {
      const db = admin.database();
      const today = new Date().toISOString().split("T")[0]; // yyyy-MM-dd

      for (const difficulty of DIFFICULTIES) {
        const ref = db.ref(`Leaderboard/${difficulty}/${today}`);
        const snapshot = await ref.once("value");

        if (!snapshot.exists()) continue;

        const users = [];
        snapshot.forEach((child) => {
          const data = child.val();
          if (data.point !== undefined) {
            users.push({
              id: child.key,
              point: data.point,
              remainMilliSeconds: data.remainMilliSeconds,
              timeStamp: data.timeStamp,
            });
          }
        });

        users.sort((a, b) => {
          if (b.point !== a.point) return b.point - a.point;
          if (b.remainMilliSeconds !== a.remainMilliSeconds) return b.remainMilliSeconds - a.remainMilliSeconds;
          return a.timeStamp - b.timeStamp;
        });

        // 순위 저장
        const updates = {};
        users.forEach((user, index) => {
          updates[`${user.id}/rank`] = index + 1;
        });

        await ref.update(updates);
        console.log(`[${difficulty}] ${today} 랭킹 갱신 완료`);

        const refAll = db.ref(`Leaderboard/${difficulty}/ALL`);
        const snapshotAll = await ref.once("value");

        if (!snapshotAll.exists()) continue;

        const usersAll = [];
        snapshotAll.forEach((child) => {
          const data = child.val();
          if (data.point !== undefined) {
            usersAll.push({
              id: child.key,
              point: data.point,
              remainMilliSeconds: data.remainMilliSeconds,
              timeStamp: data.timeStamp,
            });
          }
        });

        usersAll.sort((a, b) => {
          if (b.point !== a.point) return b.point - a.point;
          if (b.remainMilliSeconds !== a.remainMilliSeconds) return b.remainMilliSeconds - a.remainMilliSeconds;
          return a.timeStamp - b.timeStamp;
        });

        // 순위 저장
        const updatesAll = {};
        usersAll.forEach((user, index) => {
          updatesAll[`${user.id}/rank`] = index + 1;
        });

        await refAll.update(updatesAll);
        console.log(`[${difficulty}] ALL 랭킹 갱신 완료`);
      }

      return null;
    });
