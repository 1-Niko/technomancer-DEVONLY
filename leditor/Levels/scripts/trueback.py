import cv2
import numpy as np
import os

# Get the current directory
current_dir = os.path.dirname(os.path.abspath(__file__))

def change_format(string):
    return string.replace('bkg','').replace('.png','_bkg.png')

# Loop through all PNG files in the current directory
for file in os.listdir(current_dir):
    if file.endswith(".png") and  "bkg" in file:
        # Read the image using OpenCV
        cv2.imwrite(file, cv2.imread(file, cv2.IMREAD_GRAYSCALE))
        image = cv2.imread(os.path.join(current_dir, file))

        # Find the positions of pure white and non-pure white pixels in this image using NumPy
        white_mask = np.all(image == 255, axis=2)
        white_positions, non_white_positions = np.argwhere(white_mask), np.argwhere(~white_mask)

        zeros = np.zeros((image.shape[0], image.shape[1], 4), dtype=int)
        zeros[:,:,:3] = image
        zeros[:,:,3] = 255

        zeros[white_positions[:, 0], white_positions[:, 1]] = [0, 0, 0, 0]
        zeros[non_white_positions[:, 0], non_white_positions[:, 1], 3] = 48
        

        debug = np.asarray(zeros)

        # print(type(np.asarray(debug)))

        cv2.imwrite(change_format(file), debug)
        os.remove(file)