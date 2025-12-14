import random

length_options = [1,2,3,4,5,6]

target_length = 72

def is_valid(target_length, lengths):
    return (sum(lengths) < target_length)

def check_valid_runs(a, length):
    for i in range(len(a)-2):
        if a[i] == a[i+1] == a[i+2]:
            return False
    return True

def validate(run, start_and_end_minimum):
    while True:
        random.shuffle(run)
        if run[0] > start_and_end_minimum and run[-1] > start_and_end_minimum:
            if check_valid_runs(run,3):
                return run

def generate_run(target_length, options):
    lengths = []
    while is_valid(target_length, lengths):
        if sum(lengths) > target_length:
            lengths = []
        choice = -1
        while (choice == -1):
            WEIGHTS = [[4 if lengths.count(1) == 0 else [1 if lengths.count(1) > 0 and lengths.count(1) < 3 else 0][0]][0], 1, 1, 1, [4 if lengths.count(5) == 0 else [1 if lengths.count(5) > 0 and lengths.count(5) < 5 else 0][0]][0], [4 if lengths.count(1) == 0 else [1 if lengths.count(6) > 0 and lengths.count(6) < 3 else 0][0]][0]]
            choice = random.choices([1,2,3,4,5,6], weights=WEIGHTS, k=1)[0]
            # print(WEIGHTS)
        lengths.append(choice)
        if (sum(lengths) - target_length) in options:
            lengths.append(sum(lengths) - target_length)
    return lengths, sum(lengths), sum(lengths) / len(lengths)

if __name__ == '__main__':
    optimized_average = None

    while True:
        run, length, average = generate_run(target_length, length_options)
        if length == target_length and len(run) % 10 == 0 and len(run) >= 30:
            optimized_average = average
            run = validate(run, 2)
            print(''.join([str(i)+' ' for i in run]))