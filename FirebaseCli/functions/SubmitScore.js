import admin from "firebase-admin";
import functions from "firebase-functions";
const {region} = functions;
if (!admin.apps.length) {
  admin.initializeApp();
}

export const SubmitScore = region("asia-southeast1").https.onCall(async (data, context) => {
  const gameLevel = data.gameLevel;
  const date = data.date;
  const id = data.id;
  const level = data.level;
  const name = data.name;
  const point = data.point;
  const remainMilliSeconds = data.remainMilliSeconds;
  const countryCode = data.countryCode;
  const timeStamp = data.timeStamp;

  const db = admin.database();
  const ref = db.ref(`Leaderboard/${gameLevel}/${date}`);

  // 1. 사용자 점수 등록
  await ref.child(id).set({
    id: id,
    level: level,
    name: name,
    point: point,
    remainMilliSeconds: remainMilliSeconds,
    countryCode: countryCode,
    timeStamp: timeStamp,
  });

  // 2. 전체 유저 데이터 가져오기
  const snapshot = await ref.once("value");
  const users = [];

  snapshot.forEach((child) => {
    users.push(child.val());
  });

  // 3. 점수 내림차순 정렬
  users.sort((a, b) => {
    if (b.point !== a.point) return b.point - a.point;
    if (b.remainMilliSeconds !== a.remainMilliSeconds) return b.remainMilliSeconds - a.remainMilliSeconds;
    return b.timeStamp - a.timeStamp;
  });
  // 4. 순위 계산
  const myRank = users.findIndex((user) => user.id === id) + 1;
  // 5. 응답 구성 (내 순위 + 상위 10명)
  // const top10 = users.slice(0, 10);
  return {
    myRank: myRank,
  };
});
