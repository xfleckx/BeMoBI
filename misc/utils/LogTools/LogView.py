import threading
import socket
import logging
import os
import colorama
from termcolor import colored
from collections import deque


markerStack = deque([''])

def colorMessage(message):
    if 'Info' in message :
        print(colored(message, 'green'))
    elif 'Error' in message :
        print(colored(message, 'red'))
    elif 'Fatal' in message :
        print(colored(message, 'red', 'white'))
    else:
        print(message)

def appendMessageToBuffer(message):
    markerStack.append(message)
    if len(markerStack) > MAX_ELEMENTS_IN_QUEUE:
        markerStack.popleft()

def updateView():
    for marker in reversed(markerStack):
        colorMessage(marker)

class UdpListener():

    def __init__(self):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.bind(('127.0.0.1', 4242))
        self.clients_list = []

    def listen(self):
        while True:
            msg = self.sock.recv(4096)
            appendMessageToBuffer(msg)
            updateView()

    def start_listening(self):
        t = threading.Thread(target=self.listen)
        t.start()

if __name__ == "__main__": 

    print 'call'
    colorama.init()
    

    listener = UdpListener()
    listener.start_listening()