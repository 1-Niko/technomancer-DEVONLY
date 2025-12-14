import numpy as np
import ast
import cv2

def save_output_image(room_name : str):
    with open(f'World/{room_name.split("_")[0].upper()}/{room_name}.txt', 'r') as f:
    # with open('PatchTest.txt', 'r') as f:
        data = ast.literal_eval(f.read().split('\n')[0])
    
    blank = np.zeros((len(data[0]) * 20,len(data) * 20,3), dtype=int)
    
    data_0 = [data[x][y][0][0] for y in range(len(data[0])) for x in range(len(data))]
    data_1 = [data[x][y][0][1] for y in range(len(data[0])) for x in range(len(data))]

    for idx, value_00 in enumerate(data_0):
        value_01 = data_1[idx]
        y = idx // len(data)
        x = idx % len(data)
        
        # print(value_00, value_01)
        
        if value_00 in [0, 7]:
            blank[(y*20):(y*20)+20, (x*20):(x*20)+20, :] = [0, 0, 255]
        if value_00 == 2:
            for x_temp in range(20):
                for y_temp in range(20):
                    if x_temp - y_temp < 1:
                        blank[(y*20)+y_temp, (x*20)+x_temp, :] = 0
                    else:
                        blank[(y*20)+y_temp, (x*20)+x_temp, :] = [0,0,255]
        if value_00 == 3:
            for x_temp in range(20):
                for y_temp in range(20):
                    if x_temp + y_temp > 18:
                        blank[(y*20)+y_temp, (x*20)+x_temp, :] = 0
                    else:
                        blank[(y*20)+y_temp, (x*20)+x_temp, :] = [0,0,255]
        if value_00 == 4:
            for x_temp in range(20):
                for y_temp in range(20):
                    if x_temp + y_temp < 20:
                        blank[(y*20)+y_temp, (x*20)+x_temp, :] = 0
                    else:
                        blank[(y*20)+y_temp, (x*20)+x_temp, :] = [0,0,255]
        if value_00 == 5:
            for x_temp in range(20):
                for y_temp in range(20):
                    if x_temp - y_temp > -1:
                        blank[(y*20)+y_temp, (x*20)+x_temp, :] = 0
                    else:
                        blank[(y*20)+y_temp, (x*20)+x_temp, :] = [0,0,255]
    
    cv2.imwrite('Memory/' + room_name + '.png', blank)

if __name__ == '__main__':
    # save_output_image('TL_T01')
    for i in ['DM_I06']:
        save_output_image(i)