const {google} = require('googleapis');
const admin = require('firebase-admin');

// 🔑 서비스 계정 키 파일
const serviceAccount = require('./google-service-account.json');
const GOOGLE_KEY_FILE = 'google-service-account.json';

// Firebase 초기화
admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  databaseURL: 'https://maketen-2631f-default-rtdb.asia-southeast1.firebasedatabase.app/'
});

// Google Sheets API 초기화
const auth = new google.auth.GoogleAuth({
  keyFile: GOOGLE_KEY_FILE,
  scopes: ['https://www.googleapis.com/auth/spreadsheets.readonly']
});

const sheets = google.sheets({ version: 'v4', auth });

// 스프레드시트 ID
const spreadsheetId = '1f2XBozAkYcQFEtvzSuYvvjUahrnq0VQxAxmGZUDdoIk';

async function importAllSheets() {
  const db = admin.database();

  // 시트 이름 가져오기
  const metadata = await sheets.spreadsheets.get({ spreadsheetId });
  const sheetTitles = metadata.data.sheets.map(sheet => sheet.properties.title);

  for (const title of sheetTitles) {
    console.log(`📄 Processing sheet: ${title}`);

    const response = await sheets.spreadsheets.values.get({
      spreadsheetId,
      range: `${title}!A1:Z`,
    });

    const rows = response.data.values;
    if (!rows || rows.length === 0) {
      console.log(`⚠️ No data found in sheet: ${title}`);
      continue;
    }

    const headers = rows[0];
    const data = rows.slice(1).map(row => {
      const item = {};
      headers.forEach((header, index) => {
        item[header] = row[index] || null;
      });
      return item;
    });

    const ref = db.ref(`GameData/${title}`);
    await ref.set(data);

    console.log(`✅ Sheet "${title}" data uploaded to Firebase.`);
  }

  console.log('🚀 All sheets imported successfully!');
}

importAllSheets().catch(console.error);
