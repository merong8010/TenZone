import admin from "firebase-admin";
import functions from "firebase-functions";

// Firebase Admin SDK 초기화 (이미 되어있지 않다면)
if (!admin.apps.length) {
  admin.initializeApp();
}

const DIFFICULTIES = ["Easy", "Normal", "Hard", "Expert"]; // 필요한 난이도만 추가

/**
 * 특정 Realtime Database 경로의 데이터를 가져와 순위를 계산하고 업데이트하는 헬퍼 함수
 * @param {string} path - 리더보드 데이터 경로 (예: `Leaderboard/Normal/ALL`)
 * @param {string[]} sortKeys - 정렬 기준 키 배열 (예: ['point', 'remainMilliSeconds', 'timeStamp'])
 * @param {string[]} sortOrder - 각 키에 대한 정렬 순서 ('asc' 또는 'desc') 배열
 */
async function updateLeaderboardRanks(path, sortKeys, sortOrder) {
  const db = admin.database();
  const ref = db.ref(path);
  console.log(`랭킹 업데이트 시작: ${path}`);

  try {
    const snapshot = await ref.once("value");

    if (!snapshot.exists()) {
      console.log(`${path} 경로에 데이터 없음`);
      return; // 데이터가 없으면 업데이트할 것도 없으니 종료
    }

    const users = [];
    snapshot.forEach((child) => {
      const data = child.val();
      // 필요한 데이터 필드가 최소한 존재하는지 확인
      // 모든 sortKeys에 대한 데이터가 있는지 확인하는 것이 더 안전할 수 있습니다.
      // 여기서는 point 또는 exp 등 주요 키의 존재 여부만 간단히 확인합니다.
      if (data && (data[sortKeys[0]] !== undefined)) {
        const userEntry = {id: child.key};
        sortKeys.forEach((key) => {
          userEntry[key] = data[key] !== undefined ? data[key] : (typeof data[sortKeys[0]] === "number" ? 0 : "");
        });
        // rank 필드가 이미 있으면 가져옴 (쓰기 시 덮어씀)
        userEntry.rank = data.rank !== undefined ? data.rank : 0; // 기존 rank 값 가져오기

        users.push(userEntry);
      } else {
        console.warn(`경로 ${path}에서 유효하지 않은 데이터 발견 (User ID: ${child.key})`, data);
      }
    });

    // 데이터 정렬
    users.sort((a, b) => {
      for (let i = 0; i < sortKeys.length; i++) {
        const key = sortKeys[i];
        const order = sortOrder[i];

        if (a[key] < b[key]) {
          return order === "asc" ? -1 : 1;
        }
        if (a[key] > b[key]) {
          return order === "asc" ? 1 : -1;
        }
        // 같으면 다음 정렬 기준으로 넘어감
      }
      return 0; // 모든 키가 같으면 순서 변경 없음
    });

    // 계산된 순위 저장 객체 생성
    const updates = {};
    users.forEach((user, index) => {
      const calculatedRank = index + 1;
      // 기존 순위와 다를 경우에만 업데이트를 추가하여 불필요한 쓰기 방지 (선택 사항)
      // if (user.rank !== calculatedRank) {
      updates[`${user.id}/rank`] = calculatedRank;
      // }
    });

    // 업데이트할 데이터가 있으면 Realtime Database에 적용
    if (Object.keys(updates).length > 0) {
      // --- 주의: 업데이트 객체의 크기가 Realtime Database의 쓰기 한도를 초과할 수 있습니다.
      // --- 매우 큰 리더보드의 경우, updates 객체를 여러 개로 분할하여 배치 쓰기를 수행해야 할 수 있습니다.
      await ref.update(updates);
      console.log(`${path} 랭킹 ${Object.keys(updates).length}개 갱신 완료`);
    } else {
      console.log(`${path} 랭킹 변경 사항 없음 (업데이트 스킵)`);
    }
  } catch (error) {
    console.error(`RankUpdateScheduler - ${path} 처리 중 오류:`, error);
    // 이 함수 내에서 에러를 throw하지 않고 로깅만 하여 다른 카테고리 처리에 영향 주지 않도록 함
  }
}


// exports.RankUpdateScheduler = functions.pubsub.schedule("0 * * * *") // 매시 0분
export const RankUpdateScheduler = functions.pubsub.schedule("0 * * * *") // 매시 0분
    .timeZone("Europe/London") // 런던 시간대
    .onRun(async (context) => {
      console.log("RankUpdateScheduler 함수 실행 시작");

      const today = new Date().toISOString().split("T")[0]; // yyyy-MM-dd

      // 각 난이도별 ALL 및 오늘 날짜 랭킹 업데이트
      for (const difficulty of DIFFICULTIES) {
        // ALL 랭킹 업데이트
        await updateLeaderboardRanks(
            `Leaderboard/${difficulty}/ALL`,
            ["point", "timeStamp"],
            ["desc", "asc"], // 정렬 순서: point 내림차순, remainMilliSeconds 내림차순, timeStamp 오름차순
        );

        // 오늘 날짜 랭킹 업데이트
        await updateLeaderboardRanks(
            `Leaderboard/${difficulty}/${today}`,
            ["point", "timeStamp"],
            ["desc", "asc"], // 정렬 순서: point 내림차순, remainMilliSeconds 내림차순, timeStamp 오름차순
        );
      }

      // Exp 랭킹 업데이트
      await updateLeaderboardRanks(
          `Leaderboard/Exp`,
          ["point", "timeStamp"],
          ["desc", "asc"], // 정렬 순서: exp 내림차순, timeStamp 오름차순
      );

      console.log("RankUpdateScheduler 함수 실행 완료");
      return null; // 스케줄링된 함수의 경우 null 또는 Promise<void> 반환
    });
