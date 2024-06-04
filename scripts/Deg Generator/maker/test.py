import numpy as np
import cv2

a = cv2.imread('a.png')
b = cv2.imread('b.png')

print(a.shape)
print(b.shape)

# for x in range(a.shape[0]):
    # for y in range(a.shape[1]):
        # print(b[x,y,:])
        # if b[x,y,:].tobytes()==b'\xff\x00\x00':
            # print(a[x,y,:])