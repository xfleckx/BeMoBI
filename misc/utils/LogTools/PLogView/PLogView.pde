import hypermedia.net.*;
import java.util.*;
import java.util.regex.Pattern;

List<LogEntry> history = new ArrayList<LogEntry>();
Pattern infoMessagePattern = Pattern.compile(".*(Info)");
int PORT_RX=4242;
String HOST_IP = "localhost";//IP Address of the PC in which this App is running
UDP udp;//Create UDP object for recieving

void setup() {
  udp= new UDP(this, PORT_RX, HOST_IP);
  udp.log(false);
  udp.listen(true);
    
  size(700, 400);
}

void draw() {
  background(0);

  int count = history.size();
  int y_offset = 0;
  for (int i = count - 1; i >= 0; i--) {
     history.get(i).render(new PVector(10, y_offset), width - 10, 50);
  }
}


void receive(byte[] data, String HOST_IP, int PORT_RX) {
  String message = new String(data);
  println(message);
  LogEntry value = GetFrom( message );
  history.add(value);
}

public LogEntry GetFrom(String s) {
  LogEntry e = new LogEntry();
  e.content = s;
  
  if (match(s, "Info") != null) {
    e.textColor = color( 113, 224, 138 );
  }
  
  return e;
}