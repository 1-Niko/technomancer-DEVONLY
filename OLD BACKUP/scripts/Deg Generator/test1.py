from gener import lengen
import numpy as np
import cv2
import random

def generate_dialogue_string(max_len):
    length,st = 0, []
    while True:
        temp = lengen()
        st.append(temp)
        length += temp + 2
        if length > max_len:
            return st

def draw_dialogue_string(string_sequence, colour, single_size = False):
    if single_size:
        blank = np.zeros((1,((len(string_sequence) - 1) * 2) + sum(string_sequence),3), dtype=int)
    else:
        blank = np.zeros((2,((len(string_sequence) - 1) * 2) + sum(string_sequence),3), dtype=int)
    index = 0
    idd= 0
    if colour == None:
        a = 255
    else:
        a = colour
    while True:
        blank[:, index:index+string_sequence[idd], :] = a
        index += string_sequence[idd] + 2
        idd += 1
        if index >= (((len(string_sequence) - 1) * 2) + sum(string_sequence)):
            return blank

def draw_text_box(maximum_length, lines, colour = None, random_lengths = True, single_size = False):
    if single_size:
        blank = np.zeros(((lines) + ((lines-1)*3) , maximum_length, 3), dtype=int)
    else:
        blank = np.zeros(((lines * 2) + ((lines-1)*3) , maximum_length, 3), dtype=int)
    for idx, i in enumerate(range(lines)):
        if random_lengths:
            lin_length = random.randrange(maximum_length//2, maximum_length)
        else:
            lin_length = maximum_length
        while True:
            try:
                if single_size:
                    g = draw_dialogue_string(generate_dialogue_string(lin_length), colour, True)
                    blank[idx * 3: (idx * 3) + 2, :g.shape[1], :] = g
                else:
                    g = draw_dialogue_string(generate_dialogue_string(lin_length), colour)
                    blank[idx * 5: (idx * 5) + 2, :g.shape[1], :] = g
                break
            except ValueError:
                pass
    return blank

#cv2.imwrite(f'a.png', draw_text_box(140, 27, 255))
#cv2.imwrite(f'b.png', draw_text_box(140//2, 1, 255, False))

for i in range(4):
    cv2.imwrite(f'C:/Users/huntt/Desktop/MessageMaker/text/{i}.png', draw_text_box(80, 4, 255))
'''
import cv2
import numpy as np
import os

# Set the directory path
directory = "C:/Users/huntt/Desktop/MessageMaker/text"

# Iterate through each file in the directory
for filename in os.listdir(directory):
    # Check if the file is an image
    if filename.endswith(".jpg") or filename.endswith(".png") or filename.endswith(".jpeg"):
        # Open the image using OpenCV
        image = cv2.imread(os.path.join(directory, filename))

        # Get the new size
        height, width = image.shape[:2]
        new_width = width // 2
        new_height = height // 2

        # Resize the image with no smoothing using NumPy and OpenCV
        resized_image = cv2.resize(image, (new_width, new_height), interpolation=cv2.INTER_NEAREST)

        # Save the resized image
        cv2.imwrite(os.path.join(directory, filename), resized_image)
'''
'''
# print(draw_dialogue_string(generate_dialogue_string(128)))
for i, j in enumerate([255] * 10 + [255]): # - 105
    cv2.imwrite(f'data/{i}.png', draw_text_box(140, random.randrange(2,8), j))

data = cv2.imread('placer/key.png')

for i in range(11):
    temp = cv2.imread(f'data/{i}.png')
    # print(temp.shape)
    # print((39,39+temp.shape[1]), (((38*i)+2),((38*i)+2)+temp.shape[0]))
    data[((38*i)+2):((38*i)+2)+temp.shape[0],39:39+temp.shape[1],:] = temp

cv2.imwrite('placer/output.png', data)'''