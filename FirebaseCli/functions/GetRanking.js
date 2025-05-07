import {getDatabase} from "firebase-admin/database";
import admin from "firebase-admin";
import functions from "firebase-functions";

// Firebase Admin SDK 초기화 (이미 되어있지 않다면)
if (!admin.apps.length) {
  admin.initializeApp();
}
// 허용된 게임 레벨 목록 (필요에 따라 업데이트)
const ALLOWED_GAME_LEVELS = ["Easy", "Normal", "Hard", "Expert"];
// 허용된 날짜 형식 (필요에 따라 정규식 등으로 더 엄격하게 검사 가능)
// 현재는 "ALL" 또는 yyyy-MM-dd 형식으로 오는 문자열을 예상합니다.
export const GetRanking = functions.https.onCall(async (data, context) => {
  // --- 보안 강화: 클라이언트가 보낸 userId를 사용하지 않고,
  // --- Firebase Authentication을 통해 인증된 사용자의 UID를 사용합니다.
  // --- context.auth는 HTTPS 호출 가능 함수를 호출한 사용자의 인증 정보입니다.
  if (!context.auth) {
    // 인증되지 않은 사용자의 호출은 거부합니다.
    throw new functions.https.HttpsError(
        "unauthenticated",
        "이 함수는 인증된 사용자만 호출할 수 있습니다.",
    );
  }
  // 현재 함수를 호출한 사용자의 UID를 가져옵니다.
  const currentUserUid = context.auth.uid;

  // --- 입력 데이터 유효성 검사 ---
  const gameLevel = data.gameLevel;
  // date가 없으면 기본값 "ALL" 사용. typeof 검사 추가.
  const date = (data.date === undefined || data.date === null || typeof data.date !== "string") ? "ALL" : data.date;
  // limit이 없으면 기본값 10 사용. 유효성 검사 강화.
  const limit = data.limit === undefined ? 10 : data.limit;

  // gameLevel이 제공되었고 허용된 값인지 확인합니다.
  if (!gameLevel || typeof gameLevel !== "string" || !ALLOWED_GAME_LEVELS.includes(gameLevel)) {
    throw new functions.https.HttpsError(
        "invalid-argument",
        `유효한 gameLevel이 제공되어야 합니다. 허용된 값: ${ALLOWED_GAME_LEVELS.join(", ")}`,
    );
  }

  // limit이 유효한 숫자인지, 합리적인 범위 내에 있는지 확인합니다.
  // 예를 들어, 1부터 100 사이의 값으로 제한할 수 있습니다.
  const MAX_LIMIT = 100; // 최대 limit 설정
  if (typeof limit !== "number" || !Number.isInteger(limit) || limit <= 0 || limit > MAX_LIMIT) {
    throw new functions.https.HttpsError(
        "invalid-argument",
        `limit은 1부터 ${MAX_LIMIT} 사이의 유효한 정수여야 합니다.`,
    );
  }

  // date 형식에 대한 유효성 검사 (필요에 따라 강화)
  // 현재는 단순히 문자열인지 확인하며, 'ALL' 또는 YYYY-MM-DD 형식을 가정합니다.
  if (typeof date !== "string") {
    throw new functions.https.HttpsError(
        "invalid-argument",
        "date는 유효한 문자열이어야 합니다.",
    );
  }


  try {
    // Realtime Database에서 해당 경로의 상위 limit 개 랭킹 데이터를 가져옵니다.
    // RankUpdateScheduler 함수가 주기적으로 rank 필드를 업데이트한다는 전제 하에,
    // orderByChild("rank")와 limitToFirst(limit)를 사용하여 상위 N명을 효율적으로 가져옵니다.
    // --- 주의: RankUpdateScheduler가 정상 작동하지 않으면 rank 필드가 부정확할 수 있습니다.
    const leaderboardRef = getDatabase().ref(`Leaderboard/${gameLevel}/${date}`);
    const topNSnapshot = await leaderboardRef.orderByChild("rank").limitToFirst(limit).once("value");

    const topRankings = [];
    // 스냅샷의 각 자식 노드를 순회하며 상위 N명의 데이터 추출
    topNSnapshot.forEach((child) => {
      const entry = child.val();
      // 데이터 구조에 따라 필요한 필드를 가져와 객체 생성
      // 클라이언트 코드와 일치하도록 합니다.
      // 없는 필드는 기본값 설정
      if (entry) { // entry 데이터 자체의 존재 여부 확인
        topRankings.push({
          id: child.key, // 사용자 UID
          rank: typeof entry.rank === "number" ? entry.rank : 0, // 숫자인지 확인하고 기본값 설정
          level: typeof entry.level === "number" ? entry.level : 0,
          nickname: typeof entry.nickname === "string" && entry.nickname ? entry.nickname : child.key, // 닉네임 없으면 UID 사용
          point: typeof entry.point === "number" ? entry.point : 0, // point는 중요 필드, 숫자인지 확인
          countryCode: typeof entry.countryCode === "string" && entry.countryCode ? entry.countryCode : "US",
          timeStamp: typeof entry.timeStamp === "number" ? entry.timeStamp : 0, // timeStamp
          // 필요한 다른 필드가 있다면 여기에 추가
        });
      } else {
        console.warn(`경로 ${leaderboardRef.toString()}에서 상위 N명 조회 중 유효하지 않은 데이터 발견 (User ID: ${child.key})`);
      }
    });

    // 현재 사용자의 랭킹 정보 가져오기
    // 상위 N명 목록에 현재 사용자가 포함되어 있는지 먼저 확인합니다.
    let myRankEntry = topRankings.find((entry) => entry.id === currentUserUid);
    let myRank = myRankEntry ? myRankEntry.rank : -1; // 상위 N에 있으면 해당 순위, 없으면 -1

    // 상위 N명에 포함되지 않은 경우, 해당 사용자의 데이터를 개별적으로 가져옵니다.
    if (!myRankEntry) {
      const myEntrySnapshot = await leaderboardRef.child(currentUserUid).once("value");

      if (myEntrySnapshot.exists()) {
        const entry = myEntrySnapshot.val();
        if (entry) { // entry 데이터 자체의 존재 여부 확인
          myRankEntry = {
            id: currentUserUid, // 사용자 UID
            rank: typeof entry.rank === "number" ? entry.rank : 0, // 숫자인지 확인하고 기본값 설정
            level: typeof entry.level === "number" ? entry.level : 0,
            nickname: typeof entry.nickname === "string" && entry.nickname ? entry.nickname : currentUserUid,
            point: typeof entry.point === "number" ? entry.point : 0, // point는 중요 필드, 숫자인지 확인
            countryCode: typeof entry.countryCode === "string" && entry.countryCode ? entry.countryCode : "US",
            timeStamp: typeof entry.timeStamp === "number" ? entry.timeStamp : 0, // timeStamp
            // 필요한 다른 필드가 있다면 여기에 추가
          };
          // RankUpdateScheduler가 rank 필드를 업데이트한다는 전제 하에 이 값을 사용
          myRank = typeof entry.rank === "number" ? entry.rank : -1;
        } else {
          console.warn(`경로 ${leaderboardRef.child(currentUserUid).toString()}에서 현재 사용자 데이터가 비어있음.`);
          // 데이터가 없거나 유효하지 않으면 순위 정보 없음을 나타냄
          myRankEntry = null;
          myRank = -1;
        }
      } else {
        // 해당 사용자의 리더보드 기록이 없는 경우
        console.log(`현재 사용자 ${currentUserUid}의 랭킹 기록이 ${leaderboardRef.toString()}에 없습니다.`);
        myRankEntry = null;
        myRank = -1;
      }
    }
    // 클라이언트로 반환할 데이터
    return {topRankings, myRank, myEntry: myRankEntry};
  } catch (error) {
    console.error("Error fetching ranking:", error);
    // 클라이언트에게는 일반적인 오류 메시지를 반환합니다.
    // 실제 오류 내용은 서버 로그에서 확인합니다.
    throw new functions.https.HttpsError("internal", "랭킹 데이터를 가져오는 중 서버 오류가 발생했습니다.");
  }
});
