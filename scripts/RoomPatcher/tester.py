
print(len(test))
print(len(test[0]))
print(test[0][0], len(test[0][0]))
for x in range(len(test)):
    for y in range(len(test[0])):
        if len(test[x][y]) != 3:
            print(x,y,test[x][y])