import mediapipe as mp
import cv2
import numpy as np
import socket

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

# Set up socket server

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('localhost', 65432))
server_socket.listen()

print("Waiting for connection...")
conn, addr = server_socket.accept()
print('Connected by', addr)


# Initialize the video capture
cap = cv2.VideoCapture(2)

def softmax(x):
    e_x = np.exp(x - np.max(x))
    return e_x / e_x.sum()

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

    if result.gestures:
        # Extract the logits or confidence scores
        gesture_scores = [gesture[0].score for gesture in result.gestures]

        # Apply softmax to get probabilities
        gesture_probabilities = softmax(gesture_scores)

        # Get the gesture with the highest probability
        top_gesture_index = np.argmax(gesture_probabilities)
        top_gesture_name = result.gestures[top_gesture_index][0].category_name
        top_gesture_prob = gesture_probabilities[top_gesture_index]

        # Display the top gesture and its probability on the frame
        display_text = f"{top_gesture_name}: {top_gesture_prob:.2f}"
        cv2.putText(frame, display_text, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)

        # Communicate to the socket
        conn.sendall(top_gesture_name.encode())

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
