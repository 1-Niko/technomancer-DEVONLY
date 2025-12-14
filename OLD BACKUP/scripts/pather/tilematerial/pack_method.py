import numpy as np
import cv2
import random

def get_random_color():
    """Generate a random color in (B, G, R) format."""
    return tuple(random.randint(0, 255) for _ in range(3))

def fill_rectangle(image, top_left, width, height, color):
    """Fill a rectangle in the image with the specified color."""
    x, y = top_left
    image[y:y + height, x:x + width] = color

def main(shapes_with_colors):
    random.seed(12345)
    
    # Generate colors for shapes without specified colors
    unique_shapes = {}
    for shape in shapes_with_colors:
        size = shape['size']
        if shape.get('color', None) is None:
            shape['color'] = get_random_color()
        if size not in unique_shapes:
            unique_shapes[size] = []
        unique_shapes[size].append(shape['color'])

    img = cv2.imread('output.png')
    img_red_channel = img[:, :, 2]  # Extract the red channel

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

        attempts = 100  # Number of attempts to place the shape
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

if __name__ == "__main__":
    shapes_with_colors = [
        # {'size': (2, 2), 'name': 'Metal Holes', 'weight': 1},  # Metal Holes
        {'size': (3, 3), 'name': 'Dyson Fan', 'weight': 1},  # Dyson Fan
        # {'size': (3, 3), 'name': 'Metal Big Fan', 'weight': 1},  # Big Fan
        {'size': (4, 2), 'name': 'machine box A', 'weight': 1},  # machine box A
        # {'size': (, ), 'color': (, , )}, # 
    ]
    
    positions = main(shapes_with_colors)
    
    data = cv2.imread('output.png') + cv2.imread('excluded.png')
    
    data[:,:,0:2] = 0
    
    for (x, y), name in positions.items():
        if name is not None:
            found = next(shape for shape in shapes_with_colors if shape['name'] == name)
            shape_width, shape_height = found['size']
            for dx in range(shape_width):
                for dy in range(shape_height):
                    if x + dx < data.shape[1] and y + dy < data.shape[0]:
                        data[y + dy, x + dx, :] = [0, 0, 0]
    
    cv2.imwrite('output.png', data)
    
    print(positions)