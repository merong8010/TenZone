/**
 * Import function triggers from their respective submodules:
 *
 * const {onCall} = require("firebase-functions/v2/https");
 * const {onDocumentWritten} = require("firebase-functions/v2/firestore");
 *
 * See a full list of supported triggers at https://firebase.google.com/docs/functions
 */


// Create and deploy your first functions
// https://firebase.google.com/docs/functions/get-started

// exports.helloWorld = onRequest((request, response) => {
//   logger.info("Hello logs!", {structuredData: true});
//   response.send("Hello from Firebase!");
// });
import admin from "firebase-admin";
import functions from "firebase-functions";
if (!admin.apps.length) {
  admin.initializeApp();
}
const db = admin.database();

export const generateNicknameOnUserCreate = functions.database
    .ref("/Users/{userId}")
    .onCreate(async (snapshot, context) => {
      const userId = context.params.userId;
      const userRef = snapshot.ref;

      // Step 1. 기본 닉네임 생성
      const baseNickname = `Player_${userId.substring(0, 6)}`;
      let nickname = baseNickname;
      let counter = 0;

      // Step 2. 중복 닉네임 확인
      const nicknamesSnapshot = await db.ref("UserNicknames").once("value");
      const existingNicknames = new Set(
          Object.values(nicknamesSnapshot.val() || {}),
      );

      while (existingNicknames.has(nickname)) {
        counter++;
        nickname = `${baseNickname}_${counter}`;
      }

      // Step 3. 유저 데이터에 닉네임 저장
      await userRef.update({nickname});

      // Step 4. 닉네임 전용 노드에 추가 (중복 방지용)
      await db.ref(`UserNicknames/${userId}`).set(nickname);

      console.log(`닉네임 생성 완료: ${nickname}`);
      return null;
    });

