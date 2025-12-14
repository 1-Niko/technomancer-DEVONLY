import numpy as np
import cv2
import random


def draw_title(length, dir, large_text = False, tall = False, labels = False, offset = 1, texts = None):
    if not labels:
        if large_text:
            data = cv2.imread('12x12text.png')
        else:
            data = cv2.imread('lurText.png')
    else:
        data = cv2.imread('lurLabels.png')

    if tall:
        blank = np.zeros(((length * (6 * (int(large_text) + 1))) + ((length - 1) * offset), 6 * (int(large_text) + 1), 4), dtype=int)
    else:
        blank = np.zeros((6 * (int(large_text) + 1), (length * (6 * (int(large_text) + 1))) + ((length - 1) * offset), 4), dtype=int)
    blank[:,:,0:3] = 0
    for i in range(length):
        if texts != None:
            choice = texts[i]
        else:
            if labels:
                choice = random.randrange(0,18)
            else:
                choice = random.randrange(0,14)
        #blank[:, i*7:(i*7)+6, 0:3] = data[:, choice * 6 : (choice * 6) + 6, :]
        for x in range(6 * (int(large_text) + 1)):
            for y in range(6 * (int(large_text) + 1)):
                if tall:
                    if abs(255 - data[y, (choice * (6 * (int(large_text) + 1))) + x, 0]) == 255:
                        blank[(i*((6 * (int(large_text) + 1)) + offset)) + y, x, :] = 255
                    else:
                        blank[(i*((6 * (int(large_text) + 1)) + offset)) + y, x, :] = 0
                else:
                    if abs(255 - data[x, (choice * (6 * (int(large_text) + 1))) + y, 0]) == 255:
                        blank[x, (i*((6 * (int(large_text) + 1)) + offset)) + y, :] = 255
                    else:
                        blank[x, (i*((6 * (int(large_text) + 1)) + offset)) + y, :] = 0
    
    cv2.imwrite(f'{dir}.png', blank)

# for i in range(50):
    # values = list(range(18))
    # random.shuffle(values)
    # print(values)
    # C:/Users/huntt/OneDrive/Desktop/bigarc/cagesticker/cagelabels/CageLabel_{i}
draw_title(random.randrange(8,16), f'1', large_text=True, tall=True, labels=False, offset=2, texts=None) #[4, 11, 2, 0])
draw_title(random.randrange(8,16), f'2', large_text=True, tall=True, labels=False, offset=2, texts=None) #[4, 11, 2, 0])
draw_title(random.randrange(8,16), f'3', large_text=True, tall=True, labels=False, offset=2, texts=None) #[4, 11, 2, 0])

# for idx, i in enumerate([2] * 6):
    # draw_title(i, f'data/{idx}', True, True)