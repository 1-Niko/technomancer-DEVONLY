import numpy as np
import cv2

data = cv2.imread('channel_colours.png')

print(data.shape)

blank = np.zeros((data.shape[0], data.shape[1]//2, 4), dtype=int)
for x in range(data.shape[0]):
    for y in range(data.shape[1]):
        blank[x,y,2] = data[x,y,0] # Red 
        blank[x,y,1] = data[x,y+487,1] # Green
        blank[x,y,0] = data[x,y+995,2] # Blue
        blank[x,y,3] = data[x,y+1492,0] # Alpha

cv2.imwrite('data.png', blank)