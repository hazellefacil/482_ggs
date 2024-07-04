import cv2
import mediapipe as mp
import os
from google.protobuf.json_format import MessageToDict
import matplotlib.pyplot as plt

# Initialize MediaPipe hands module
mpHands = mp.solutions.hands
mpDraw = mp.solutions.drawing_utils
hands = mpHands.Hands(
    static_image_mode=False,
    model_complexity=1,
    min_detection_confidence=0.90,
    min_tracking_confidence=0.75,
    max_num_hands=2)

# Start capturing video from webcam
cap = cv2.VideoCapture(0)


def classify_gesture(hand_landmarks):
    # Implement a simple gesture classifier
    thumb_tip = hand_landmarks.landmark[mpHands.HandLandmark.THUMB_TIP]
    index_tip = hand_landmarks.landmark[mpHands.HandLandmark.INDEX_FINGER_TIP]
    middle_tip = hand_landmarks.landmark[mpHands.HandLandmark.MIDDLE_FINGER_TIP]
    ring_tip = hand_landmarks.landmark[mpHands.HandLandmark.RING_FINGER_TIP]
    pinky_tip = hand_landmarks.landmark[mpHands.HandLandmark.PINKY_TIP]

    # Simple open hand gesture (all fingers up)

    # distal interphalangeal joint (DIP)
    if (thumb_tip.y <= hand_landmarks.landmark[mpHands.HandLandmark.THUMB_IP].y and
            index_tip.y < hand_landmarks.landmark[mpHands.HandLandmark.INDEX_FINGER_DIP].y and
            middle_tip.y < hand_landmarks.landmark[mpHands.HandLandmark.MIDDLE_FINGER_DIP].y and
            ring_tip.y < hand_landmarks.landmark[mpHands.HandLandmark.RING_FINGER_DIP].y and
            pinky_tip.y < hand_landmarks.landmark[mpHands.HandLandmark.PINKY_DIP].y):
        return "Open Hand"

    # Simple fist gesture (all fingers down)
    if (thumb_tip.y < hand_landmarks.landmark[mpHands.HandLandmark.THUMB_IP].y and
            index_tip.y > hand_landmarks.landmark[mpHands.HandLandmark.INDEX_FINGER_DIP].y and
            middle_tip.y > hand_landmarks.landmark[mpHands.HandLandmark.MIDDLE_FINGER_DIP].y and
            ring_tip.y > hand_landmarks.landmark[mpHands.HandLandmark.RING_FINGER_DIP].y and
            pinky_tip.y > hand_landmarks.landmark[mpHands.HandLandmark.PINKY_DIP].y):
        return "Fist"

    return "Unknown Gesture"


while True:
    # Read video frame by frame
    success, img = cap.read()
    if not success:
        break

    # Flip the image (frame)
    img = cv2.flip(img, 1)

    # Convert BGR image to RGB image
    imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

    # Process the RGB image
    results = hands.process(imgRGB)

    # If hands are present in image (frame)
    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            mpDraw.draw_landmarks(img, hand_landmarks, mpHands.HAND_CONNECTIONS)
            gesture = classify_gesture(hand_landmarks)
            cv2.putText(img, gesture, (10, 70), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)

    # Display Video and when 'q' is entered, destroy the window
    cv2.imshow('Image', img)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release the video capture and destroy all OpenCV windows
cap.release()
cv2.destroyAllWindows()
