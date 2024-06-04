import numpy as np
import ast
import cv2

def save_output_image(room_name : str):
    PATH = f'{room_name.split("_")[0].upper()}/{room_name}'
    if '/' in room_name:
        PATH = room_name
    with open(f'World/{PATH}.txt', 'r') as f:
    # with open('PatchTest.txt', 'r') as f:
        data = ast.literal_eval(f.read().split('\n')[0])
    
    blank = np.zeros((len(data[0]),len(data),3), dtype=int)
    
    data_0 = [data[x][y][0][0] for y in range(blank.shape[0]) for x in range(blank.shape[1])]
    data_1 = [data[x][y][0][1] for y in range(blank.shape[0]) for x in range(blank.shape[1])]

    for idx, value_00 in enumerate(data_0):
        value_01 = data_1[idx]
        y = idx // blank.shape[1]
        x = idx % blank.shape[1]
        
        if value_00 in [0, 7]:
            blank[y, x, :] = [0, 0, 255]
        elif value_00 in [2, 3, 4, 5, 6]:
            blank[y, x, :] = [0, 0, 153]
        if 11 in value_01 and value_00 == 1:
            blank[y, x, :] = [0, 0, 153]
        if (2 in value_01 or 1 in value_01) and value_00 == 0:
            blank[y, x, :] = [0, 0, 153]
    
    cv2.imwrite('Memory/' + [room_name if '/' not in room_name else room_name.split('/')[-1]][0] + '.png', blank)

if __name__ == '__main__':
    # save_output_image('TL_T01')
    for i in ['TL_P01']:
        save_output_image(i)