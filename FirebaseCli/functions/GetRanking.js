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

if (!admin.apps.length) {
  admin.initializeApp();
}

export const GetRanking = functions.https.onCall(async (data, context) => {
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
        rank: entry.rank || 0,
        level: entry.level || 0,
        nickname: entry.nickname || child.key,
        point: entry.point || 0,
        countryCode: entry.countryCode || "US",
        timeStamp: entry.timeStamp || 0,
      });
    });

    rankingList.sort((a, b) => {
      if (b.point !== a.point) return b.point - a.point;
      return a.timeStamp - b.timeStamp;
    });

    const topRankings = rankingList.slice(0, limit);
    const myRankIndex = rankingList.findIndex((entry) => entry.id === userId);
    const myRank = myRankIndex >= 0 ? myRankIndex + 1 : -1;
    const myEntry = myRankIndex >= 0 ? rankingList[myRankIndex] : null;

    return {topRankings, myRank, myEntry};
  } catch (error) {
    console.error("Error fetching ranking:", error);
    throw new functions.https.HttpsError("internal", "랭킹 데이터를 가져오는 중 오류가 발생했습니다.");
  }
});
