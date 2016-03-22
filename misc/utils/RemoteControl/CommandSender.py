import socket
class CommandSender(object):
    """description of class"""

    def __init__(self, *args, **kwargs):
        return super(CommandSender, self).__init__(*args, **kwargs)

    def send(self, target, message):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.settimeout(3)
        print "send '" + message + "' to " + target[0] + ":" + str(target[1])
        self.sock.connect(target)
        self.sock.sendall(message)
        self.sock.close()
