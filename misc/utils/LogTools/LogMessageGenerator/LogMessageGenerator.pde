import controlP5.*;
import java.util.*;
import hypermedia.net.*;

int PORT_SX=4243; 
int PORT_TX=4242;
String HOST_IP = "localhost";//IP Address of the PC in which this App is running
UDP udp;//Create UDP object for recieving

ControlP5 cp5;

void setup(){
  udp= new UDP(this, PORT_SX, HOST_IP);
  udp.log(true);
  size(700, 400);
  
  cp5 = new ControlP5(this);
  
    cp5.addTextfield("target host name")
     .setPosition(20,100)
     .setSize(145,20)
     .setFocus(true)
     .setColor(color(255,0,0))
     .setValue("localhost")
     ;
     
   cp5.addTextfield("target host port")
     .setPosition(170,100)
     .setSize(50,20)
     .setFocus(true)
     .setColor(color(255,0,0))
     .setValue("4242")
     ;
     
  cp5.addButton("Send")
     .setPosition(225,100)
     .setSize(50,19);
}

void draw(){
}

public void controlEvent(ControlEvent theEvent) {
  println(theEvent.getController().getName());
}

public void Send() { 
  String logType = "Info"; 
  udp.send(new Date() + logType + " Buzz ", "localhost", 4242);
}

String GetTargetHostName(){
 return cp5.get(Textfield.class,"target host name").getStringValue();
}

int GetTargetPortName(){
 String portAsString = cp5.get(Textfield.class,"target host port").getStringValue();
 int port = Integer.parseInt(portAsString); //<>//
 return port;
}