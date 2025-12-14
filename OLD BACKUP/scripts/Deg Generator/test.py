import numpy as np
import cv2

data = cv2.imread('builder.png')

List = []

'''KEY
[148   1 255] b'\x94\x01\xff' start counter
[227 255   1] b'\xe3\xff\x01' end counter
'''

def count_from_point(data, point):
    count = 0
    while True:
        temp = data[point[0], point[1] + (count + 1), :]
        if temp.tobytes() == b'\x00\x00\x00':
            return count
        count += 1


g = []
t = np.where(np.all(data == [148, 1, 255], axis=-1))
for i in zip(t[0], t[1]):
    g.append(count_from_point(data, i))
    # data[i[0],i[1],:] = [count_from_point(data, i), 0, 255]\

print(g)