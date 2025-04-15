/**
 * Import function triggers from their respective submodules:
 *
 * const {onCall} = require("firebase-functions/v2/https");
 * const {onDocumentWritten} = require("firebase-functions/v2/firestore");
 *
 * See a full list of supported triggers at https://firebase.google.com/docs/functions
 */

// const {onRequest} = require("firebase-functions/v2/https");
// const logger = require("firebase-functions/logger");

// Create and deploy your first functions
// https://firebase.google.com/docs/functions/get-started

// exports.helloWorld = onRequest((request, response) => {
//   logger.info("Hello logs!", {structuredData: true});
//   response.send("Hello from Firebase!");
// });


// import {initializeApp, cert} from "firebase-admin/app";
import {getDatabase} from "firebase-admin/database";
import admin from "firebase-admin";
import functions from "firebase-functions";
const {region, HttpsError} = functions;

if (!admin.apps.length) {
  admin.initializeApp();
}
import {generateNicknameOnUserCreate} from "./generateNickname.js";
export {generateNicknameOnUserCreate};
import {SubmitScore} from "./SubmitScore.js";
export {SubmitScore};

export const GetRanking = region("asia-southeast1").https.onCall(async (data, context) => {
  const gameLevel = data.gameLevel;
  const date = data.date || "ALL";
  const userId = data.userId;
  const limit = data.limit || 10;

  try {
    const snapshot = await getDatabase().ref(`Leaderboard/${gameLevel}/${date}`).once("value");
    const rankingList = [];
    snapshot.forEach((child) => {
      const entry = child.val();
      rankingList.push({
        id: child.key,
        rank: child.rank || 0,
        level: child.level || 0,
        name: entry.name || child.key,
        point: entry.point || 0,
        remainMilliSeconds: entry.remainMilliSeconds || 0,
        countryCode: entry.countryCode || "US",
        timeStamp: entry.timeStamp || 0,
      });
    });

    rankingList.sort((a, b) => {
      if (b.point !== a.point) return b.point - a.point;
      if (b.remainMilliSeconds !== a.remainMilliSeconds) return b.remainMilliSeconds - a.remainMilliSeconds;
      return b.timeStamp - a.timeStamp;
    });

    const topRankings = rankingList.slice(0, limit);
    const myRankIndex = rankingList.findIndex((entry) => entry.userId === userId);
    const myRank = myRankIndex >= 0 ? myRankIndex + 1 : -1;
    const myEntry = myRankIndex >= 0 ? rankingList[myRankIndex] : null;

    return {topRankings, myRank, myEntry};
  } catch (error) {
    console.error("Error fetching ranking:", error);
    throw new HttpsError("internal", "랭킹 데이터를 가져오는 중 오류가 발생했습니다.");
  }
});

// const db = admin.database();

// export const generateNicknameOnUserCreate = functions.database
//     .ref("/Users/{userId}")
//     .onCreate(async (snapshot, context) => {
//       const userId = context.params.userId;
//       const userRef = snapshot.ref;

//       // Step 1. 기본 닉네임 생성
//       const baseNickname = `Player_${userId.substring(0, 6)}`;
//       let nickname = baseNickname;
//       let counter = 0;

//       // Step 2. 중복 닉네임 확인
//       const nicknamesSnapshot = await db.ref("UserNicknames").once("value");
//       const existingNicknames = new Set(
//           Object.values(nicknamesSnapshot.val() || {}),
//       );

//       while (existingNicknames.has(nickname)) {
//         counter++;
//         nickname = `${baseNickname}_${counter}`;
//       }

//       // Step 3. 유저 데이터에 닉네임 저장
//       await userRef.update({nickname});

//       // Step 4. 닉네임 전용 노드에 추가 (중복 방지용)
//       await db.ref(`UserNicknames/${userId}`).set(nickname);

//       console.log(`닉네임 생성 완료: ${nickname}`);
//       return null;
//     });

