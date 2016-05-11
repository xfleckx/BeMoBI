import java.util.*;
import java.util.concurrent.*;

public class LogHistory implements Iterable<LogEntry> {
 
  private Queue<LogEntry> history;
  private int maxSize = 10;
  
  public LogHistory(int size){
    history = new ArrayBlockingQueue<LogEntry>(size);
    maxSize = size;
  }
  
  public void add(LogEntry newEntry){
     
    if(history.size() == maxSize)
       history.poll();
       
    history.offer(newEntry);  
  }
  
  public Iterator<LogEntry> iterator(){ 
    return history.iterator();
  }
 
  public ListIterator<LogEntry> listIterator(){
    
    List<LogEntry> temp = new ArrayList(history);
    
    return temp.listIterator(temp.size());
  }
 
 public void clear()
 {
   history.clear();
 }
 
}