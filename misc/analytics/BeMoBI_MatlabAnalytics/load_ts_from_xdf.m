[Streams,Header] = load_xdf('.\..\Test_Data\test_ts.xdf')

stream = Streams{1,3}
marker = Streams{1,7}

stream_header = stream.info

ts = timeseries(stream.time_series(2,:), stream.time_stamps, 'Name', 'FrameTime')

figure
ycoord_marker =  repmat(0.009,1,length(marker.time_stamps))
ycoord_zeros = repmat(0.005,1,length(marker.time_stamps))
x = [marker.time_stamps; marker.time_stamps]
y = [ycoord_zeros;ycoord_marker]


plot(ts)
h1 = line(x, y)

% Set properties of lines
set(h1,'Color','k','LineWidth',1)