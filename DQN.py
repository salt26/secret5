# Nature 2015
import numpy as np
# import gym
import random
import tensorflow as tf
from collections import deque
import sys
import time
import traceback
from datetime import datetime

# env = gym.make('CartPole-v0')
# env._max_episode_steps = 10001

# Constants defining our neural network
input_size = 81  # env.observation_space.shape[0]
output_size = 8  # env.action_space.n

# Set Q-learning related parameters
dis = .99
REPLAY_MEMORY = 50000


class DQN:
    def __init__(self, session, input_size, output_size, name="main"):
        self.session = session
        self.input_size = input_size
        self.output_size = output_size
        self.net_name = name

        self._build_network()

    def _build_network(self, h_size=300, l_rate=1e-2):
        with tf.variable_scope(self.net_name):
            self._X = tf.placeholder(
                tf.float32, [None, self.input_size], name="input_x")

            # First layer of weights
            W1 = tf.get_variable("W1", shape=[self.input_size, h_size],
                                 initializer=tf.contrib.layers.xavier_initializer())
            layer1 = tf.nn.tanh(tf.matmul(self._X, W1))

            # Second layer of weights
            W2 = tf.get_variable("W2", shape=[h_size, self.output_size],
                                 initializer=tf.contrib.layers.xavier_initializer())

            # Q prediction
            self._Qpred = tf.matmul(layer1, W2)

        # We need to define the parts of the network needed for learning a
        # policy
        self._Y = tf.placeholder(
            shape=[None, self.output_size], dtype=tf.float32)
        # Loss function
        self._loss = tf.reduce_mean(tf.square(self._Y - self._Qpred))
        # Learning
        self._train = tf.train.AdamOptimizer(
            learning_rate=l_rate).minimize(self._loss)

    def predict(self, state):
        x = np.reshape(state, [1, self.input_size])
        return self.session.run(self._Qpred, feed_dict={self._X: x})

    def update(self, x_stack, y_stack):
        return self.session.run([self._loss, self._train], feed_dict={
            self._X: x_stack, self._Y: y_stack})


def replay_train(mainDQN, targetDQN, train_batch):
    x_stack = np.empty(0).reshape(0, input_size)
    y_stack = np.empty(0).reshape(0, output_size)

    # Get stored information from the buffer
    for state, action, reward, next_state, done in train_batch:
        Q = mainDQN.predict(state)

        # terminal?
        if done:
            Q[0, action] = reward
        else:
            # get target from target DQN (Q')
            Q[0, action] = reward + dis * np.max(
                targetDQN.predict(next_state))
        y_stack = np.vstack([y_stack, Q])
        x_stack = np.vstack([x_stack, state])

    # Train our network using target and predicted Q values on each episode
    return mainDQN.update(x_stack, y_stack)


def get_copy_var_ops(*, dest_scope_name="target", src_scope_name="main"):
    # Copy variables src_scope to dest_scope
    op_holder = []

    src_vars = tf.get_collection(
        tf.GraphKeys.TRAINABLE_VARIABLES, scope=src_scope_name)
    dest_vars = tf.get_collection(
        tf.GraphKeys.TRAINABLE_VARIABLES, scope=dest_scope_name)

    return op_holder

"""
def bot_play(mainDQN):
    # See our trained network in action
    s = env.reset()
    reward_sum = 0
    while True:
        env.render()
        a = np.argmax(mainDQN.predict(s))
        s, reward, done, _ = env.step(a)
        reward_sum += reward
        if done:
            print("# Total score: {}".format(reward_sum))
            break
"""

def main():
    max_episodes = 2000
    # store the previous observations in replay memory
    replay_buffer = deque()

    win_count = 0
    log_record = []
    last_log_length = 0

    try:
        file = open("record.txt", mode='w', encoding='utf-8')
        file.write(datetime.now().strftime('%Y-%m-%d %H:%M:%S') + "\n")
        file.close()

        with tf.Session() as sess:
            mainDQN = DQN(sess, input_size, output_size, name="main")
            targetDQN = DQN(sess, input_size, output_size, name="target")
            tf.global_variables_initializer().run()

            # initial copy q_net -> target_net
            copy_ops = get_copy_var_ops(dest_scope_name="target", src_scope_name="main")

            sess.run(copy_ops)
            input()

            for episode in range(max_episodes):
                e = 1. / ((episode / 10) + 1)
                done = 0
                step_count = 0
                reward_sum = 0

                while done == 0:
                    done = int(input())
                    print("# done(start): " + str(done))
                    if done > 0:
                        break
                    state = list(map(float, input().split(' ')))  # secret5.state()
                    print("# state: " + str(state))
                    if np.random.rand(1) < e:
                        action = [[1, 1, 1, 1, 1, 1, 1, 1]]  # env.action_space.sample()
                    else:
                        action = mainDQN.predict(state).tolist() # np.argmax(mainDQN.predict(state))

                    print("# action: " + str(action[0]))
                    # Get new state and reward from environment
                    string_out = []
                    for a in action[0]:
                        string_out.append(str(a))
                    print(' '.join(string_out))
                    performed_action = list(map(int, input().split(' ')))  # secret5.step(action)
                    print("# performed_action: " + str(performed_action))
                    next_state = list(map(float, input().split(' ')))
                    print("# next_state: " + str(next_state))
                    reward = float(input())
                    print("# reward: " + str(reward))
                    done = int(input())
                    print("# done(end): " + str(done))
                    """
                    if done:  # penalty
                        reward = -100
                    """
                    reward_sum += reward

                    if done > 0:
                        done_record = True
                    else:
                        done_record = False
                    # Save the experiance to our buffer
                    replay_buffer.append((state, performed_action, reward, next_state, done_record))
                    if len(replay_buffer) > REPLAY_MEMORY:
                        replay_buffer.popleft()

                    step_count += 1
                    """
                    if step_count > 10000:  # Good enough. Let's move on
                        break
                    """

                if done == 1:
                    win = True
                    win_count += 1
                elif done == 2:
                    win = False

                log = "Episode: {}    steps: {}    reward sum: {}    win: {}".format(episode + 1, step_count, reward_sum, win)
                print("# " + log)
                log_record.append(log)
                if step_count > 10000:
                    pass  # break

                if episode % 10 == 1:  # train every 10 episode
                    # Get a random batch of experiences.
                    for _ in range(50):
                        minibatch = random.sample(replay_buffer, 10)
                        loss, _ = replay_train(mainDQN, targetDQN, minibatch)

                    log = "Loss: {}    win rate: {} / {} = {}".format(loss, win_count, episode + 1, (float(win_count)/(episode + 1)))
                    print("# " + log)
                    log_record.append(log)

                    file = open("record.txt", mode='at', encoding='utf-8')
                    file.write('\n'.join(log_record[last_log_length:]) + '\n')
                    file.close()
                    last_log_length = len(log_record)

                    # Copy q_net -> target_net
                    sess.run(copy_ops)

            # bot_play(mainDQN)
    except Exception:
        traceback.print_exc()
        time.sleep(8)

    file = open("record.txt", mode='at', encoding='utf-8')
    file.write('\n'.join(log_record[last_log_length:]) + "\nwin rate: {} / {} = {}\n".format(win_count, max_episodes,
                                                                           (float(win_count)/max_episodes)))
    file.write(datetime.now().strftime('%Y-%m-%d %H:%M:%S') + "\n")
    file.close()


if __name__ == "__main__":
    main()

