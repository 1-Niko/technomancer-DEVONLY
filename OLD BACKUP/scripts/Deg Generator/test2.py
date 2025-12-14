import cv2
import numpy as np

with open('text template.txt', 'r') as f:
    data = f.read()

MAX_LENGTH = -1
for i in data.split('\n'):
    MAX_LENGTH = max(len(i), MAX_LENGTH)

lines = len(data.split('\n'))

blank = np.zeros(((lines * 2) + ((lines-1)*3) , MAX_LENGTH, 4), dtype=int)
blank[:,:,:] = [0,0,0,255]

for idx, i in enumerate(data.split('\n')):
    for cidx, c in enumerate(i):
        if c != ' ':
            blank[idx*5:(idx*5)+2,cidx,:] = 255

cv2.imwrite('debug.png', blank)