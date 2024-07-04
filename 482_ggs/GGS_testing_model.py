import mediapipe as mp
import cv2

# Path to your custom gesture recognizer model
model_path = 'gesture_recognizer.task'

# Initialize MediaPipe Gesture Recognizer with the custom model
BaseOptions = mp.tasks.BaseOptions
GestureRecognizer = mp.tasks.vision.GestureRecognizer
GestureRecognizerOptions = mp.tasks.vision.GestureRecognizerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

options = GestureRecognizerOptions(
    base_options=BaseOptions(model_asset_path=model_path),
    running_mode=VisionRunningMode.IMAGE
)

recognizer = GestureRecognizer.create_from_options(options)

# Initialize the video capture
cap = cv2.VideoCapture(2)

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break
    
    # Convert the BGR image to RGB
    image = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    # Create an image input for the gesture recognizer
    mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=image)
    
    # Perform gesture recognition
    result = recognizer.recognize(mp_image)

    for i, gesture in enumerate(result.gestures):
        # Get the top gesture from the recognition result
        print("Top Gesture Result: ", gesture[0].category_name)
        gesture_name = gesture[0].category_name
        cv2.putText(frame, gesture_name, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)

    if result.hand_landmarks:
        # Obtain hand landmarks from MediaPipe
        hand_landmarks = result.hand_landmarks
        #print("Hand Landmarks: " + str(hand_landmarks))

        # Obtain hand connections from MediaPipe
        mp_hands_connections = mp.solutions.hands.HAND_CONNECTIONS
        #print("Hand Connections: " + str(mp_hands_connections))
        

    # Display the frame
    cv2.imshow('Gesture Recognition', frame)
    
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()