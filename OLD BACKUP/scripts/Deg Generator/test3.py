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

cv2.imwrite(f'C:/Niko/Desktop/MessageMaker/data/sign/text.png', draw_text_box(240, 3, 255, True))
    