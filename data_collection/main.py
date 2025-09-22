import emg_data, leap_data
import time, keyboard, json, csv, os
import Leap
import os.path as path

# Create object for emg and leap_motion
arduino_port = "COM3"
baud = 921600
is_saved = False
emg_listener = emg_data.EmgCollector(arduino_port, baud, is_saved)

leap_listener = leap_data.LeapCollector(is_saved)
controller = Leap.Controller()

# Setup (data not be saved)
sample = 700
for i in range(sample):
    emg_listener.get_emg_data()
    controller.add_listener(leap_listener)

# Start collection (data will be saved)
is_saved = True
emg_listener.flag = is_saved
leap_listener.flag = is_saved

# Setup for filter (data not be saved after filter)
filter_sample = 0
start = time.time()
for i in range(filter_sample):
    emg_listener.get_emg_data()
    controller.add_listener(leap_listener)

print "Start Collecting ..."
while True:
    if keyboard.is_pressed('esc'):
        break

    emg_listener.get_emg_data()
    controller.add_listener(leap_listener)

controller.remove_listener(leap_listener)

end = time.time()
emg_sample_fre = len(emg_listener.sensor_data) / (end - start)
print "emg_sample_fre", emg_sample_fre
print "len(emg)", len(emg_listener.sensor_data)

leap_sample_fre = len(leap_listener.sensor_data) / (end - start)
print "leap_sample_fre", leap_sample_fre
print "len(leap)", len(leap_listener.sensor_data)


# Save data
p_root = "D:\\fingerTrack\\Database"

user_name = "test"
session = str(1)
p_user = path.join(p_root,user_name,"data",session)
 
#create folder
folder = os.path.exists(p_user)
if not folder:
    os.makedirs(p_user)
    print "---  new folder...  ---"

# create emg file
emg_file =  open(path.join(p_user,"emg_data.csv"), 'wb')
writer = csv.writer(emg_file)
writer.writerows(emg_listener.sensor_data)
emg_file.close()

# create leap motion file
leap_file = open(path.join(p_user,"leap_data.json"), 'w')
json.dump(leap_listener.sensor_data, leap_file, sort_keys=False, indent=4, separators=(',', ':'))
leap_file.close()

print "Data collection complete!"
