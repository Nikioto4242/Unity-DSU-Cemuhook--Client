using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using Force.Crc32;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine.Events;

public class DSUClient : MonoBehaviour
{
    public class Unofficial//todo
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct In_ControllerMotorInfo
        {
            [FieldOffset(0)]
            public ControllerIdentificationHeader controllerIdentificationHeader;
        }
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct Out_Full_ControllerMotorInfo
        {
            [FieldOffset(0)]
            public Header header;
            [FieldOffset(20)]
            public SharedResponseBeginning complex;
            [FieldOffset(31)]
            public byte motorCount;
        }
    }
    public class Out
    {
        [StructLayout(LayoutKind.Explicit, Size = 100)]
        public struct Full_ActualControllerData
        {
            // Header section
            [FieldOffset(0)]
            public Header header;

            // Pad meta section
            [FieldOffset(20)]
            public SharedResponseBeginning sharedResponse;
            [FieldOffset(31)]
            public byte isControllerConnected;
            [FieldOffset(32)]
            public uint packetNumber;

            // Primary controls
            [FieldOffset(36)]
            public byte buttonsBitmask1;//D-Pad Left, D-Pad Down, D-Pad Right, D-Pad Up, Options (?), R3, L3, Share (?)
            [FieldOffset(37)]
            public byte buttonsBitmask2;//Y, B, A, X, R1, L1, R2, L2
            [FieldOffset(38)]
            public byte homeButton;
            [FieldOffset(39)]
            public byte touchButton;
            [FieldOffset(40)]
            public byte lx;// l - left stick 
            [FieldOffset(41)]
            public byte ly;
            [FieldOffset(42)]
            public byte rx;
            [FieldOffset(43)]
            public byte ry;
            [FieldOffset(44)]
            public byte analogDpadLeft;
            [FieldOffset(45)]
            public byte analogDpadDown;
            [FieldOffset(46)]
            public byte analogDpadRight;
            [FieldOffset(47)]
            public byte analogDpadUp;
            [FieldOffset(48)]
            public byte analogY;//square
            [FieldOffset(49)]
            public byte analogB;//cross
            [FieldOffset(50)]
            public byte analogA;//circle
            [FieldOffset(51)]
            public byte analogX;//triangle
            [FieldOffset(52)]
            public byte analogR1;
            [FieldOffset(53)]
            public byte analogL1;
            [FieldOffset(54)]
            public byte analogR2;
            [FieldOffset(55)]
            public byte analogL2;

            // Touch 1
            [FieldOffset(56)]
            public TouchData touch1;

            // Touch 2
            [FieldOffset(62)]
            public TouchData touch2;

            // Accel
            [FieldOffset(68)]
            public ulong totalMicroSec;//update only with accelerometer (but not gyro only) changes
            [FieldOffset(76)]
            public float accelXG;
            [FieldOffset(80)]
            public float accelYG;
            [FieldOffset(84)]
            public float accelZG;

            // Gyro
            [FieldOffset(88)]
            public float angVelPitch;
            [FieldOffset(92)]
            public float angVelYaw;
            [FieldOffset(96)]
            public float angVelRoll;
        }
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        public struct Full_ConnectedControllersInfo
        {
            [FieldOffset(0)]
            public Header header;
            [FieldOffset(20)]
            public SharedResponseBeginning sharedResponse;
            [FieldOffset(31)]
            public byte zeroByte;
        }
    }
    public class In
    {
        public static readonly byte[] ConnectedControllersInfo = new byte[8] { 4, 0, 0, 0, 0, 1, 2, 3 };//give info about all controllers
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct ActualControllerData
        {
            [FieldOffset(0)]
            public ControllerIdentificationHeader controllerIdentificationHeader;
        }
    }





    [StructLayout(LayoutKind.Explicit, Size = 6)]
    public struct TouchData
    {
        [FieldOffset(0)]
        public byte active;
        [FieldOffset(1)]
        public byte packetId;
        [FieldOffset(2)]
        public ushort X;
        [FieldOffset(4)]
        public ushort Y;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public unsafe struct ControllerIdentificationHeader
    {
        [FieldOffset(0)]
        public RegistraitonType registraitonType;
        [FieldOffset(1)]
        public byte slotToReportAbout;
        [FieldOffset(2)]
        public fixed byte macToReportAbout[6];
    }
    public enum RegistraitonType : byte
    {
        SubscribeToAll = 0,
        SlotBased = 1,
        MacBased = 2
    }


    [StructLayout(LayoutKind.Explicit, Size = 11)]
    public unsafe struct SharedResponseBeginning
    {
        [FieldOffset(0)]
        public byte slot;
        [FieldOffset(1)]
        public SlotState slotState;
        [FieldOffset(2)]
        public DeviceModel deviceModel;
        [FieldOffset(3)]
        public ConnectionType connectionType;
        [FieldOffset(4)]
        public fixed byte macAddress[6];
        [FieldOffset(10)]
        public BatteryStatus batteryStatus;
    }
    public enum SlotState : byte
    {
        NotConnected = 0,
        Reserved = 1,
        Connected = 2
    }
    public enum DeviceModel : byte
    {
        NotApplicable = 0,
        NoOrPartialGyro = 1,//ds3
        FullGyro = 2,//ds4
        VrProbably = 3//generic gamepad
    }
    public enum ConnectionType : byte
    {
        NotApplicable = 0,
        USB = 1,
        Bluetooth = 2
    }
    public enum BatteryStatus : byte
    {
        None = 0x00,
        Dying = 0x01,
        Low = 0x02,
        Medium = 0x03,
        High = 0x04,
        Full = 0x05,
        Charging = 0xEE,
        Charged = 0xEF
    };


    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public unsafe struct Header
    {
        [FieldOffset(0)]
        public fixed byte magicString[4];
        [FieldOffset(4)]
        public ushort protocolVersion;
        [FieldOffset(6)]
        public ushort packetDataLength;//without header (includes messageType)
        [FieldOffset(8)]
        public int CRC32;
        [FieldOffset(12)]
        public uint clientID;
        [FieldOffset(16)]
        public uint messageType;
    }
    public enum MessageType : uint
    {
        ProtocolVersion = 0x100000,
        ControllerInfo = 0x100001,
        ActualControllerData = 0x100002,
        ControllerMotorInfo = 0x110001,//unofficial
        RumbleControllerMotor = 0x110002//unofficial
    };



    public class ControllerData
    {
        public static readonly ControllerData[] controllerSlots = new ControllerData[4];

        public byte slot = 0;
        public SlotState slotState = SlotState.NotConnected;
        public DeviceModel deviceModel = DeviceModel.NotApplicable;
        public ConnectionType connectionType = ConnectionType.NotApplicable;
        public readonly byte[] macAddress = new byte[6];
        public BatteryStatus batteryStatus = BatteryStatus.None;

        public Out.Full_ActualControllerData controllerData = new Out.Full_ActualControllerData();
    }
    public static DSUClient Instance;
    public readonly UnityEvent[] onDataReceived = new UnityEvent[4] {
    new UnityEvent(),
    new UnityEvent(),
    new UnityEvent(),
    new UnityEvent()
    };
    public NewUDPReceive udpManager;
    public string packetDataPath = "";
    public Deb debug;
    public bool recieveOnlyLatestPackets = true;//todo
    private uint clientID;

    

    private void Awake()
    {
        //clientID = (uint)UnityEngine.Random.Range(uint.MinValue, uint.MaxValue);
        clientID = 286331153;//for simplicity

        udpManager.SetCallback(Receive);
        Instance = this;
    }

    private void Receive(byte[] receivedBytes)
    {
        debug.WriteReceived(receivedBytes);
        try
        {
            if (receivedBytes[0] != 'D' || receivedBytes[1] != 'S' || receivedBytes[2] != 'U' || receivedBytes[3] != 'S')
            {
                Debug.LogError("error: wrong magic string");
                return;
            }
            ushort packetDataLength = BitConverter.ToUInt16(receivedBytes, 6);
            if (receivedBytes.Length - 16 < packetDataLength)//drop packet if too short
            {
                Debug.LogError(
                    "dropped packet because it was too short\n" +
                    $"received: {receivedBytes.Length} bytes\n" +
                    $"should've received: {packetDataLength} bytes");
                return;
            }
            if (receivedBytes.Length - 16 > packetDataLength)//truncate packet if too long
            {
                Array.Resize(ref receivedBytes, packetDataLength);
                print("packet has been truncated");
            }
            if (PacketCRCCheck(receivedBytes) == false)
            {
                Debug.LogError("packet didn't pass the crc check");
                return;
            }
            var messageType = (MessageType)BitConverter.ToUInt32(receivedBytes, 16);
            switch (messageType)
            {
                case MessageType.ProtocolVersion:
                    break;
                case MessageType.ControllerInfo:
                    {
                        var a = BytesToStruct<Out.Full_ConnectedControllersInfo>(receivedBytes);
                        var rsr = a.sharedResponse;
                        print(
                            $"messageType: ConnectedControllersInfo\n" +
                            $"slot: {rsr.slot}\n" +
                            $"slotState: {rsr.slotState}\n" +
                            $"deviceModel: {rsr.deviceModel}");
                        HandleSharedResponse(rsr, null);


                    }
                    break;
                case MessageType.ActualControllerData:
                    {
                        var a = BytesToStruct<Out.Full_ActualControllerData>(receivedBytes);
                        //print(
                        //    $"messageType: ActualControllerData\n" +
                        //    $"slot: {a.sharedResponse.slot}\n" +
                        //    $"slotState: {a.sharedResponse.slotState}\n" +
                        //    $"deviceModel: {a.sharedResponse.deviceModel}\n" +
                        //    $"accel x: {a.accelXG}");
                        //LogStruct(a);
                        HandleSharedResponse(a.sharedResponse, a);
                    }
                    break;
                case MessageType.ControllerMotorInfo:
                    {
                        var a = BytesToStruct<Unofficial.Out_Full_ControllerMotorInfo>(receivedBytes);
                        print(
                            $"messageType: {a.header.messageType}\n" +
                            $"slot: {a.complex.slot}\n" +
                            $"motorCount: {a.motorCount}");
                    }
                    break;
                case MessageType.RumbleControllerMotor:
                    Debug.LogError("somehow received RumbleControllerMotor message (shouldn't be possible)");
                    break;
                default:
                    Debug.LogError("unsupported message type: " + messageType);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("message receive error: " + ex);
        }
    }
    unsafe private ControllerData HandleSharedResponse(SharedResponseBeginning rsr, Out.Full_ActualControllerData? controllerData)
    {
        ControllerData b = ControllerData.controllerSlots[rsr.slot];
        if (b == null)
        {
            b = new ControllerData();
        }
        var currentSlot = rsr.slot;
        ControllerData.controllerSlots[currentSlot] = b;

        b.slotState = rsr.slotState;
        b.slot = currentSlot;
        b.macAddress[0] = rsr.macAddress[0];
        b.macAddress[1] = rsr.macAddress[1];
        b.macAddress[2] = rsr.macAddress[2];
        b.macAddress[3] = rsr.macAddress[3];
        b.macAddress[4] = rsr.macAddress[4];
        b.macAddress[5] = rsr.macAddress[5];
        b.deviceModel = rsr.deviceModel;
        b.connectionType = rsr.connectionType;
        b.batteryStatus = rsr.batteryStatus;

        if (controllerData.HasValue)
        {
            b.controllerData = controllerData.Value;
            onDataReceived[currentSlot].Invoke();
        }

        return b;
    }
    unsafe private byte[] GenerateHeaderNew(byte[] packetBytes, MessageType messageType)
    {
        var a = new Header();

        a.magicString[0] = (byte)'D';
        a.magicString[1] = (byte)'S';
        a.magicString[2] = (byte)'U';
        a.magicString[3] = (byte)'C';

        a.protocolVersion = 1001;
        a.packetDataLength = (ushort)(packetBytes.Length + 4);
        a.CRC32 = 0;
        a.clientID = clientID;
        a.messageType = (uint)messageType;

        return StructToBytes(a);
    }
    private void AddCRC(byte[] wholePacket)
    {
        uint crc = Crc32Algorithm.Compute(wholePacket);
        Array.Copy(BitConverter.GetBytes(crc), 0, wholePacket, 8, 4);
    }
    private bool PacketCRCCheck(byte[] wholePacket)
    {
        var bytes = (byte[])wholePacket.Clone();
        var CRC32 = BitConverter.ToUInt32(bytes, 8);
        bytes[8] = 0;
        bytes[9] = 0;
        bytes[10] = 0;
        bytes[11] = 0;
        uint realCRC32 = Crc32Algorithm.Compute(bytes);
        if (realCRC32 == CRC32)
            return true;
        return false;
    }
    private void SendPacket(byte[] packetData, MessageType messageType)
    {
        byte[] header = GenerateHeaderNew(packetData, messageType);
        byte[] finalPacket = header.Concat(packetData).ToArray();
        AddCRC(finalPacket);
        udpManager.SendData(finalPacket);
        //debug.WriteSent(finalPacket);
    }
    public void SendActualControllerDataRequest(byte slot)
    {
        var a = new In.ActualControllerData();
        a.controllerIdentificationHeader.registraitonType = RegistraitonType.SlotBased;
        a.controllerIdentificationHeader.slotToReportAbout = slot;
        byte[] packetData = StructToBytes(a);
        SendPacket(packetData, MessageType.ActualControllerData);
    }




    public static byte[] StructToBytes<T>(T structure) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] bytes = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(structure, ptr, false);
        Marshal.Copy(ptr, bytes, 0, size);
        Marshal.FreeHGlobal(ptr);

        return bytes;
    }
    public static T BytesToStruct<T>(byte[] bytes) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(bytes, 0, ptr, size);
        T structure = Marshal.PtrToStructure<T>(ptr);
        Marshal.FreeHGlobal(ptr);

        return structure;
    }


    public static void LogStruct(object a)
    {
        var result = "";
        foreach (var fieldInfo in a.GetType().GetFields())
        {
            var value = fieldInfo.GetValue(a);
            result += $"{fieldInfo.Name}: {value}\n";
        }
        print(result);
    }



    [InspectorButton("OnButtonClicked")]
    public bool connectedControllersInfo;
    private void OnButtonClicked()
    {
        //byte[] packetData = File.ReadAllBytes(packetDataPath);
        byte[] packetData = In.ConnectedControllersInfo;
        SendPacket(packetData, MessageType.ControllerInfo);
    }
    [InspectorButton("OnButtonClicked1")]
    public bool sendMotorInfoRequest;
    private void OnButtonClicked1()
    {
        var a = new Unofficial.In_ControllerMotorInfo();
        a.controllerIdentificationHeader.registraitonType = RegistraitonType.SlotBased;
        a.controllerIdentificationHeader.slotToReportAbout = 0;
        byte[] packetData = StructToBytes(a);
        SendPacket(packetData, MessageType.ControllerMotorInfo);
    }

    

    [InspectorButton("OnButtonClicked2")]
    public bool sendActualControllerDataRequest;
    private void OnButtonClicked2()
    {
        SendActualControllerDataRequest(0);
    }
}
