import admin from "firebase-admin";
import functions from "firebase-functions";

if (!admin.apps.length) {
  admin.initializeApp();
}

const db = admin.database();

export const changeNickname = functions.https.onCall(async (data, context) => {
  // 인증 체크
  const uid = context.auth?.uid;
  if (!uid) {
    throw new functions.https.HttpsError("unauthenticated", "로그인이 필요합니다.");
  }

  const newNickname = data.nickname?.trim();
  if (!newNickname || newNickname.length < 2 || newNickname.length > 12) {
    throw new functions.https.HttpsError("invalid-argument", "닉네임은 2~12자 사이여야 합니다.");
  }

  const nicknameRef = db.ref(`UserNicknames/${newNickname}`);
  const userRef = db.ref(`Users/${uid}`);

  // 중복 체크
  const snapshot = await nicknameRef.once("value");
  if (snapshot.exists()) {
    throw new functions.https.HttpsError("already-exists", "이미 사용 중인 닉네임입니다.");
  }

  // 기존 닉네임 제거
  const currentNicknameSnap = await userRef.child("nickname").once("value");
  const currentNickname = currentNicknameSnap.val();
  const updates = {};

  if (currentNickname) {
    updates[`UserNicknames/${currentNickname}`] = null;
  }

  // 새 닉네임 설정
  updates[`Users/${uid}/nickname`] = newNickname;
  updates[`UserNicknames/${newNickname}`] = uid;

  await db.ref().update(updates);
  return {success: true, nickname: newNickname};
});
