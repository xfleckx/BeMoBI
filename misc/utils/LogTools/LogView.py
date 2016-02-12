__doc__="""Usage: logview [options] [<port>]

Options:
	-h, --help			show this help message
	-v, --verbose		print status messages
	--ignore=loglevels	ignore logs of the specified levels
"""

import threading
import socket
import logging
import os
import colorama
from docopt import docopt
from termcolor import colored
from collections import deque

MAX_ELEMENTS_IN_QUEUE = 5
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

    def listen(self):
        while True:
            msg = self.sock.recv(4096)
            appendMessageToBuffer(msg)
            updateView()

    def start_listening(self):
        t = threading.Thread(target=self.listen)
        t.start()

if __name__ == "__main__": 

    arguments = docopt(__doc__, version='LogView 0.0.1')
    colorama.init()
    

    listener = UdpListener()
    listener.start_listening()
