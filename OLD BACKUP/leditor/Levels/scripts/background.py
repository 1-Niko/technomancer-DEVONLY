from PIL import Image

def make_transparent(filename):
    with Image.open(filename) as im:
        # Convert the image to RGBA mode, so that we have an alpha channel
        # to control transparency.
        rgba_im = im.convert('RGBA')

        # Create a new transparent image the same size as the original.
        transparent_im = Image.new('RGBA', rgba_im.size, (0, 0, 0, 0))

        # Loop through all the pixels in the image, and copy them to the
        # transparent image if they are not white.
        for x in range(rgba_im.width):
            for y in range(rgba_im.height):
                r, g, b, a = rgba_im.getpixel((x, y))
                if r != 255 or g != 255 or b != 255:
                    transparent_im.putpixel((x, y), (r, g, b, a))

        # Save the new transparent image over the original file.
        transparent_im.save(filename)

# Example usage:
filenames = [f'TL_V01SHADOW_{i+1}.png' for i in range(3)]
for filename in filenames:
    make_transparent(filename)
