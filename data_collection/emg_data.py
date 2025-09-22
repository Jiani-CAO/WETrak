import serial
import keyboard
import time
import csv

#----------------------------------------------------------------------------------------
class EmgCollector():
    def __init__(self,arduino_port,baud,flag):
        self.arduino_port = arduino_port
        self.baud = baud
        self.ser = serial.Serial(self.arduino_port, self.baud)
        self.sensor_data = []
        self.flag = flag

        print "Connected to Arduino port:" + self.arduino_port

    def get_emg_data(self):
        getData = self.ser.readline()
        data = getData[0:][:-2]
        readings = data.split(",")

        if self.flag == True:
            self.sensor_data.append(readings)

def main():
    arduino_port = "COM3"  # serial port
    baud = 921600  # arduino uno runs at xxx baud
    file_name = "emg_data.csv"
    emg_listener = EmgCollector(arduino_port, baud)

    start = time.time()

    while True:
        if keyboard.is_pressed('enter'):
            break

        emg_listener.get_emg_data()

    end = time.time()
    sample_fre = len(emg_listener.sensor_data) / (end - start)
    print "sample_fre", sample_fre
    print "len(sensor_data)", len(emg_listener.sensor_data)

    # create the CSV
    with open(file_name, 'wb') as f:
        writer = csv.writer(f)
        writer.writerows(emg_listener.sensor_data)

    print("Data collection complete!")
    f.close()


if __name__ == '__main__':
    main()






















