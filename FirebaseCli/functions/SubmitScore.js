import {getDatabase} from "firebase-admin/database";
import admin from "firebase-admin";
import functions from "firebase-functions";

if (!admin.apps.length) {
  admin.initializeApp();
}

export const SubmitScore = functions.https.onCall(async (data, context) => {
  // 인증 체크
  console.log(`SubmitScore : ${context.auth}`);
  if (!context.auth) {
    throw new functions.https.HttpsError("unauthenticated", "로그인된 사용자만 가능합니다.");
  }

  const userId = data.userId;
  const nickname = data.nickname || "Unknown";
  const gameLevel = data.gameLevel;
  const level = data.level;
  const point = data.point;
  const date = data.date || "ALL";
  const countryCode = data.countryCode || "US";
  const timeStamp = Date.now();
  console.log(`gameLevel : ${gameLevel} | ${point}`);
  if (!gameLevel || point === undefined) {
    throw new functions.https.HttpsError("invalid-argument", "필수 인자 누락");
  }

  const userRef = getDatabase().ref(`Leaderboard/${gameLevel}/${date}/${userId}`);

  try {
    // const existingSnapshot = await userRef.once("value");
    // const existing = existingSnapshot.val();

    // // 기존 점수가 더 높으면 등록하지 않음
    // if (existing && existing.point >= point) {
    //   // 현재 랭킹 계산
    //   const rankSnapshot = await getDatabase().ref(`Leaderboard/${gameLevel}/${date}`).once("value");
    //   const allEntries = [];
    //   rankSnapshot.forEach(child => {
    //     const val = child.val();
    //     allEntries.push({
    //       userId: child.key,
    //       point: val.point || 0,
    //       timeStamp: val.timeStamp || 0
    //     });
    //   });

    //   allEntries.sort((a, b) => {
    //     if (b.point !== a.point) return b.point - a.point;
    //     return a.timeStamp - b.timeStamp;
    //   });

    //   const rank = allEntries.findIndex(entry => entry.userId === userId) + 1;

    //   return {
    //     status: "skipped",
    //     message: "기존 점수보다 낮아 등록하지 않음",
    //     myRank: rank,
    //     point: existing.point
    //   };
    // }

    // 점수 저장
    await userRef.set({
      userId,
      nickname,
      level,
      point,
      countryCode,
      timeStamp,
    });

    // 랭킹 계산
    // const snapshot = await getDatabase().ref(`Leaderboard/${gameLevel}/${date}`).once("value");
    // const rankingList = [];
    // snapshot.forEach((child) => {
    //   const val = child.val();
    //   rankingList.push({
    //     userId: child.key,
    //     point: val.point || 0,
    //     timeStamp: val.timeStamp || 0,
    //   });
    // });

    // rankingList.sort((a, b) => {
    //   if (b.point !== a.point) return b.point - a.point;
    //   return a.timeStamp - b.timeStamp;
    // });

    // const rank = rankingList.findIndex((entry) => entry.userId === userId) + 1;
    // await userRef.update({rank});

    return {
      status: "success",
      message: "점수 등록 완료",
      // myRank: rank,
      point,
    };
  } catch (error) {
    console.error("SubmitScore Error:", error);
    throw new functions.https.HttpsError("internal", "점수 등록 중 오류 발생");
  }
});
