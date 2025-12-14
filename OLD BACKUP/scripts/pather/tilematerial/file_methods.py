import ast
import numpy as np

def retrieve_matrix(filepath):
    with open(filepath, 'r') as f:
        lines = f.read().split('\n')
        geo = lines[0]
        tiles = lines[1]
    # Extract tile matrix
    matrix = tiles.split('#tlMatrix: ')[1].split(', #defaultMaterial:')[0]
    # Format matrix to be valid
    matrix = matrix.replace('#tp:', '\'#tp:\',').replace('#Data:', '\'#Data:\',').replace('point','\'point').replace(')',')\'')
    return ast.literal_eval(geo), ast.literal_eval(matrix)

def encode_matrix(filepath, material_name='PlaceholderMateralplacer'):
    geo, tiles = retrieve_matrix(filepath)
    data = np.zeros((len(tiles[0]), len(tiles), 3)) # Might need to switch these around
    excluded_tiles = np.zeros(data.shape)
    
    for x in range(len(tiles[0])):
        for y in range(len(tiles)):
            for c in range(3):
                if geo[y][x][c][0] == 1 and material_name in tiles[y][x][c]:
                    data[x,y,abs(c-2)] = 255
                elif material_name in tiles[y][x][c]:
                    excluded_tiles[x,y,abs(c-2)] = 255
    
    return excluded_tiles, data

if __name__ == '__main__':
    import cv2
    # print(retrieve_matrix(r'C:\Niko\Desktop\pather\level\TL_G02.txt'))
    cv2.imwrite('output.png', encode_matrix(r'C:\Niko\Desktop\pather\level\TL_G02.txt')[1])
    cv2.imwrite('excluded.png', encode_matrix(r'C:\Niko\Desktop\pather\level\TL_G02.txt')[0])