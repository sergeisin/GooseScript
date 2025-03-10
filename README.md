# GooseScript
��������� ��� ���������� GOOSE ��������� � ������������ �� ���������� IEC61850-8-1.  

## �����������
- ���������� ��� ������ ������� �� ����� C#
- ������ � ����� ����� Goose-���������
- ����� ���� � ��������� ������ ��� ����������
- ������ ��� �������������� ����� ����������
- �������������� ���������� ��������� �������������
- ���������� cid-����� ��� �������� �� ����������� Goose

## ����������
- Windows 10 / 11
- [Microsoft .NET Framework 4.8](https://dotnet.microsoft.com/ru-ru/download/dotnet-framework/net48)
- [������� ������� Npcap       ](https://npcap.com/#download)

> [!NOTE]
> ������� Npcap ����� ���������������, ��� ��������� ��������� ������ WireShark

## ��� ������������
- ������������ ����� ��� ������� �� ����� C#, ��������� ��������������� ������ ��� ������ � Goose.
- ��� ������� �� ������ "Run Script" ��� ����� ������������� � ��������.
- � ���� ��������� ������������� ����������� �������� ����������� ����� C# (����������, �����, �������� � �.�.).
- � ������ ������������� ������ ����� �������� ��������������� ������������� ���������.

> [!TIP]
> �� ������ ������� ����� ����� ���� �� ��������, �������� ��� �� ���� ����.

## �������
��� �� ������ ������� ����� ���������� � �������� �������� � ���������

- [�������� ���� ��������������� �������               ](Examples/Manual.md)
- [������ ������ � ����� Boolean                       ](Examples/Script_Boolean.md)
- [������ ������ � ���������� ������ Int � Float       ](Examples/Script_Numeric.md)
- [������ ������ � ����� Dbpos (��������� �����������) ](Examples/Script_Dbpos.md)
- [������ ������ � ����� Octet64                       ](Examples/Script_Octets.md)
- [������ ������ � ����� Quality                       ](Examples/Script_Quality.md)
- [���������� ���������� Goose ������������            ](Examples/Script_TwoPublishers.md)

## �������� �������
```C#
// ������ ��� �������� �������� �����������
var settings = new GooseSettings()
{
    interfaceName = "Ethernet X",

    gocbRef = "IED1SYS/LLN0$GO$GSE1",
    datSet  = "IED1SYS/LLN0$DataSet",
    goId    = "MyGoose"
};

//
var publisher = new GoosePublisher(settings);
publisher.Send();
```

������ ������ GooseSettings �������� ��������� ��������, ����������� �� ������� �������� �������������.
��� ������������ ��������� ��������� � ���� ����.
������ �������� ��������� � ������� 'Manual'.

## ��������� ������ ������

### MMStypes
��������� ��������� ����� ������ � ������������� ���������  
��� ���� DO ���� ������� ������ DA (stVal + q + t).

### MMS types
|MMS_TYPE     |     Value    |
|:------------|:------------:|
|BOOLEAN      | true / false |
|BIT_STRING   | "011010"     |
|INT32        | -42          |
|INT32U       | 404          |
|FLOAT32      | 3.14         |
|OCTET_STRING | "c0ffee"     |

## �������� ��������
- Ctrl + MouseWheel: ��������� �������� ������
- Ctrl + X: �������� ������� ������
- ��� �������� ���� ���� ��� ����������� � GooseScript.cs
- ��� �������� ������ �� ���� ��������� ����� ����������� ����� ������������ ������ �� ���������.

## �������� �� ����������� Goose
��� �������� �� ����������� ���������� ���� ��� ��������
- ������ �������� �� ������, ������������� � WireShark
- �������� � ������� cid ����� � ������������� ICT

���������� cid-�����:
```C#
publisher.SaveSCL("IED1");
```
����� ��������, ��� ���������� ��������� ������ � ��� ������, ���� ���������� ���� ��������
gocbRef � datSet ����� ����� ���������:
- ������ ���������� � ����� IED
- � �������� DO ������������ LLN0

## BOOLEAN
```C#
pub.Value = true;
...
pub.Value = !publisher.Value;
```

## INT32 - INT32U - FLOAT32
```C#
publisher.Value = 42;
...
publisher.Value++;
...
publisher.Value += 1.5;
```

## BIT_STRING
```C#
// Dbpos
pub.Value = Dbpos.Intermediate;
pub.Value = Dbpos.Off
pub.Value = Dbpos.On;
pub.Value = Dbpos.BadState;

// Raw
pub.Value =       "1_0101"; // bits:  5, padding: 3
pub.Value =      "01_0101"; // bits:  6, padding: 2
pub.Value =     "101_0101"; // bits:  7, padding: 1
pub.Value =    "0101_0101"; // bits:  8, padding: 0
pub.Value =  "1_0101_0101"; // bits:  9, padding: 7
pub.Value = "01_0101_0101"; // bits: 10, padding: 6
```

## OCTET_STRING
```C#
publisher.Value = "DEAD_BEEF";
.
publisher.Value += "BAAD_FOOD";
```

## Runtime operations with 'Quality' data attribute
```C#
publisher.Quality = new Quality()
{
    Validity = Validity.Invalid,
    Test = true,
    OperatorBlocked = true
};
```

## Send single message
```C#
publisher.Send();
```

## Send few messages with delay
```C#
publisher.SendFew(count, sleepTime);
```

## Retransmission mechanism
```C#
publisher.Run(minTime, maxTime);

while(true)
{
    // Some work with stVal
    Timer.Sleep(1000);
}
```

## Direct counters update (unsafe)
```C#
while(loop)
{
    pub.Send();
    pub.StNum = my_stNum;
    pub.SqNum = my_sqNum;
    pub.TAL   = my_TAL_ms;
}
```

## Scripting

```C#
pub.mmsType = MMS_TYPE.OCTET_STRING
pub.Value = "c0ffee"
```

## Run publishing
```C#
publisher.Run(minTime: 10, maxTime: 1000);
```

DataAttribute - stVal   (MMS_TYPE)
DataAttribute - q       (Quality)

BOOLEAN / BIT_STRING / INT32 / INT32U / FLOAT32 / OCTET_STRING

## ��������������� ������
��� ������ ����� ������������ Microsoft Visual�Studio Community

* ����������� ������ github.com/sergeisin/GooseScript
* �������� ����������� NuGet (SharpPcap) 
* ������� Release
* ��� ��������� ������ ������������ ����� ����� ������������ ilMerge

```Batchfile
.\ILMerge /target:winexe /out:Out.exe ^
GooseScript.exe ^
PacketDotNet.dll ^
SharpPcap.dll ^
System.Buffers.dll ^
System.Memory.dll ^
System.Numerics.Vectors.dll ^
System.Runtime.CompilerServices.Unsafe.dll ^
System.Text.Encoding.CodePages.dll

del /q GooseScript.exe
del /q Out.pdb

ren Out.exe GooseScript.exe
```

## �����������
- ������ ��������� ����� ������. ��������, �������� ��������� DO.
- ���� ������������ (.cid) ���� �������� ������������� ����� SCL � �� �������� ���������.

## �����
- [@Sergey Sinitcyn](https://github.com/sergeisin)
- <u>sergei28.01.1994@gmail.com</u>

## ����������
��������� ����� ���� ������� ������������� � ���������� ��������� ���,
� ���-�� ������ ������������, ������������ �������� ����� � ���������� GOOSE-���������.

## �������� ����������
- �������� ����� ��������� � ��������� ��������� ������
- �������� ����������� ����� ��������� ����� ������ (BOOLEAN / INT32 / Dbpos)
- ���������� ping-pong ������������ ������������������ Goose
- �������� ����������� ����� ���������� � ������� ���������� VlanID
- �������� ������ IED ��� ������������ ��������� stNum ��� sqNum
- �������� ������ IED ��� ����� �������������� Goose-���������
- �������� ������������ ���������� � ������������ (PIXIT)
