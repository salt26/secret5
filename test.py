"""
import time
import traceback

done = False
input()
try:
    while not done:
        string = input()
        print("# done: " + string)
        if int(string) == 1:
            done = True
        if done:
            break
        string = input()
        print("# current state: " + string)
        print("1 0 1 0 1 0 1 0")
        string = input()
        print("# performed: " + string)
        string = input()
        print("# next state: " + string)
        string = input()
        print("# reward: " + string)
        string = input()
        print("# done: " + string)
        if int(string) == 1:
            done = True
except Exception:
    traceback.print_exc()

print("Program terminates")
time.sleep(10)
"""
import sys
import matplotlib.pyplot as plt

file = open("record.txt", mode='r', encoding='utf-8')
recent_win_rates = []
win_rates = []
while True:
    line = file.readline()
    if not line:
        break
    if "recent win rate: " in line:
        index = line.find("=")
        recent_win_rates.append(float(line[index+2:]))
    elif "Loss: " in line and "win rate: " in line:
        index = line.find("=")
        win_rates.append(float(line[index+2:]))
r = 0
index = 0
for rate in win_rates:
    if r < rate:
        r = rate
        index = win_rates.index(rate)
print("max win rate: " + str(r) + "    episode: " + str(index * 10 + 1))

r = 0
index = 0
for rate in recent_win_rates:
    if r < rate:
        r = rate
        index = recent_win_rates.index(rate)
print("max recent win rate: " + str(r) + "    episode: " + str(index * 30 + 1))

file.close()
plt.plot([i * 30 + 1 for i in range(1, len(recent_win_rates) + 1)], recent_win_rates, color="Green")
plt.plot([i * 10 + 1 for i in range(1, len(win_rates) + 1)], win_rates, color="Blue")
plt.show()

