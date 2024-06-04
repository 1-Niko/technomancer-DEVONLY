import cv2
import numpy as np

def count_matching_pixels(A, B, coordinates):
    # coordinates represents the offset
    def black_adjacency(test_array):
        assert (test_array.shape[0] == test_array.shape[1] == test_array.shape[2] == 3)
        if test_array[1,1,:].tobytes() == b'\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00':
            return None
        for i in [[0,1],[1,0],[2,1],[1,2]]:
            if test_array[i[0], i[1], :].tobytes() == b'\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00':
                return True
        return False
    # print(black_adjacency(np.array([[[0, 0, 0], [0, 0, 0], [0, 0, 0]], [[0, 0, 0], [0, 1, 0], [0, 0, 0]], [[0, 0, 0], [0, 0, 0], [0, 0, 0]]])))
    
    for x in 

def find_mask_in_image(input_image, mask_image):
    # Convert images to grayscale
    input_gray = cv2.cvtColor(input_image, cv2.COLOR_BGR2GRAY)
    mask_gray = cv2.cvtColor(mask_image, cv2.COLOR_BGR2GRAY)

    # Create a binary mask from the mask image
    _, mask_binary = cv2.threshold(mask_gray, 1, 255, cv2.THRESH_BINARY)

    # Perform template matching with mask
    result = cv2.matchTemplate(input_gray, mask_gray, cv2.TM_CCORR_NORMED, mask=mask_binary)

    # Set a threshold for matching results
    threshold = 0.8
    locations = np.where(result >= threshold)
    coordinates = []

    # Process each match
    for pt in zip(*locations[::-1]):
        # Calculate the center coordinates
        center_x = pt[0] + mask_image.shape[1] // 2
        center_y = pt[1] + mask_image.shape[0] // 2
        coordinates.append((center_x, center_y))

    if coordinates:
        # Sort the coordinates based on the closest match
        closest_match = min(coordinates, key=lambda c: abs(c[0] - input_image.shape[1] // 2) + abs(c[1] - input_image.shape[0] // 2))
        return closest_match
    else:
        return None

if __name__ == '__main__':
    data = cv2.imread('SB_H03.png')
    count_matching_pixels(data, data, (0, 0))