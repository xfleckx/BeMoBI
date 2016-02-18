"""Example program to show how to read a marker time series from LSL."""

from pylsl import StreamInlet, resolve_stream
import sys
import os
from collections import deque

os.system('cls' if os.name == 'nt' else 'clear')

MAX_ELEMENTS_IN_QUEUE = 5

# first resolve an EEG stream on the lab network
print("looking for an Gait stream...")
streams = resolve_stream('type', 'Force')

streamsFound = len(streams)

if (streamsFound > 0):
	print 'found ' + str(streamsFound)
else:
	print 'found none'

# create a new inlet to read from the stream
inlet = StreamInlet(streams[0])

markerStack = deque([''])

while True:
	
	sample, timestamp = inlet.pull_sample()

	markerStack.append(str(timestamp) + '\t' + sample[0])

	if len(markerStack) > MAX_ELEMENTS_IN_QUEUE:
		markerStack.popleft()

	os.system('cls' if os.name == 'nt' else 'clear')

	for marker in reversed(markerStack):
		print marker