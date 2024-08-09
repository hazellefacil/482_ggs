import time
import socket

gestures = ["reload", "shoot", "block"]
curr = 0

# Set up socket server
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind(('localhost', 65432))
server_socket.listen()

print("Waiting for connection...")
conn, addr = server_socket.accept()
print('Connected by', addr)

# send the gesture
gesture_name = gestures[curr]
conn.sendall(gesture_name.encode())

# Wait for Unity's "ready" signal
data = conn.recv(1024)
if data.decode() == "ready":
    print("Unity is ready. Starting 5-second countdown...")
    
    # Start the 5-second countdown
    for i in range(5, 0, -1):
        print(f"Revealing actions in {i} seconds...")
        time.sleep(1)

    # After countdown, reveal the gesture
    print(f"You chose {gesture_name}")
    
    # Wait to receive and display Unity's move
    data = conn.recv(1024)
    computer_move = data.decode()
    print(f"Computer chose {computer_move}")

# Close the connection
conn.close()
server_socket.close()
