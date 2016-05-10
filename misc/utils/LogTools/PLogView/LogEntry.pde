import java.util.regex.Pattern;

public class LogEntry{

  public String content;
  
  public color textColor = color(255,255,255);
  public float fontSize = 14;
  public color backgroundColor;
  
  public void render(PVector pos, float w, float h){
    
    pushMatrix();
    pushStyle();
    
    fill(textColor);
    
    textSize(fontSize);
    text(content, pos.x, pos.y + fontSize, w, h);
    
    popStyle();
    popMatrix();
    
  }

}