/**
 * Import function triggers from their respective submodules:
 *
 * const {onCall} = require("firebase-functions/v2/https");
 * const {onDocumentWritten} = require("firebase-functions/v2/firestore");
 *
 * See a full list of supported triggers at https://firebase.google.com/docs/functions
 */

const {onRequest} = require("firebase-functions/v2/https");
const logger = require("firebase-functions/logger");
const { Timestamp } = require("firebase-admin/firestore");

// Create and deploy your first functions
// https://firebase.google.com/docs/functions/get-started

// exports.helloWorld = onRequest((request, response) => {
//   logger.info("Hello logs!", {structuredData: true});
//   response.send("Hello from Firebase!");
// });
const functions = require('firebase-functions');
const admin = require('firebase-admin');

admin.initializeApp();

exports.GetRanking = functions.https.onCall(async (data, context) => {
    const gameLevel = data.gameLevel;
    const date = data.date || 'ALL';
    const userId = data.userId;
    const limit = data.limit || 10;

    try {
        const snapshot = null;
        if(date == '')
        {
            snapshot = await admin.database().ref(`Leaderboard/${gameLevel}/${date}`).once('value');
        }
        const rankingList = [];
        snapshot.forEach(child => {
            const entry = child.val();
            rankingList.push({
                id: child.key,
                name: entry.name || '',
                point: entry.point || 0,
                remainMilliSeconds: entry.remainMilliSeconds || 0,
                countryCode : entry.countryCode || US,
                timeStamp : entry.timeStamp || 0
            });
        });

        // 정렬
        rankingList.sort((a, b) => {
            if (b.point !== a.point) return b.point - a.point;
            if (b.remainMilliSeconds != a.remainMilliSeconds) b.remainMilliSeconds - a.remainMilliSeconds;
            return b.timeStamp - a.timeStamp;
        });

        // 상위 limit명
        const topRankings = rankingList.slice(0, limit);

        // 내 랭킹 찾기
        const myRankIndex = rankingList.findIndex(entry => entry.userId === userId);
        const myRank = myRankIndex >= 0 ? myRankIndex + 1 : -1;
        const myEntry = myRankIndex >= 0 ? rankingList[myRankIndex] : null;

        return {
            topRankings,
            myRank,
            myEntry
        };
    } catch (error) {
        console.error('Error fetching ranking:', error);
        throw new functions.https.HttpsError('internal', '랭킹 데이터를 가져오는 중 오류가 발생했습니다.');
    }
});

