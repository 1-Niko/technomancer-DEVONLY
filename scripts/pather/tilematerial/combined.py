import ast
import numpy as np
import cv2
import random
from functools import cache

level_name = 'TL_C14.txt'

placement_attempts = 1000
# placement_attempts = 550

# material_name = 'Random Machines'
material_name = 'PlaceholderMateralplacer'
# material_name = 'Chaotic Stone'

# ' - RandomDepth'
shapes_with_colors = [
    {'size': (1, 1), 'name': 'Small Stone', 'weight': 1},  # 
    {'size': (2, 1), 'name': 'Small Wide Stone', 'weight': 30},  # 
    {'size': (1, 2), 'name': 'Small Tall Stone', 'weight': 30},  # 
    {'size': (1, 3), 'name': 'Narrow Tall Stone', 'weight': 45},  # 
    {'size': (3, 1), 'name': 'Narrow Wide Stone', 'weight': 45},  # 
    {'size': (2, 2), 'name': 'Square Stone', 'weight': 60},  # 
    {'size': (2, 3), 'name': 'Tall Stone', 'weight': 90},  # 
    {'size': (3, 2), 'name': 'Wide Stone', 'weight': 90},  # 
    {'size': (3, 3), 'name': 'Big Stone', 'weight': 120},  # 
    {'size': (3, 3), 'name': 'Big Stone Marked', 'weight': 120},  # 
]
'''shapes_with_colors = [
    {'size': (1, 1), 'name': 'Small Machine A', 'weight': 1},  # Tank Holder
    {'size': (1, 1), 'name': 'Small Machine B', 'weight': 1},  # Tank Holder
    {'size': (1, 1), 'name': 'Small Machine C', 'weight': 1},  # Tank Holder
    {'size': (1, 1), 'name': 'Small Machine D', 'weight': 1},  # Tank Holder
    {'size': (1, 1), 'name': 'Small Machine E', 'weight': 1},  # Tank Holder
    {'size': (1, 1), 'name': 'Small Machine F', 'weight': 1},  # Tank Holder
    {'size': (1, 1), 'name': 'Small Machine G', 'weight': 1},  # Tank Holder
    {'size': (2, 2), 'name': 'Metal Holes', 'weight': 1, 'point': ()},  # Metal Holes
    # # {'size': (3, 3), 'name': 'Dyson Fan', 'weight': 1},  # Dyson Fan
    # # {'size': (3, 3), 'name': 'Big Fan', 'weight': 1},  # Big Fan
    {'size': (4, 2), 'name': 'machine box A', 'weight': 1},  # machine box A
    {'size': (3, 3), 'name': 'machine box B', 'weight': 1},  # machine box B
    {'size': (2, 2), 'name': 'machine box C_E', 'weight': 1},  # machine box C_E
    {'size': (2, 2), 'name': 'machine box C_W', 'weight': 1},  # machine box C_W
    {'size': (2, 2), 'name': 'machine box C_Sym', 'weight': 1},  # machine box C_Sym
    {'size': (1, 1), 'name': 'Tank Holder', 'weight': 1},  # Tank Holder
    {'size': (3, 3), 'name': 'Machine Box D', 'weight': 1},  # Machine Box D
    {'size': (1, 3), 'name': 'Machine Box E L', 'weight': 1},  # Machine Box E L
    {'size': (1, 3), 'name': 'Machine Box E R', 'weight': 1},  # Machine Box E R
    {'size': (2, 1), 'name': 'Mud Elevator', 'weight': 1},  # Mud Elevator
    {'size': (2, 1), 'name': 'Elevator Track', 'weight': 1},  # Elevator Track
    {'size': (3, 3), 'name': 'Hub Machine', 'weight': 1},  # Hub Machine
    {'size': (1, 2), 'name': 'Feather Box - W', 'weight': 1},  # Feather Box - W
    {'size': (1, 2), 'name': 'Feather Box - E', 'weight': 1},  # Feather Box - E
    {'size': (2, 2), 'name': 'Compressor L', 'weight': 1},  # Compressor L
    {'size': (2, 2), 'name': 'Compressor R', 'weight': 1},  # Compressor R
    {'size': (1, 2), 'name': 'Compressor Segment', 'weight': 1},  # Compressor Segment
    {'size': (4, 4), 'name': 'Giant Screw', 'weight': 1},  # Giant Screw
    {'size': (2, 2), 'name': 'Pipe Box R', 'weight': 1},  # Pipe Box R
    {'size': (2, 2), 'name': 'Pipe Box L', 'weight': 1},  # Pipe Box L
    {'size': (1, 1), 'name': 'Door Holder R', 'weight': 1},  # Door Holder R
    {'size': (1, 1), 'name': 'Door Holder L', 'weight': 1},  # Door Holder L
    {'size': (4, 1), 'name': 'Piston Top', 'weight': 1},  # Piston Top
    {'size': (4, 1), 'name': 'Piston Segment Empty', 'weight': 1},  # Piston Segment Empty
    {'size': (4, 5), 'name': 'Piston Head', 'weight': 1},  # Piston Head
    {'size': (4, 1), 'name': 'Piston Segment Filled', 'weight': 1},  # Piston Segment Filled
    {'size': (4, 1), 'name': 'Piston Bottom', 'weight': 1},  # Piston Bottom
    {'size': (4, 4), 'name': 'Ventilation Box', 'weight': 1},  # Ventilation Box
    # # {'size': (4, 4), 'name': 'Ventilation Box Empty', 'weight': 1},  # Ventilation Box Empty
    {'size': (1, 1), 'name': 'Drill A', 'weight': 1},  # Drill A
    {'size': (1, 1), 'name': 'Drill B', 'weight': 1},  # Drill B
    {'size': (5, 1), 'name': 'Drill Shell A', 'weight': 1},  # Drill Shell A
    {'size': (5, 2), 'name': 'Drill Shell B', 'weight': 1},  # Drill Shell B
    {'size': (3, 1), 'name': 'Drill Rim', 'weight': 1},  # Drill Rim
    {'size': (1, 1), 'name': 'Small Metal', 'weight': 1},  # Small Metal
    {'size': (2, 1), 'name': 'Metal Floor', 'weight': 1},  # Metal Floor
    {'size': (2, 2), 'name': 'Square Metal', 'weight': 1},  # Square Metal
    {'size': (3, 3), 'name': 'Big Metal', 'weight': 1},  # Big Metal
    {'size': (3, 3), 'name': 'Big Metal Marked', 'weight': 1},  # Big Metal Marked
    # # {'size': (, ), 'color': (, , )}, # 
]'''

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
                    data[x,y,abs(c-2)] = 127
                elif material_name in tiles[y][x][c]:
                    excluded_tiles[x,y,abs(c-2)] = 127
    
    return excluded_tiles, data

def decode_matrix(matrix):
    return str(matrix).replace('"', "'").replace('\'#tp:\',', '#tp:').replace('\'#Data:\',', '#Data:').replace('\'point','point').replace(')\'',')').replace("'", '"')

def get_random_color():
    """Generate a random color in (B, G, R) format."""
    return tuple(random.randint(0, 255) for _ in range(3))

def fill_rectangle(image, top_left, width, height, color):
    """Fill a rectangle in the image with the specified color."""
    x, y = top_left
    image[y:y + height, x:x + width] = color

@cache
def get_tile_pointer(filepath, tilename):
    with open(filepath, 'r') as f:
        groups = f.read().split('\n\n-')
    
    for gidx, group in enumerate(groups):
        if f'"{tilename}"' in group:
            tiles = group.split('\n')
            for idx, tile in enumerate(tiles[1:]):
                if f'"{tilename}"' in tile:
                    return gidx + 8, idx + 1
    
    return 1,1

def main(file_name, shapes_with_colors, channel, seed=12345):
    random.seed(seed)
    
    # Generate colors for shapes without specified colors
    unique_shapes = {}
    for shape in shapes_with_colors:
        size = shape['size']
        if shape.get('color', None) is None:
            shape['color'] = get_random_color()
        if size not in unique_shapes:
            unique_shapes[size] = []
        unique_shapes[size].append(shape['color'])

    img = cv2.imread(file_name)# - cv2.imread('excluded.png')
    img_red_channel = img[:, :, channel]  # Extract the red channel

    # Convert to color image 
    img_color = cv2.cvtColor(img_red_channel, cv2.COLOR_GRAY2BGR)
    
    height, width = img_red_channel.shape
    covered_mask = np.zeros_like(img_red_channel, dtype=np.uint8)
    
    def can_place_rectangle(top_left, rect_width, rect_height):
        """Check if a rectangle can be placed without overlap and within white area."""
        x, y = top_left
        if x + rect_width > width or y + rect_height > height:
            return False
        if np.any(covered_mask[y:y + rect_height, x:x + rect_width] == 1):
            return False
        if np.any(img_red_channel[y:y + rect_height, x:x + rect_width] == 0):
            return False
        return True

    positions = {}
    all_shapes = [(shape['size'], shape['name'], shape['color'], shape['weight']) for shape in shapes_with_colors]
    remaining_shapes = all_shapes.copy()
    
    while all_shapes:
        size, name, color, weight = random.choices(all_shapes, weights=[w for _, _, _, w in all_shapes], k=1)[0]
        shape_width, shape_height = size

        attempts = placement_attempts  # Number of attempts to place the shape
        placed = False

        for _ in range(attempts):
            x = random.randint(0, width - shape_width)
            y = random.randint(0, height - shape_height)

            if can_place_rectangle((x, y), shape_width, shape_height):
                fill_rectangle(img_color, (x, y), shape_width, shape_height, color)
                covered_mask[y:y + shape_height, x:x + shape_width] = 1
                positions[(x, y)] = name
                placed = True
                break

        if not placed:
            for dx in range(shape_width):
                for dy in range(shape_height):
                    if x + dx < width and y + dy < height:
                        positions[(x + dx, y + dy)] = None
            all_shapes.remove((size, name, color, weight))

    # Second pass to fill remaining gaps
    for size, name, color, weight in remaining_shapes:
        shape_width, shape_height = size

        for y in range(0, height - shape_height + 1):
            for x in range(0, width - shape_width + 1):
                if can_place_rectangle((x, y), shape_width, shape_height):
                    fill_rectangle(img_color, (x, y), shape_width, shape_height, color)
                    covered_mask[y:y + shape_height, x:x + shape_width] = 1
                    positions[(x, y)] = name

    return positions

def get_size_from_name(table, name):
    for i in table:
        if i['name'] == name:
            return i['size']
    raise IndexError(f"Index {name} does not exist in the table!")

def ALL_SOLID_GEO(geo_map, min_x, max_x, min_y, max_y, layer):
    for x in range(min_x,max_x):
        for y in range(min_y,max_y):
            if geo[x][y][layer][0] != 1:
                return False
    return True

if __name__ == '__main__':
    import cv2
    # print(retrieve_matrix(r'C:\Niko\Desktop\pather\level\MATERIAL TESTER.txt'))
    cv2.imwrite('output.png', encode_matrix(fr'C:\Niko\Desktop\pather\level\{level_name}', material_name)[1])
    cv2.imwrite('excluded.png', encode_matrix(fr'C:\Niko\Desktop\pather\level\{level_name}', material_name)[0])
    
    data = cv2.imread('output.png') + cv2.imread('excluded.png')
    
    SEED = 12345
    
    for channel in range(3):
    
        positions = main('output.png', shapes_with_colors, channel, seed=SEED)
        
        # data[:,:,0:2] = 0
        
        for (x, y), name in positions.items():
            if name is not None:
                found = next(shape for shape in shapes_with_colors if shape['name'] == name)
                shape_width, shape_height = found['size']
                for dx in range(shape_width):
                    for dy in range(shape_height):
                        if x + dx < data.shape[1] and y + dy < data.shape[0]:
                            data[y + dy, x + dx, channel] = (7 * shape_width) + shape_height + 56
                            data[y, x, channel] = 255
    
    cv2.imwrite('output.png', data)
    
    default_material = 'Random Machines'
    
    init_path = 'Init.txt'
    
    geo, tiles = retrieve_matrix(fr'C:\Niko\Desktop\pather\level\{level_name}')
    
    for i in range(3):
        layer = main('output.png', shapes_with_colors, i, seed=SEED)
        for (y, x), name in layer.items():
            if name is not None:
                page, position = get_tile_pointer(init_path, name)
                n = abs(i-2)
                
                size = get_size_from_name(shapes_with_colors, name)
                
                if (ALL_SOLID_GEO(geo, y, y + size[0], x, x + size[1], n)) and (y + size[0] - 1 < data.shape[1]) and (x + size[1] - 1 < data.shape[0]):
                    if get_size_from_name(shapes_with_colors, name) == (1,1):
                        tiles[y][x][n][1] = "tileHead"
                        tiles[y][x][n][3] = [f'point({page}, {position})', name]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (2,1):
                        tiles[y][x][n][1] = "tileHead"
                        tiles[y][x][n][3] = [f'point({page}, {position})', name]
                        
                        tiles[y + 1][x][n][1] = "tileBody"
                        tiles[y + 1][x][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (1,2):
                        tiles[y][x][n][1] = "tileHead"
                        tiles[y][x][n][3] = [f'point({page}, {position})', name]
                        
                        tiles[y][x + 1][n][1] = "tileBody"
                        tiles[y][x + 1][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (2,2):
                        tiles[y][x][n][1] = "tileHead"
                        tiles[y][x][n][3] = [f'point({page}, {position})', name]
                        
                        tiles[y + 1][x][n][1] = "tileBody"
                        tiles[y + 1][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y][x + 1][n][1] = "tileBody"
                        tiles[y][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 1][n][1] = "tileBody"
                        tiles[y + 1][x + 1][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (1,3):
                        tiles[y][x + 1][n][1] = "tileHead"
                        tiles[y][x + 1][n][3] = [f'point({page}, {position})', name]
                        
                        tiles[y][x][n][1] = "tileBody"
                        tiles[y][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y][x + 2][n][1] = "tileBody"
                        tiles[y][x + 2][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (3,2):
                        tiles[y + 0][x + 0][n][1] = "tileBody"
                        tiles[y + 0][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 0][n][1] = "tileHead"
                        tiles[y + 1][x + 0][n][3] = [f'point({page}, {position})', name]
                        tiles[y + 2][x + 0][n][1] = "tileBody"
                        tiles[y + 2][x + 0][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 0][x + 1][n][1] = "tileBody"
                        tiles[y + 0][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 1][n][1] = "tileBody"
                        tiles[y + 1][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 1][n][1] = "tileBody"
                        tiles[y + 2][x + 1][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (2,3):
                        tiles[y + 0][x + 0][n][1] = "tileBody"
                        tiles[y + 0][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 0][x + 1][n][1] = "tileHead"
                        tiles[y + 0][x + 1][n][3] = [f'point({page}, {position})', name]
                        tiles[y + 0][x + 2][n][1] = "tileBody"
                        tiles[y + 0][x + 2][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 1][x + 0][n][1] = "tileBody"
                        tiles[y + 1][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 1][n][1] = "tileBody"
                        tiles[y + 1][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 2][n][1] = "tileBody"
                        tiles[y + 1][x + 2][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (3,1):
                        tiles[y + 1][x][n][1] = "tileHead"
                        tiles[y + 1][x][n][3] = [f'point({page}, {position})', name]
                        
                        tiles[y][x][n][1] = "tileBody"
                        tiles[y][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x][n][1] = "tileBody"
                        tiles[y + 2][x][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (3,3):
                        tiles[y + 1][x + 1][n][1] = "tileHead"
                        tiles[y + 1][x + 1][n][3] = [f'point({page}, {position})', name]
                        
                        tiles[y][x][n][1] = "tileBody"
                        tiles[y][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y][x + 1][n][1] = "tileBody"
                        tiles[y][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y][x + 2][n][1] = "tileBody"
                        tiles[y][x + 2][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x][n][1] = "tileBody"
                        tiles[y + 1][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 2][n][1] = "tileBody"
                        tiles[y + 1][x + 2][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x][n][1] = "tileBody"
                        tiles[y + 2][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 1][n][1] = "tileBody"
                        tiles[y + 2][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 2][n][1] = "tileBody"
                        tiles[y + 2][x + 2][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (4,2):
                        tiles[y + 1][x][n][1] = "tileHead"
                        tiles[y + 1][x][n][3] = [f'point({page}, {position})', name]
                        
                        tiles[y + 0][x][n][1] = "tileBody"
                        tiles[y + 0][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x][n][1] = "tileBody"
                        tiles[y + 2][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x][n][1] = "tileBody"
                        tiles[y + 3][x][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 0][x + 1][n][1] = "tileBody"
                        tiles[y + 0][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 1][n][1] = "tileBody"
                        tiles[y + 1][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 1][n][1] = "tileBody"
                        tiles[y + 2][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 1][n][1] = "tileBody"
                        tiles[y + 3][x + 1][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (4,4):
                        tiles[y + 0][x + 0][n][1] = "tileBody"
                        tiles[y + 0][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 0][n][1] = "tileBody"
                        tiles[y + 1][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 0][n][1] = "tileBody"
                        tiles[y + 2][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 0][n][1] = "tileBody"
                        tiles[y + 3][x + 0][n][3] = [f'point({page}, {position})', 1]
                        
                        
                        tiles[y + 0][x + 1][n][1] = "tileBody"
                        tiles[y + 0][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 1][n][1] = "tileHead"
                        tiles[y + 1][x + 1][n][3] = [f'point({page}, {position})', name]
                        tiles[y + 2][x + 1][n][1] = "tileBody"
                        tiles[y + 2][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 1][n][1] = "tileBody"
                        tiles[y + 3][x + 1][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 0][x + 2][n][1] = "tileBody"
                        tiles[y + 0][x + 2][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 2][n][1] = "tileBody"
                        tiles[y + 1][x + 2][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 2][n][1] = "tileBody"
                        tiles[y + 2][x + 2][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 2][n][1] = "tileBody"
                        tiles[y + 3][x + 2][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 0][x + 3][n][1] = "tileBody"
                        tiles[y + 0][x + 3][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 3][n][1] = "tileBody"
                        tiles[y + 1][x + 3][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 3][n][1] = "tileBody"
                        tiles[y + 2][x + 3][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 3][n][1] = "tileBody"
                        tiles[y + 3][x + 3][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (4,1):
                        tiles[y + 0][x + 0][n][1] = "tileBody"
                        tiles[y + 0][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 0][n][1] = "tileHead"
                        tiles[y + 1][x + 0][n][3] = [f'point({page}, {position})', name]
                        tiles[y + 2][x + 0][n][1] = "tileBody"
                        tiles[y + 2][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 0][n][1] = "tileBody"
                        tiles[y + 3][x + 0][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (4,5):
                        tiles[y + 0][x + 0][n][1] = "tileBody"
                        tiles[y + 0][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 0][n][1] = "tileBody"
                        tiles[y + 1][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 0][n][1] = "tileBody"
                        tiles[y + 2][x + 0][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 0][n][1] = "tileBody"
                        tiles[y + 3][x + 0][n][3] = [f'point({page}, {position})', 1]
                        
                        
                        tiles[y + 0][x + 1][n][1] = "tileBody"
                        tiles[y + 0][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 1][n][1] = "tileBody"
                        tiles[y + 1][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 1][n][1] = "tileBody"
                        tiles[y + 2][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 1][n][1] = "tileBody"
                        tiles[y + 3][x + 1][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 0][x + 2][n][1] = "tileBody"
                        tiles[y + 0][x + 2][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 2][n][1] = "tileHead"
                        tiles[y + 1][x + 2][n][3] = [f'point({page}, {position})', name]
                        tiles[y + 2][x + 2][n][1] = "tileBody"
                        tiles[y + 2][x + 2][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 2][n][1] = "tileBody"
                        tiles[y + 3][x + 2][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 0][x + 3][n][1] = "tileBody"
                        tiles[y + 0][x + 3][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 3][n][1] = "tileBody"
                        tiles[y + 1][x + 3][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 3][n][1] = "tileBody"
                        tiles[y + 2][x + 3][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 3][n][1] = "tileBody"
                        tiles[y + 3][x + 3][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 0][x + 4][n][1] = "tileBody"
                        tiles[y + 0][x + 4][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 4][n][1] = "tileBody"
                        tiles[y + 1][x + 4][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 4][n][1] = "tileBody"
                        tiles[y + 2][x + 4][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 4][n][1] = "tileBody"
                        tiles[y + 3][x + 4][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (5,1):
                        tiles[y + 0][x][n][1] = "tileBody"
                        tiles[y + 0][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x][n][1] = "tileBody"
                        tiles[y + 1][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x][n][1] = "tileHead"
                        tiles[y + 2][x][n][3] = [f'point({page}, {position})', name]
                        tiles[y + 3][x][n][1] = "tileBody"
                        tiles[y + 3][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 4][x][n][1] = "tileBody"
                        tiles[y + 4][x][n][3] = [f'point({page}, {position})', 1]
                        
                    elif get_size_from_name(shapes_with_colors, name) == (5,2):
                        tiles[y + 0][x][n][1] = "tileBody"
                        tiles[y + 0][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x][n][1] = "tileBody"
                        tiles[y + 1][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x][n][1] = "tileHead"
                        tiles[y + 2][x][n][3] = [f'point({page}, {position})', name]
                        tiles[y + 3][x][n][1] = "tileBody"
                        tiles[y + 3][x][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 4][x][n][1] = "tileBody"
                        tiles[y + 4][x][n][3] = [f'point({page}, {position})', 1]
                        
                        tiles[y + 0][x + 1][n][1] = "tileBody"
                        tiles[y + 0][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 1][x + 1][n][1] = "tileBody"
                        tiles[y + 1][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 2][x + 1][n][1] = "tileBody"
                        tiles[y + 2][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 3][x + 1][n][1] = "tileBody"
                        tiles[y + 3][x + 1][n][3] = [f'point({page}, {position})', 1]
                        tiles[y + 4][x + 1][n][1] = "tileBody"
                        tiles[y + 4][x + 1][n][3] = [f'point({page}, {position})', 1]
            
                # if get_size_from_name(shapes_with_colors, name) == (2,1) or get_size_from_name(shapes_with_colors, name) == (1,1):
                    # Place tileHead
                    # tiles[y + (size[0] // 2)][x + (size[1] // 2)][abs(i-2)][1] = "tileHead"
                    # tiles[y + (size[0] // 2)][x + (size[1] // 2)][abs(i-2)][3] = [f'point({page}, {position})', name]
    
    with open(fr'C:\Niko\Desktop\pather\level\{level_name}', 'r') as f:
        lines = f.read().split('\n')
    
    lines[1] = f'[#lastKeys: [#L: 0, #m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #c: 0, #q: 0], #Keys: [#L: 0, #m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #c: 0, #q: 0], #workLayer: 1, #lstMsPs: point(148, 62), #tlMatrix: {decode_matrix(tiles)}, #defaultMaterial: "Concrete", #toolType: "material", #toolData: "PlaceholderMateralplacer", #tmPos: point(4, 1), #tmSavPosL: [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1], #specialEdit: 0]'
    
    lines = '\n'.join(lines)
    
    with open(fr'C:\Niko\Desktop\pather\level\{level_name.split(".")[0]}_modified.{level_name.split(".")[1]}', 'w') as f:
        f.write(lines)