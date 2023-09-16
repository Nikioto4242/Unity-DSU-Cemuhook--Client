# Unity DSU(Cemuhook) Client
 Lets you easily stream **gyroscope** and **accelerometer** data from your device to pc over the local network (also buttons, analog buttons, touches etc. depends on your device and DSU server you're using).  
Note: you'll have to filter the data and do the calculations yourself.
  
## Compactible devices:
- Android phone
- Dualshock 4
- Dualshock 3
- Steam controller
- Nintendo Switch controllers 
- Nintendo Wii Remotes with Wii MotionPlus
- Probably more in future (basically everything that can act as a DSU server)   

Guide: https://cemuhook.sshnuke.net/padudpserver.html
## How to use?
- Step 1: Your unity project should have instances of *New UDP Receive*, *DSU Client* and *Controller Data Requester*.  
- Step 2: Set the device's port and ip address into *New UDP Receive* (you can also change the local port)  
- Step 3: Start receive using the *Controller Data Requester* component.  
- Step 4: Access device data: DSUClient.ControllerData.controllerSlots[slot];
- Tip: There's an example of using and accessing that data: *dsuDataVisualizer* in the sample scene.
