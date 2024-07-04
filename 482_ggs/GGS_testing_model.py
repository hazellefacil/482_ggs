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

gesture_recognizer = GestureRecognizer.create_from_options(options)

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
    result = gesture_recognizer.recognize(mp_image)
    print(result.gestures)

    # Draw the recognized gesture on the frame
    #if result.gestures:
        #for gesture in result.gestures:
            #gesture_name = gesture.
            #cv2.putText(frame, gesture_name, (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2, cv2.LINE_AA)

    # Display the frame
    cv2.imshow('Gesture Recognition', frame)
    
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()