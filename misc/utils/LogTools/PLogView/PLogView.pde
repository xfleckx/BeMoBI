import hypermedia.net.*;
import java.util.*;

List<String> history = new ArrayList<String>();

int PORT_RX=4242;
String HOST_IP = "localhost";//IP Address of the PC in which this App is running
UDP udp;//Create UDP object for recieving

void setup(){
  udp= new UDP(this, PORT_RX, HOST_IP);
  udp.log(false);
  udp.listen(true);
  
  size(700, 400);

}

void draw(){
  background(0);
  
  int count = history.size();
  
  for(int i = count - 1; i >= 0; i--){
    int y_offset = i * 12;
    text(history.get(i), 10, y_offset);
  }
  
}


void receive(byte[] data, String HOST_IP, int PORT_RX){
 
  String value=new String(data);
  history.add(value);
    
}