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