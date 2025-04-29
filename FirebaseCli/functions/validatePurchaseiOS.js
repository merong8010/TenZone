import functions from "firebase-functions";
import axios from "axios";

const APPLE_PRODUCTION_URL = "https://buy.itunes.apple.com/verifyReceipt";
const APPLE_SANDBOX_URL = "https://sandbox.itunes.apple.com/verifyReceipt";

// Firebase Function
export const validatePurchaseiOS = functions.https.onRequest(async (req, res) => {
  if (req.method !== "POST") {
    return res.status(405).send({success: false, message: "Only POST requests are allowed."});
  }

  try {
    const {receiptData, password} = req.body;

    if (!receiptData) {
      return res.status(400).send({success: false, message: "Missing receipt data."});
    }

    const requestBody = {
      "receipt-data": receiptData,
      "password": password, // Optional, only needed for auto-renewable subscriptions
      "exclude-old-transactions": true,
    };

    // Step 1: Production 요청
    const response = await axios.post(APPLE_PRODUCTION_URL, requestBody, {
      headers: {"Content-Type": "application/json"},
    });

    const {status, receipt} = response.data;

    // Step 2: Sandbox fallback
    if (status === 21007) { // receipt is from sandbox
      const sandboxResponse = await axios.post(APPLE_SANDBOX_URL, requestBody, {
        headers: {"Content-Type": "application/json"},
      });

      return res.status(200).send({
        success: true,
        data: sandboxResponse.data,
      });
    }

    if (status === 0) {
      return res.status(200).send({
        success: true,
        data: receipt,
      });
    } else {
      return res.status(400).send({
        success: false,
        message: `Apple verification failed. Status code: ${status}`,
      });
    }
  } catch (error) {
    console.error("iOS 검증 실패:", error?.response?.data || error.message || error);
    return res.status(500).send({success: false, message: "Internal server error."});
  }
});
