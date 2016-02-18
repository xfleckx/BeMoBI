[Streams,Header] = load_xdf('.\..\Test_Data\test_ts.xdf')

stream = Streams{1,3}

stream_header = stream.info

ts = timeseries(stream.time_series(2,:), stream.time_stamps, 'Name', 'FrameTime')

plot(ts)