from PIL import Image

def make_black_and_white(filename):
    with Image.open(filename) as im:
        # Convert the image to grayscale and then to black and white.
        bw_im = im.convert('L').point(lambda x: 0 if x < 255 else 255, '1')

        # Invert the image so that the black and white pixels are swapped.
        # bw_im = Image.eval(bw_im, lambda x: 255-x)

        # Save the new black and white image over the original file.
        bw_im.save(filename)

# Example usage:
filenames = [f'TL_V01SHADOW_{i+1}.png' for i in range(3)]
for filename in filenames:
    make_black_and_white(filename)
