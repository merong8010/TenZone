import {getDatabase} from "firebase-admin/database";
import admin from "firebase-admin";
import functions from "firebase-functions";

// Firebase Admin SDK 초기화 (이미 되어있지 않다면)
if (!admin.apps.length) {
  admin.initializeApp();
}

// 허용된 게임 레벨 목록 (RankUpdateScheduler와 동일하게 유지)
const ALLOWED_GAME_LEVELS = ["Easy", "Normal", "Hard", "Expert"];
// 허용된 날짜 형식 (RankUpdateScheduler와 동일하게 유지)
// 현재는 "ALL" 또는 yyyy-MM-dd 형식으로 오는 문자열을 예상합니다.

export const SubmitScore = functions.https.onCall(async (data, context) => {
  // --- 보안 강화: 클라이언트가 보낸 userId를 사용하지 않고,
  // --- Firebase Authentication을 통해 인증된 사용자의 UID를 사용합니다.
  if (!context.auth) {
    throw new functions.https.HttpsError(
        "unauthenticated",
        "이 함수는 인증된 사용자만 호출할 수 있습니다.",
    );
  }
  const currentUserUid = context.auth.uid;

  // --- 입력 데이터 유효성 검사 ---
  const nickname = data.nickname || "Unknown";
  const gameLevel = data.gameLevel;
  const level = data.level || 0;
  const countryCode = data.countryCode || "US";
  // date가 없으면 기본값 "ALL" 사용. typeof 검사 추가.
  const date = (data.date === undefined || data.date === null || typeof data.date !== "string") ? "ALL" : data.date;
  // 점수 및 관련 데이터 유효성 검사
  const point = data.point;
  const timeStamp = data.timeStamp || admin.database.ServerValue.TIMESTAMP; // 서버 타임스탬프 사용 권장

  // gameLevel이 제공되었고 허용된 값인지 확인합니다.
  if (!gameLevel || typeof gameLevel !== "string" || !ALLOWED_GAME_LEVELS.includes(gameLevel)) {
    throw new functions.https.HttpsError(
        "invalid-argument",
        `유효한 gameLevel이 제공되어야 합니다. 허용된 값: ${ALLOWED_GAME_LEVELS.join(", ")}`,
    );
  }

  // point가 유효한 숫자인지 확인합니다. (음수 점수 허용 여부는 게임 규칙에 따름)
  if (typeof point !== "number") {
    throw new functions.https.HttpsError(
        "invalid-argument",
        "유효한 point 값이 제공되어야 합니다.",
    );
  }

  // timeStamp 유효성 검사 (클라이언트에서 보낼 경우)
  if (data.timeStamp !== undefined && typeof data.timeStamp !== "number") {
    throw new functions.https.HttpsError(
        "invalid-argument",
        "timeStamp가 유효한 숫자여야 합니다.",
    );
  }

  // 데이터베이스 경로 설정
  const userScoreRef = getDatabase().ref(`Leaderboard/${gameLevel}/${date}/${currentUserUid}`);

  try {
    // 해당 사용자의 현재 최고 점수를 가져옵니다.
    const snapshot = await userScoreRef.once("value");
    const currentScoreData = snapshot.val();

    let isNewHighScore = false;

    // 현재 기록이 없거나, 새로운 점수가 기존 최고 점수보다 높을 경우 업데이트
    // RankUpdateScheduler와 동일한 정렬 기준을 사용해야 합니다.
    if (!currentScoreData || typeof currentScoreData.point !== "number") {
      // 기존 기록이 없는 경우
      isNewHighScore = true;
      console.log(`새로운 사용자 ${currentUserUid}의 기록 등록: ${point}`);
    } else {
      // 기존 기록이 있는 경우, 새로운 점수와 비교
      const currentPoint = currentScoreData.point;
      const currentTimeStamp = currentScoreData.timeStamp !== undefined ? currentScoreData.timeStamp : 0;

      // RankUpdateScheduler의 정렬 로직과 동일하게 비교
      if (point > currentPoint) {
        isNewHighScore = true;
      } else if (point === currentPoint) {
        if (timeStamp < currentTimeStamp) { // 타임스탬프 더 빠르면 승
          isNewHighScore = true;
        }
      }

      if (isNewHighScore) {
        console.log(`사용자 ${currentUserUid}의 새로운 최고 기록 갱신: ${point} (이전: ${currentPoint})`);
      } else {
        console.log(`사용자 ${currentUserUid}의 기록 ${point}가 기존 최고 기록 ${currentPoint}보다 낮거나 같음. 업데이트 안함.`);
      }
    }


    // 새로운 최고 기록이면 데이터베이스 업데이트
    if (isNewHighScore) {
      const updates = {
        point: point,
        nickname: nickname,
        level: level,
        countryCode: countryCode,
        // remainMilliSeconds가 입력 데이터에 있으면 업데이트, 없으면 스킵하거나 제거 (게임 규칙에 따름)
        // timeStamp는 서버 타임스탬프를 사용하는 것이 가장 정확하고 안전합니다.
        // 클라이언트에서 timeStamp를 보내더라도 서버 타임스탬프를 사용하거나 검증하세요.
        // 여기서는 입력 데이터에 없으면 ServerValue.TIMESTAMP 사용
        timeStamp: data.timeStamp || admin.database.ServerValue.TIMESTAMP,
        // 닉네임 등 다른 업데이트할 정보가 있다면 여기에 추가
        // 예: nickname: data.nickname // 닉네임 변경 허용 시
      };

      // 주의: 여기서 'rank' 필드를 직접 업데이트하지 않습니다!
      // rank 필드는 RankUpdateScheduler가 관리합니다.
      // updates.rank = ... (추가하지 마세요!)

      await userScoreRef.update(updates);

      // 성공 응답 반환 (클라이언트에게 최고 기록 갱신 여부 등을 알림)
      return {success: true, isNewHighScore: true, newScore: point, message: "최고 기록이 업데이트되었습니다."};
    } else {
      // 최고 기록 갱신이 아니면 업데이트하지 않고 응답 반환
      return {success: true, isNewHighScore: false, message: "기존 최고 기록보다 낮거나 같습니다."};
    }
  } catch (error) {
    console.error("SubmitScore 오류:", error);
    // 클라이언트에게 일반적인 오류 메시지 반환
    throw new functions.https.HttpsError("internal", "점수 제출 중 오류가 발생했습니다.");
  }
});
