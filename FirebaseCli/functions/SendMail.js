import functions from "firebase-functions";
import admin from "firebase-admin";
if (!admin.apps.length) {
  admin.initializeApp();
}

export const SendMail = functions.https.onCall(async (data, context) => {
  const targetId = data.id;
  const mail = {
    title: data.title,
    desc: data.desc,
    receiveDate: Date.now(),
    rewards: data.rewards, // rewards는 [{type: "Gold", amount: 100}, ...]
  };

  const db = admin.database();

  if (!targetId || targetId.trim() === "") {
    // 모든 유저에게 전송
    const usersSnapshot = await db.ref("Users").once("value");
    const updates = {};

    usersSnapshot.forEach((userSnap) => {
      const userId = userSnap.key;
      const mailId = db.ref().push().key; // 고유한 mail ID 생성
      if (userId && mailId) {
        updates[`Users/${userId}/mailDatas/${mailId}`] = mail;
      }
    });

    await db.ref().update(updates);

    return {success: true, message: "우편이 모든 유저에게 전송되었습니다."};
  } else {
    // 특정 유저에게만 전송
    const userRef = db.ref(`Users/${targetId}`);
    const userSnapshot = await userRef.once("value");

    if (!userSnapshot.exists()) {
      throw new functions.https.HttpsError("not-found", "해당 ID의 유저를 찾을 수 없습니다.");
    }

    const mailId = db.ref().push().key;
    if (!mailId) {
      throw new functions.https.HttpsError("internal", "메일 ID 생성 실패");
    }

    await userRef.child(`mailDatas/${mailId}`).set(mail);

    return {success: true, message: `유저 ${targetId}에게 전송되었습니다.`};
  }
});
