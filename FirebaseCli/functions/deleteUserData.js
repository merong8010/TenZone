import functions from "firebase-functions";
import admin from "firebase-admin";

if (!admin.apps.length) {
  admin.initializeApp();
}

export const deleteUserData = functions.https.onCall(async (data, context) => {
  const anonymousUid = data.anonymousUid;
  if (!anonymousUid) {
    throw new functions.https.HttpsError("invalid-argument", "anonymousUid가 필요합니다.");
  }

  try {
    const userRecord = await admin.auth().getUser(anonymousUid);

    if (userRecord.email || (userRecord.providerData && userRecord.providerData.length > 0)) {
      throw new functions.https.HttpsError("failed-precondition", "익명 계정이 아닙니다.");
    }

    // 익명 계정 삭제
    await admin.auth().deleteUser(anonymousUid);

    // 익명 데이터 삭제
    await admin.database().ref(`/Users/${anonymousUid}`).remove();

    console.log(`익명 계정 및 데이터 삭제 완료: ${anonymousUid}`);
    return {success: true};
  } catch (error) {
    console.error("마이그레이션 실패:", error);
    throw new functions.https.HttpsError("internal", error.message || "마이그레이션 실패");
  }
});
