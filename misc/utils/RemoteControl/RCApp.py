from Tkinter import Tk
from RC_UI import RC_UI
from CommandSender import CommandSender

def main():
  
    root = Tk()
    #root.geometry("250x150+300+300")
    sender = CommandSender()
    app = RC_UI(root, sender)
    root.mainloop()

if __name__ == '__main__':
    main()