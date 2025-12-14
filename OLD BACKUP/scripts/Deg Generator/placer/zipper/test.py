import numpy as np
import cv2

colour = cv2.imread('col.png')
alpha = cv2.imread('alpha.png')

blank = np.zeros((colour.shape[0], colour.shape[1], 4),dtype=int)
for x in range(colour.shape[0]):
    for y in range(colour.shape[1]):
        for i in range(3):
            blank[x,y,i] = colour[x,y,i]
        blank[x,y,3] = alpha[x,y,0]

cv2.imwrite('data.png', blank)