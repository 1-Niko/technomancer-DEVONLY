import os

def duplicate_file_with_new_name(source_file_path, destination_file_path):
    with open(source_file_path, 'rb') as source_file:
        with open(destination_file_path, 'wb') as destination_file:
            destination_file.write(source_file.read())
    print(f"File duplicated successfully. New file: {destination_file_path}")

# Example usage:
# source_file_path = 'path/to/source_file.txt'
# destination_file_path = 'path/to/destination_file_copy.txt'
# duplicate_file_with_new_name(source_file_path, destination_file_path)

a = 128
for i in range(a):
    new_name = str(i).rjust(len(str(a)), '0')
    
    duplicate_file_with_new_name('default/default.png', new_name + '.png')
    duplicate_file_with_new_name('default/default.txt', new_name + '.txt')