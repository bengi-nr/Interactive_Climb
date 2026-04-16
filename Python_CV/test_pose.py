import cv2
import mediapipe as mp
import socket
import time

UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

mp_hands = mp.solutions.hands
hands = mp_hands.Hands(
    static_image_mode=False,
    max_num_hands=2,           # ← İki el takibi
    min_detection_confidence=0.6,
    min_tracking_confidence=0.5
)
mp_draw = mp.solutions.drawing_utils

cap = cv2.VideoCapture(1)
last_send = time.time()
SEND_INTERVAL = 0.033  # ~30 FPS ile gönder

while cap.isOpened():
    success, image = cap.read()
    if not success:
        break  # ← continue yerine break

    image = cv2.cvtColor(cv2.flip(image, 1), cv2.COLOR_BGR2RGB)
    results = hands.process(image)
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

    if results.multi_hand_landmarks and results.multi_handedness:
        for hand_landmarks, handedness in zip(
            results.multi_hand_landmarks, results.multi_handedness
        ):
            lm9 = hand_landmarks.landmark[9]
            x, y = lm9.x, lm9.y
            hand_label = handedness.classification[0].label  # "Left" / "Right"

            # FPS sınırla
            now = time.time()
            if now - last_send >= SEND_INTERVAL:
                data = f"{hand_label},{x:.4f},{y:.4f}"  # ← El adı da eklendi
                print(data)
                sock.sendto(data.encode(), (UDP_IP, UDP_PORT))
                last_send = now

            mp_draw.draw_landmarks(image, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    cv2.imshow('Tırmanış Takip Sistemi', image)
    if cv2.waitKey(1) & 0xFF == 27:
        break

cap.release()
cv2.destroyAllWindows()