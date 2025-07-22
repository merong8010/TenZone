import admin from "firebase-admin";
import functions from "firebase-functions";
import {google} from "googleapis";

if (!admin.apps.length) {
  admin.initializeApp();
}

const packageName = "studio.jjm.maketen";

const serviceAccountEmail = functions.config().serviceaccount.client_email;
const privateKey = functions.config().serviceaccount.private_key.replace(/\\n/g, "\n");

const authClient = new google.auth.JWT(
    serviceAccountEmail,
    null,
    privateKey,
    ["https://www.googleapis.com/auth/androidpublisher"],
);

const playDeveloperApi = google.androidpublisher({version: "v3", auth: authClient});

export const validatePurchase = functions.https.onRequest(async (req, res) => {
  if (req.method !== "POST") {
    return res.status(405).send({success: false, message: "Only POST requests are allowed"});
  }

  try {
    const {productId, purchaseToken} = req.body;

    if (!productId || !purchaseToken) {
      return res.status(400).send({success: false, message: "Invalid request."});
    }

    // const receiptData = JSON.parse(receipt);
    // const purchaseToken = receiptData.purchaseToken;

    // if (!purchaseToken) {
    //   return res.status(400).send({success: false, message: "Invalid receipt data."});
    // }

    await authClient.authorize();
    console.log("serviceAccountEmail : "+serviceAccountEmail);
    console.log("productId : "+productId);
    const result = await playDeveloperApi.purchases.products.get({
      packageName,
      productId,
      token: purchaseToken,
    });

    const purchaseState = result.data.purchaseState;
    const acknowledgementState = result.data.acknowledgementState;

    if (purchaseState === 0) { // 0: PURCHASED (구매 완료)
      // 👇 --- 추가된 부분 시작 --- 👇
      // 아직 구매 '확인'(Acknowledge)이 되지 않은 경우에만 실행
      if (acknowledgementState === 0) { // 0: YET_TO_BE_ACKNOWLEDGED
        await playDeveloperApi.purchases.products.acknowledge({
          packageName,
          productId,
          token: purchaseToken,
        });
      }
      // 👆 --- 추가된 부분 끝 --- 👆
      return res.status(200).send({success: true});
    } else {
      return res.status(200).send({success: false, message: "Purchase not completed."});
    }
  } catch (error) {
    console.error("검증 실패", error?.errors || error.message || error);
    return res.status(500).send({success: false, message: "Internal server error"});
  }
});
