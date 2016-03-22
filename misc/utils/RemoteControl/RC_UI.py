from Tkinter import Frame, BOTH, LEFT, Text, W, N, E, S, Entry, StringVar
from ttk import Frame, Button, Style, Label

class RC_UI(Frame):
    """description of class"""

    def __init__(self, parent, commandSender):
        Frame.__init__(self, parent)
        self.parent = parent
        self.initUI()
        self.commands = commandSender

    def initUI(self):
      
        self.style = Style()
        self.style.theme_use("default")
        self.parent.title("Remote Control")
        self.pack(fill=BOTH, expand=1)
        
        Style().configure("TButton", padding=(0, 5, 0, 5), 
            font='serif 10')
        
        self.columnconfigure(0, pad=3)
        self.columnconfigure(1, pad=3)
        self.columnconfigure(2, pad=3)
        self.columnconfigure(3, pad=3)
        
        self.rowconfigure(0, pad=3)
        self.rowconfigure(1, pad=3)
        self.rowconfigure(2, pad=3)
        self.rowconfigure(3, pad=3)
        self.rowconfigure(4, pad=3)

        self.port_content = StringVar()
        self.port_content.set(str(7897))
        self.host_content = StringVar()
        self.entry_TargetHost = Entry(self,textvariable=self.host_content)
        self.entry_TargetHost.grid(row=1, column =0, columnspan=3, sticky=W+E)
        self.entry_PortOnTarget = Entry(self, textvariable=self.port_content)
        self.entry_PortOnTarget.grid(row=1, column=3, sticky=W+E)

        cls = Button(self, text="Pause", command = self.send_Pause)
        cls.grid(row=2, column=0)
        bck = Button(self, text="End Pause", command = self.send_PauseEnd)
        bck.grid(row=2, column=1)
        lbl = Button(self, text="Recalibrate", command = self.send_recalibrate)
        lbl.grid(row=2, column=2)
        lbl = Button(self, text="End Experiment", command = self.send_ForceEndExperiment)
        lbl.grid(row=2, column=3)
        
        self.pack()

    def _getTargetAddress(self):
        port = self.port_content.get()
        host = self.host_content.get()
        if not host:
            print "host address not available"
            raise ValueError("Missing target host");
        if not port:
            print "host address not available"
            raise ValueError("Missing target host");

        return (host, int(port))

    def send_Pause(self):
        self.commands.send(self._getTargetAddress(), "pause")

    def send_PauseEnd(self):
        self.commands.send(self._getTargetAddress(), "pause end")

    def send_ForceEndExperiment(self):
        self.commands.send(self._getTargetAddress(), "force_end_of_experiment")

    def send_recalibrate(self):
        self.commands.send(self._getTargetAddress(), "recalibrate_SubjectsOrientation")