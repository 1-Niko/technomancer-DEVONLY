import re
import math

def rotate_points(points_str, degrees):
    # Extract the float values from the string
    float_pattern = r'[-+]?\d*\.\d+|\d+'
    points = re.findall(fr'point\(({float_pattern}), ({float_pattern})\)', points_str)

    # Calculate the center point
    x_center = sum(float(p[0]) for p in points) / len(points)
    y_center = sum(float(p[1]) for p in points) / len(points)

    # Convert degrees to radians for the rotation
    radians = math.radians(degrees)

    # Rotate each point around the center and format as strings
    rotated_points = []
    for point in points:
        x = float(point[0])
        y = float(point[1])
        rotated_x = (x - x_center) * math.cos(radians) - (y - y_center) * math.sin(radians) + x_center
        rotated_y = (x - x_center) * math.sin(radians) + (y - y_center) * math.cos(radians) + y_center
        rotated_points.append((f'{rotated_x:.4f}', f'{rotated_y:.4f}'))

    # Format the points as a string and return
    formatted_points = ', '.join([f'point({p[0]}, {p[1]})' for p in rotated_points])
    return '[' + formatted_points + ']'

blanks = ['[point(880.0000, 241.0000), point(928.0000, 241.0000), point(928.0000, 289.0000), point(880.0000, 289.0000)]', '[point(880.0000, 289.0000), point(928.0000, 289.0000), point(928.0000, 337.0000), point(880.0000, 337.0000)]']

template = '[-20, "8fan", point(8, 1), <FIRSTONE>, [#settings: [#renderorder: 0, #seed: 663, #renderTime: 0]]], [-20, "8fan", point(8, 1), <SECONDONE>, [#settings: [#renderorder: 0, #seed: 663, #renderTime: 0]]]'

# [-20, "8fan", point(8, 1), [point(880.0000, 241.0000), point(928.0000, 241.0000), point(928.0000, 289.0000), point(880.0000, 289.0000)], [#settings: [#renderorder: 0, #seed: 663, #renderTime: 0]]], [-20, "8fan", point(8, 1), [point(880.0000, 289.0000), point(928.0000, 289.0000), point(928.0000, 337.0000), point(880.0000, 337.0000)], [#settings: [#renderorder: 0, #seed: 663, #renderTime: 0]]]

def f(x, n=2):
    return (30 * n * x) % 360

def changetemplate(template, blanks, degree_A, degree_B):
    return template.replace('<FIRSTONE>', rotate_points(blanks[0], degree_A)).replace('<SECONDONE>', rotate_points(blanks[1], degree_B))

replaceKey = "<ALL THE MODIFIED STUFF GOES HERE>"

# data = open('TL_TRAINEXAMPLE.txt', 'r').read()

# print(changetemplate(template, blanks, 45, 50))
for idx, i in enumerate(zip([15,75,135,195,255,315],[345,45,105,165,225,285])):
    print(idx, changetemplate(template, blanks, i[0], i[1]))
    print()
    # with open(f'TL_TRAINEXAMPLE_{idx}.txt', 'w') as f:
        # f.write(data.replace(replaceKey, changetemplate(template, blanks, i[0], i[1])))