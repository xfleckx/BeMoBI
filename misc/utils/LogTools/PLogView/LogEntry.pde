import java.util.regex.Pattern;

public class LogEntry{

  public String content;
  
  public color textColor = color(255,255,255);
  public float fontSize = 14;
  public color backgroundColor;
  
  public void render(PVector pos, float w, float h){
    
    pushMatrix();
    pushStyle();
    
    translate(pos.x, pos.y);
    
    rect(0, h, 0, h);
    
    fill(textColor);
    
    textSize(fontSize);
    rectMode(CORNERS);
    
    text(content, 0, 0 + fontSize);
    
    popStyle();
    popMatrix();
    
  }

}