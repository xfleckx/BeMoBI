"""Usage: 
prmc (start | pause | init (name) | restart) (hostname) [<port>]

Options:
	start						send the start command to the paradigm
	pause						send the pause command to the paradigm
	init <name>					initialize condition named <name>
	restart [--only-condition]	restarts the paradigm or the current condition			
	<port>						specify the port at the target host which is running the paradigm
	-h, --help					show this help message
	-v, --verbose				print status messages
"""

import threading
import sys
import socket
import logging
import os
import colorama
from docopt import docopt
from termcolor import colored
from collections import deque

if __name__ == '__main__':
    arguments = docopt(__doc__)
    print(arguments)