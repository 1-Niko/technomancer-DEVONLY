import ast
import numpy as np
import cv2

region_name = 'LF'

keys = [
'LF_ledge',r'Gates/GATE_LF_SB'
]

magnitude = 1

coords = [(-1,-1) for _ in enumerate(keys)]

data = cv2.imread('RoomMap.png')

import re

def effect_retriever(room_name : str, match_group : str):
    PATH = f'{room_name.split("_")[0].upper()}/{room_name}'
    if '/' in room_name:
        PATH = room_name
    with open(f'World/{PATH}.txt', 'r') as f:
        string = f.read().split('#effects: [[#nm:')[1].split(']], #emPos:')[0].split(', [#nm: ')
    
    for i in string:
        if re.findall(r'^\s?\"(.+?)\"', i)[0].lower() == match_group.lower():
            array = np.array(ast.literal_eval(i.split('#mtrx: ')[1].split(', #Options:')[0]))
            return array
    # print(string)

    # a = '[#lastKeys: [#n: 0, #m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #e: 0, #r: 0, #f: 0], #Keys: [#n: 0, #m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #e: 0, #r: 0, #f: 0], #lstMsPs: point(31, 26), #effects: ['
    # b = '], #emPos: point(1, 1), #editEffect: 0, #selectEditEffect: 1, #mode: "chooseEffect", #brushSize: 1]'
    # return a + '' + b

def camera_retriever(room_name : str):
    PATH = f'{room_name.split("_")[0].upper()}/{room_name}'
    if '/' in room_name:
        PATH = room_name
    with open(f'World/{PATH}.txt', 'r') as f:
        string = f.read().split('\n')[6]
    
    def extract_groups(string):
        cameras_regex = r"#cameras:\s\[(.+?)\]"
        quads_regex = r"#quads:\s(\[.+?\]\]\])"

        cameras_match = re.search(cameras_regex, string)
        quads_match = re.search(quads_regex, string)

        cameras_group = cameras_match.group(1) if cameras_match else None
        quads_group = quads_match.group(1) if quads_match else None

        return [ast.literal_eval(i.replace('point','')) for i in cameras_group.split(', point')], ast.literal_eval(quads_group)

    return extract_groups(string)

def find_regex_matches(room_name : str):
    PATH = f'{room_name.split("_")[0].upper()}/{room_name}'
    if '/' in room_name:
        PATH = room_name
    with open(f'World/{PATH}.txt', 'r') as f:
        string = f.read().split('\n')[1].split('#tlMatrix: ')[1].split(', #defaultMaterial:')[0]
        
    matches = string.split('[#tp: "')
    return [([j.startswith(i) for i in ['default', 'material', 'tileHead', 'tileBody']], re.findall(r'\[.+?\]', j), j) for j in matches[1:]]

def save_output_image(room_name : str):
    PATH = f'{room_name.split("_")[0].upper()}/{room_name}'
    if '/' in room_name:
        PATH = room_name
    with open(f'World/{PATH}.txt', 'r') as f:
        return ast.literal_eval(f.read().split('\n')[0])
def prop_list_get(room_name : str):
    PATH = f'{room_name.split("_")[0].upper()}/{room_name}'
    if '/' in room_name:
        PATH = room_name
    with open(f'World/{PATH}.txt', 'r') as f:
        return f.read().split('\n')[8]

def gen_blank_room(size_x, size_y):
    return [[[[1, []], [1, []], [1, []]] for x in range(size_x)] for y in range(size_y)], f'#size: point({size_y}, {size_x})'
def gen_tile_keys(size_x, size_y):
    return [[[['TILEKEY'] for c in range(3)] for x in range(size_x)] for y in range(size_y)]
def map_index_to_coordinates(x_size, y_size, z_size, index):
    z = index // (x_size * y_size)   # Compute the Z coordinate
    remainder = index % (x_size * y_size)
    y = remainder // x_size           # Compute the Y coordinate
    x = remainder % x_size            # Compute the X coordinate
    return (x, y, z)                  # Return the coordinates as a tuple

# print(find_regex_matches(keys[0]))

# DEBUG = effect_retriever('sb_H03', 'BlackGoo')
# print(DEBUG)

# blankGoo = np.zeros((data.shape[0], data.shape[1]), dtype = float)
# blankGoo[:,:] = 100



# cv2.imwrite('DEBUG.png', blankGoo)

# '''
tileKeys = gen_tile_keys(data.shape[0], data.shape[1])
blankroom = gen_blank_room(data.shape[0], data.shape[1])[0]

OVERRIDE = True

for x in range(data.shape[0]):
    for y in range(data.shape[1]):
        for c in range(3):
            # RANGE_TEST = tileKeys[y][x][c]
            if tileKeys[y][x][c][0] == 'TILEKEY':
                tileKeys[y][x][c][0] = '#tp: "default", #Data: 0'
                
        if data[x,y,2] == 0 and data[x,y,1] == 0 and data[x,y,0] != 0:
            tiles = find_regex_matches(keys[data[x,y,0] - 1])
            sizes = save_output_image(keys[data[x,y,0] - 1])
            
            coords[data[x,y,0] - 1] = (y * 20, x * 20)
                    
            indexer = 0
            # print(tiles[0])
            
            for Rx in range(len(sizes)):
                for Ry in range(len(sizes[0])):
                    try:
                        if blankroom[Rx + y][Ry + x] == [[1, []], [1, []], [1, []]]:
                            blankroom[Rx + y][Ry + x] = sizes[Rx][Ry]
                        
                        for c in range(3):
                            # if tileKeys[y][x][c][0] == 'TILEKEY':
                            if tiles[indexer][0][1]:
                                keyed = re.findall(' \".+?\"', tiles[indexer][2])[0]
                                tileKeys[Rx + y][Ry + x][c] = ['#tp: "material", #Data:' + keyed]
                            elif tiles[indexer][0][2]:
                                tileKeys[Rx + y][Ry + x][c] = ['#tp: "tileHead", #Data:' + tiles[indexer][1][0]]
                            elif tiles[indexer][0][3]:
                                # print(tiles[indexer])
                                tileKeys[Rx + y][Ry + x][c] = ['#tp: "tileBody", #Data:' + tiles[indexer][1][0]]
                            indexer += 1
                    except IndexError:
                        indexer += 1
        # try:
        # except IndexError:
            # pass

tilestart = '[#lastKeys: [], #Keys: [], #workLayer: 1, #lstMsPs: point(0, 0), #tlMatrix: '
tileend = ', #defaultMaterial: "Concrete", #toolType: "material", #toolData: "Big Metal", #tmPos: point(1, 1), #tmSavPosL: [], #specialEdit: 0]'
# '''
# with open('testdata.txt', 'w') as f:
    # f.write(str(blankroom) + '\n' + tilestart + str(tileKeys).replace('\'','') + tileend)

# print(coords)

def add_tuple(a, b):
    return (float(round(a[0] + b[0], 4)), float(round(a[1] + b[1], 4)))

cams = [camera_retriever(i) for i in keys]

positions = []
quad_poss = []

def format_number(number):
    return number.split('.')[0] + '.' + number.split('.')[1].ljust(4,'0')

def format_point(tupler : tuple):
    nums = str(tupler)[1:-1].split(', ')
    
    numA = format_number(nums[0])
    numB = format_number(nums[1])
    
    return 'point(' + numA + ', ' + numB + ')'
# '''
def cleanup(x):
    return '[' + x + ']]]'

def extract_prop_groups(string):
    pattern = r'\[(\-?\d+)\,\s\"(.+)\",\spoint(\(\d+\.?\d*\,\s\d+\.?\d*\))\,\s(\[.+?\])\,\s\[\#settings:\s(\[.+?\])\,?\s?\#?p?o?i?n?t?s?:?\s?(\[?.*?\])'
    matches = re.findall(pattern, string)
    return list(matches[0])

def prop_formatter(prop_list, offset = None):
    list_3 = ast.literal_eval(', '.join([i.replace('point','') for i in prop_list[3].split(', point')]))

    list_3 = str([format_point(add_tuple(i, offset)) for i in list_3]).replace('\'','')
    
    # print(list_3)

    # print(prop_list[0])
    if prop_list[-1] == ']':
        output = f'[{prop_list[0]}, "{prop_list[1]}", point{prop_list[2]}, {list_3}, [#settings: {prop_list[4]}]]'
    else:
        list_5 = ast.literal_eval(', '.join([i.replace('point','') for i in prop_list[5].split(', point')]))
        list_5 = str([format_point(add_tuple(i, offset)) for i in list_5]).replace('\'','')
        output = f'[{prop_list[0]}, "{prop_list[1]}", point{prop_list[2]}, {list_3}, [#settings: {prop_list[4]}, #points: {list_5}]]'
    return output

def correct_offset(a):
    return (a[0] / 20) * 16, (a[1] / 20) * 16

props = []
for idx, i in enumerate(keys):
    data = prop_list_get(i)
    data = [cleanup(j) for j in data[11:].split(']]]], #lastKeys: ')[0].split(']]], [')]
    for j in data:
        
        # prop_formatter(extract_prop_groups(j), coords[idx])
        try:
            props.append(prop_formatter(extract_prop_groups(j), correct_offset(coords[idx])))
        except IndexError:
            pass

propdat = f"[#props: [{', '.join(props)}], #lastKeys: [#w: 0, #a: 0, #s: 0, #d: 0, #L: 0, #n: 0, #m1: 0, #m2: 0, #c: 0, #z: 0], #Keys: [#w: 0, #a: 0, #s: 0, #d: 0, #L: 0, #n: 0, #m1: 0, #m2: 0, #c: 0, #z: 0], #workLayer: 1, #lstMsPs: point(0, 0), #pmPos: point(1, 1), #pmSavPosL: [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1], #propRotation: 148, #propStretchX: 1, #propStretchY: 1, #propFlipX: 1, #propFlipY: 1, #depth: 0, #color: 0]"

for idx, i in enumerate(cams):
    # print(i[0])
    positions = positions + [format_point(add_tuple(j, coords[idx])) for j in i[0]]
    quad_poss = quad_poss + [[[format_number(str(float(l))) for l in k] for k in j] for j in i[1]]
# '''

# print(format_point((1.0,0.0)))

# print(str(positions).replace('\'',''))
# print()
# print(str(quad_poss).replace('\'',''))

camera_count = len(positions)

def convert_int_to_string(num):
    if num <= 0:
        return ""

    result = ""
    while num > 0:
        remainder = (num - 1) % 26
        result = chr(65 + remainder) + result
        num = (num - 1) // 26

    return result

# print(convert_int_to_string(len(positions)))

def shadow_eq(N):
    return 20 * N + 300

def format_integer(num):
    return f'{num:02}'

_3_4_5_PLACEHOLDER = '''
[#lastKeys: [], #Keys: [], #lstMsPs: point(0, 0), #effects: [], #emPos: point(1, 1), #editEffect: 0, #selectEditEffect: 0, #mode: "createNew", #brushSize: 5]
[#pos: point(520, 400), #rot: 0, #sz: point(50, 70), #col: 1, #Keys: 0, #lastKeys: 0, #lastTm: 0, #lightAngle: 180, #flatness: 1, #lightRect: rect(1000, 1000, -1000, -1000), #paintShape: "pxl"]
[#timeLimit: 4800, #defaultTerrain: 1, #maxFlies: 10, #flySpawnRate: 50, #lizards: [], #ambientSounds: [], #music: "NONE", #tags: [], #lightType: "Static", #waterDrips: 1, #lightRect: rect(0, 0, 1040, 800), #Matrix: []]
'''

positions = str(positions).replace('\'','')
quad_poss = str(quad_poss).replace('\'','')

LINE_8 = '[#waterLevel: -1, #waterInFront: 1, #waveLength: 60, #waveAmplitude: 5, #waveSpeed: 10]'

cameras = f"[#cameras: {positions}, #selectedCamera: 0, #quads: {quad_poss}, #Keys: [#n: 0, #d: 0, #e: 0, #p: 0], #lastKeys: [#n: 0, #d: 0, #e: 0, #p: 0]]"

roomsize = cv2.imread('RoomMap.png')

level_size = f'''[#mouse: 1, #lastMouse: 1, #mouseClick: 0, #pal: 1, #pals: [[#detCol: color( 255, 0, 0 )]],
#eCol1: 1, #eCol2: 2, #totEcols: 5, #tileSeed: 354, #colGlows: [0, 0], #size: point({roomsize.shape[1]}, {roomsize.shape[0]}), #extraTiles: [12, 3, 12, 5], #light: 1]'''.replace('\n', ' ')

# print(level_size)

data = cv2.imread('RoomMap.png')
shadow_map = np.zeros((shadow_eq(data.shape[0]), shadow_eq(data.shape[1]), 3), dtype = int)
shadow_map[:,:,:] = 255
cv2.imwrite(f'{region_name}_{convert_int_to_string(camera_count)}{format_integer(magnitude)}.png', shadow_map)

with open(f'{region_name}_{convert_int_to_string(camera_count)}{format_integer(magnitude)}.txt', 'w') as f:
    f.write(str(blankroom) + '\n' + tilestart + str(tileKeys).replace('\'','') + tileend + _3_4_5_PLACEHOLDER + level_size + '\n' + cameras + (f'\n{LINE_8}\n') + propdat + '\n')

# print(camera_retriever('sb_H03'))