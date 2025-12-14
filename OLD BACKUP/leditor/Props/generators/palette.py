import numpy as np
import cv2

data = np.zeros((6,30),dtype=np.uint8)

index = 1
for y in range(6):
    for x in range(30):
        data[y,x] = index
        index += 1

cv2.imwrite('debug.png', data)