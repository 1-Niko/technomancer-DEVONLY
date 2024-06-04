class Iterator:
    def __init__(self, bits: int):
        self.bits = bits
        self.counter = [0] * bits

    def __repr__(self):
        return f'Iterator({self.counter})'

    def __iter__(self):
        return self

    def __next__(self):
        for idx in range(len(self.counter)):
            if self.counter[len(self.counter) - idx - 1] == 0:
                self.counter[len(self.counter) - idx - 1] = 1
                return self.counter
            else:
                self.counter[len(self.counter) - idx - 1] = 0
        raise StopIteration

    def asNum(self):
        return sum([i * n for i, n in zip([pow(2,n) for n in range(len(self.counter))][::-1], self.counter)])

    def determine_viability(self):
        # Rules:
        # 1. Can't have any gaps in the bits with a distance greater than N
        N = 6
        invalid_values = [[1] + ([0] * (N - 2)) + [1], [0] * N]
        for i in range(len(self.counter) - N + 1):
            if self.counter[i:i+N] in invalid_values:
                return False
        # Will add more later
        return True

if __name__ == '__main__':
    a = Iterator(71)
    for i in a:
        if a.determine_viability():
            print(i, a.asNum())
        print(a.asNum(),end='\r')