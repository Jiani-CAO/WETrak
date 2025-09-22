import Leap
import time
from math import pi
import json
import keyboard


def radians2degree(radians):
    return radians * 180 / pi

class LeapCollector(Leap.Listener):
    def __init__(self,flag):
        Leap.Listener.__init__(self)
        self.finger_names = ['Thumb', 'Index', 'Middle', 'Ring', 'Pinky']
        self.bone_names = ['Metacarpal', 'Proximal', 'Intermediate', 'Distal']
        self.sensor_data = []
        self.flag = flag

    def on_init(self, controller):
        print "Initialized"

    def on_connect(self, controller):
        print "Connected"

    def on_disconnect(self, controller):
        print "Disconnected"

    def on_exit(self, controller):
        print "Exited"

    def on_frame(self, controller):
        # Get the most recent frame and report some basic information
        frame = controller.frame()

        if len(frame.hands) != 0:
            info = dict(zip(['frame_id', 'timestamp', 'hands', 'fingers', 'left_hand', 'right_hand'], [
                        frame.id, frame.timestamp, len(frame.hands), len(frame.fingers), {}, {}]))

            # Get hands
            for hand in frame.hands:
                hand_type = "left_hand" if hand.is_left else "right_hand"

                hand_transform = self.coordinates_transform(hand.basis.x_basis,
                                                            hand.basis.y_basis,
                                                            hand.basis.z_basis,
                                                            hand.arm.wrist_position)

                info[hand_type]['bone_angles'] = self.get_bone_angles(hand)
                info[hand_type]['joint_positions'] = self.get_joint_positions(hand)
                info[hand_type]['bone_length'] = self.get_bone_length(hand)

            if self.flag == True:
                self.sensor_data.append(info)

    def coordinates_transform(self, x_basis, y_basis, z_basis, position):
        '''
        Input: original x, y and z basis of target reference
               original position of reference
        Return: transform matrix
        '''
        transform = Leap.Matrix(x_basis, y_basis, z_basis, position)
        transform = transform.rigid_inverse()
        return transform
    
    def get_finger_by_type(self, hand, type):
        for finger in hand.fingers:
            finger_type = self.finger_names[finger.type]
            if finger_type == type:
                return finger
        return None

    def get_bone_angles(self, hand):
        bone_angles = {}

        for finger in hand.fingers:
            finger_type = self.finger_names[finger.type]
            bone_angles[finger_type] = {}

            if 'Thumb' == finger_type:
                # Proximal Phalanx Pitch
                bone = finger.bone(1)
                bone_type = self.bone_names[bone.type]
                bone_angles[finger_type][bone_type] = {}

                # Set metacarpals of index finger as reference coordinates
                index_finger = self.get_finger_by_type(hand, 'Index')

                pre_bone = index_finger.bone(0)
                transform = self.coordinates_transform(pre_bone.basis.x_basis,
                                                       pre_bone.basis.y_basis,
                                                       pre_bone.basis.z_basis,
                                                       pre_bone.center)
                    
                transformed_direction = transform.transform_direction(
                        bone.direction)

                pitch_radians = transformed_direction.pitch
                    
                # Proximal Phalanx Yaw
                # Set metacarpals of index finger as reference coordinates
                index_finger = self.get_finger_by_type(hand, 'Index')

                pre_bone = index_finger.bone(0)
                transform = self.coordinates_transform(pre_bone.basis.x_basis,
                                                       pre_bone.basis.y_basis,
                                                       pre_bone.basis.z_basis,
                                                       pre_bone.center)
                    
                transformed_direction = transform.transform_direction(bone.direction)

                yaw_radians = transformed_direction.yaw

                bone_angles[finger_type][bone_type]['Pitch'] = pitch_radians
                bone_angles[finger_type][bone_type]['Yaw'] = yaw_radians

                pre_bone = finger.bone(1)
                for b in range(2, 4):
                    bone = finger.bone(b)
                    bone_type = self.bone_names[bone.type]
                    bone_angles[finger_type][bone_type] = {}

                    transform = self.coordinates_transform(pre_bone.basis.x_basis,
                                                           pre_bone.basis.y_basis,
                                                           pre_bone.basis.z_basis,
                                                           pre_bone.center)

                    transformed_direction = transform.transform_direction(bone.direction)
                    
                    pitch_radians = transformed_direction.pitch

                    bone_angles[finger_type][bone_type]['Pitch'] = pitch_radians

                    pre_bone = bone
            else:
                pre_bone = finger.bone(0)
                for b in range(1, 4):
                    bone = finger.bone(b)
                    bone_type = self.bone_names[bone.type]
                    bone_angles[finger_type][bone_type] = {}

                    transform = self.coordinates_transform(pre_bone.basis.x_basis,
                                                           pre_bone.basis.y_basis,
                                                           pre_bone.basis.z_basis,
                                                           pre_bone.center)

                    transformed_direction = transform.transform_direction(bone.direction)
                    # Pitch is the angle between the negative z-axis and the projection of
                    # the vector onto the y-z plane.

                    # If the vector points upward, the returned angle is between 0 and pi
                    # radians (180 degrees); if it points downward, the angle is between 0
                    # and -pi radians.
                    pitch_radians = transformed_direction.pitch

                    # Yaw is the angle between the negative z-axis and the projection of
                    # the vector onto the x-z plane.

                    # If the vector points to the right of the negative z-axis, then the
                    # returned angle is between 0 radians (180 degrees); if it points to
                    # the left, the angle is between 0 and -pi radians.
                    yaw_radians = transformed_direction.yaw

                    bone_angles[finger_type][bone_type]['Pitch'] = pitch_radians
                    if 'Proximal' == bone_type:
                        bone_angles[finger_type][bone_type]['Yaw'] = yaw_radians

                    pre_bone = bone
        return bone_angles

    def get_bone_length(self, hand):
        bone_length = {}
        for finger in hand.fingers:
            finger_type = self.finger_names[finger.type]
            bone_length[finger_type] = {}
            
            start_idx = 1 if finger_type == 'Thumb' else 0
            for b in range(start_idx, 4):
                bone = finger.bone(b)
                bone_type = self.bone_names[bone.type]
                bone_length[finger_type][bone_type] = bone.length
        return bone_length
    
    def get_joint_positions(self, hand):
        hand_transform = self.coordinates_transform(hand.basis.x_basis,
                                                    hand.basis.y_basis,
                                                    hand.basis.z_basis,
                                                    hand.arm.wrist_position)

        joint_positions = {}
        for finger in hand.fingers:
            finger_type = self.finger_names[finger.type]
            joint_positions[finger_type] = {}
            joint_positions[finger_type]['tip'] = hand_transform.transform_point(finger.joint_position(3)).to_tuple()
            
            if 'Thumb' == finger_type:
                joint_positions[finger_type]['ip'] = hand_transform.transform_point(finger.joint_position(2)).to_tuple()
                joint_positions[finger_type]['mcp'] = hand_transform.transform_point(finger.joint_position(1)).to_tuple()
                joint_positions[finger_type]['tm'] = hand_transform.transform_point(finger.bone(1).prev_joint).to_tuple()
            else:
                joint_positions[finger_type]['dip'] = hand_transform.transform_point(finger.joint_position(2)).to_tuple()
                joint_positions[finger_type]['pip'] = hand_transform.transform_point(finger.joint_position(1)).to_tuple()
                joint_positions[finger_type]['mcp'] = hand_transform.transform_point(finger.joint_position(0)).to_tuple()
                joint_positions[finger_type]['tm'] = hand_transform.transform_point(finger.bone(0).prev_joint).to_tuple()
        return joint_positions


if __name__ == '__main__':
    # Create a sample listener and controller
    listener = LeapCollector(False)
    controller = Leap.Controller()

    sample = 5000
    for i in range(sample):
        print i
        controller.add_listener(listener)

    listener.flag = True
    print "Start Collecting ..."
    start = time.time()
    print "start",start
    while(True):
        if keyboard.is_pressed('esc'):
            break

        controller.add_listener(listener)

    controller.remove_listener(listener)

    end = time.time()
    sample_fre = len(listener.sensor_data) / (end - start)
    print "end", end
    print "length", len(listener.sensor_data)
    print "sample_fre", sample_fre

    f = open("leap_data.json", 'w')
    for info in listener.sensor_data:
        str_info = json.dumps(info, sort_keys=False, indent=4, separators=(',', ':'))
        f.write(str_info)
    f.close()

    print "Data collection complete!"


























