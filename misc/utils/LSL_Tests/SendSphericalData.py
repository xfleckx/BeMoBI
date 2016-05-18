import sys
sys.path.append('./pylsl') # help python find pylsl relative to this example program
from pylsl import StreamInfo, StreamOutlet
import random
import time
import math

#Send spherical coordinats 
info = StreamInfo('RandomSpehricalData','3DCoord',3,100,'float32','myuid34234');

# next make an outlet
outlet = StreamOutlet(info)

print("now sending data...")
while True:
	current = time.time()
	# make a new random 8-channel sample; this is converted into a pylsl.vectorf (the data type that is expected by push_sample)
	sample = [1 + math.sin(current),  1 + math.cos(current), 1 + math.sin(current) ]

	# now send it and wait for a bit
	outlet.push_sample(sample)
	time.sleep(0.01)
