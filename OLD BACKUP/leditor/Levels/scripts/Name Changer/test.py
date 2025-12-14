import os

def iterate_non_py_files():
    current_dir = os.path.dirname(os.path.abspath(__file__))
    for filename in os.listdir(current_dir):
        if filename.endswith(".py"):
            continue

        if os.path.isfile(filename):
            print(f"Processing non-Python file: {filename}")
            # Add your code here to perform operations on non-Python files
            # For example, you can read the contents of the file, modify it, etc.
            modify_file_name(filename, filename.replace('D02', 'SMALLEXAMPLE'))

def modify_file_name(old_name, new_name):
    try:
        os.rename(old_name, new_name)
        print(f"File '{old_name}' renamed to '{new_name}' successfully.")
    except FileNotFoundError:
        print(f"Error: File '{old_name}' not found.")
    except FileExistsError:
        print(f"Error: A file with the name '{new_name}' already exists.")

# if __name__ == "__main__":
    # Example usage:
    # original_file = "example.txt"
    # modified_file = "modified_example.txt"
    # modify_file_name(original_file, modified_file)

if __name__ == "__main__":
    iterate_non_py_files()